using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using TermBullet.Application.Ai;
using TermBullet.Services.Configuration;

namespace TermBullet.Services.Ai;

public sealed class OpenAiCompatiblePlanningProvider(
    HttpClient httpClient,
    AiProfile profile) : IAiPlanningProvider
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public async Task<AiPlanningProviderResponse> SendAsync(
        AiPlanningModelRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var endpoint = BuildEndpoint(profile);
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint);
        ApplyAuthorization(httpRequest, profile);

        var body = new ChatCompletionRequest(
            profile.Model,
            request.Messages.Select(ToProviderMessage).ToArray(),
            request.RequireStructuredDraft ? new ResponseFormat("json_object") : null,
            request.MaxOutputTokens ?? (request.RequireStructuredDraft ? 700 : 300),
            0.2);
        var json = JsonSerializer.Serialize(body, JsonOptions);
        httpRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");

        HttpResponseMessage response;
        try
        {
            response = await httpClient.SendAsync(httpRequest, cancellationToken);
        }
        catch (OperationCanceledException exception)
        {
            throw new InvalidOperationException("AI provider request timed out or was cancelled.", exception);
        }

        using (response)
        {
            var responseText = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"AI provider request failed ({(int)response.StatusCode}): {TrimForError(responseText)}");
            }

            return ParseResponse(responseText, profile.Model);
        }
    }

    private static Uri BuildEndpoint(AiProfile profile)
    {
        if (string.IsNullOrWhiteSpace(profile.BaseUrl))
        {
            throw new InvalidOperationException("AI profile base_url is required for openai-compatible provider.");
        }

        var baseUrl = profile.BaseUrl.TrimEnd('/');
        return new Uri($"{baseUrl}/chat/completions", UriKind.Absolute);
    }

    private static void ApplyAuthorization(HttpRequestMessage request, AiProfile profile)
    {
        var keySource = string.IsNullOrWhiteSpace(profile.ApiKeySource)
            ? "environment"
            : profile.ApiKeySource.Trim();
        if (string.Equals(keySource, "none", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (string.Equals(keySource, "literal", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(profile.ApiKey))
            {
                throw new InvalidOperationException("AI profile api_key is required for literal API key source.");
            }

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", profile.ApiKey);
            return;
        }

        if (!string.Equals(keySource, "environment", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Unsupported API key source: {profile.ApiKeySource}");
        }

        if (string.IsNullOrWhiteSpace(profile.ApiKeyEnv))
        {
            throw new InvalidOperationException("AI profile api_key_env is required for environment API key source.");
        }

        var apiKey = Environment.GetEnvironmentVariable(profile.ApiKeyEnv);
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException($"API key environment variable is not set: {profile.ApiKeyEnv}");
        }

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
    }

    private static ProviderMessage ToProviderMessage(AiPlanningMessage message)
    {
        var role = message.Role switch
        {
            AiPlanningMessageRole.Agent => "system",
            AiPlanningMessageRole.Context => "system",
            AiPlanningMessageRole.User => "user",
            AiPlanningMessageRole.Assistant => "assistant",
            _ => throw new ArgumentOutOfRangeException(nameof(message), message.Role, "Unsupported AI planning message role.")
        };

        return new ProviderMessage(role, message.Content);
    }

    private static AiPlanningProviderResponse ParseResponse(string responseText, string fallbackModel)
    {
        ChatCompletionResponse? document;
        try
        {
            document = JsonSerializer.Deserialize<ChatCompletionResponse>(responseText, JsonOptions);
        }
        catch (JsonException exception)
        {
            throw new InvalidOperationException("AI provider response is malformed JSON.", exception);
        }

        var content = document?.Choices?.FirstOrDefault()?.Message?.Content;
        if (content is null)
        {
            throw new InvalidOperationException("AI provider response is malformed: missing choices[0].message.content.");
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new InvalidOperationException("AI provider response content is empty.");
        }

        return new AiPlanningProviderResponse(content.Trim(), document?.Model ?? fallbackModel);
    }

    private static string TrimForError(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return "(empty response)";
        }

        var trimmed = text.Trim();
        return trimmed.Length <= 500 ? trimmed : trimmed[..500];
    }

    private sealed record ChatCompletionRequest(
        string Model,
        IReadOnlyList<ProviderMessage> Messages,
        ResponseFormat? ResponseFormat,
        int MaxTokens,
        double Temperature);

    private sealed record ResponseFormat(string Type);

    private sealed record ProviderMessage(
        string Role,
        string Content);

    private sealed class ChatCompletionResponse
    {
        public string? Model { get; set; }

        public List<Choice>? Choices { get; set; }
    }

    private sealed class Choice
    {
        public ResponseMessage? Message { get; set; }
    }

    private sealed class ResponseMessage
    {
        public string? Content { get; set; }
    }
}
