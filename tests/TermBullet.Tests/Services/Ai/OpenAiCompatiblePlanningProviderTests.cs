using System.Net;
using System.Text.Json;
using TermBullet.Application.Ai;
using TermBullet.Services.Ai;
using TermBullet.Services.Configuration;

namespace TermBullet.Tests.Services.Ai;

public sealed class OpenAiCompatiblePlanningProviderTests
{
    [Fact]
    public async Task SendAsync_posts_chat_completion_request_and_returns_content()
    {
        using var handler = new CapturingHandler(
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent(
                    """
                    {
                      "choices": [
                        {
                          "message": {
                            "content": "{\"summary\":\"Draft ready\"}"
                          }
                        }
                      ]
                    }
                    """)
            });
        using var httpClient = new HttpClient(handler);
        var profile = new AiProfile(
            Provider: "openai-compatible",
            Model: "llama3.1",
            BaseUrl: "http://localhost:11434/v1",
            ApiKeySource: "none");
        var provider = new OpenAiCompatiblePlanningProvider(httpClient, profile);

        var response = await provider.SendAsync(CreateRequest());

        Assert.Equal("{\"summary\":\"Draft ready\"}", response.Content);
        Assert.Equal("llama3.1", response.Model);
        Assert.NotNull(handler.Request);
        Assert.Equal(HttpMethod.Post, handler.Request.Method);
        Assert.Equal("http://localhost:11434/v1/chat/completions", handler.Request.RequestUri?.ToString());
        Assert.Null(handler.Request.Headers.Authorization);

        var body = Assert.IsType<string>(handler.RequestBody);
        using var document = JsonDocument.Parse(body);
        Assert.Equal("llama3.1", document.RootElement.GetProperty("model").GetString());
        var messages = document.RootElement.GetProperty("messages");
        Assert.Equal("system", messages[0].GetProperty("role").GetString());
        Assert.Equal("agent prompt", messages[0].GetProperty("content").GetString());
        Assert.Equal("system", messages[1].GetProperty("role").GetString());
        Assert.Equal("context", messages[1].GetProperty("content").GetString());
        Assert.Equal("user", messages[2].GetProperty("role").GetString());
        Assert.Equal("Plan Java studies.", messages[2].GetProperty("content").GetString());
        var responseFormat = document.RootElement.GetProperty("response_format");
        Assert.Equal("json_object", responseFormat.GetProperty("type").GetString());
        Assert.Equal(700, document.RootElement.GetProperty("max_tokens").GetInt32());
        Assert.Equal(0.2, document.RootElement.GetProperty("temperature").GetDouble());
    }

    [Fact]
    public async Task SendAsync_adds_bearer_token_from_environment_key_source()
    {
        var envName = $"TERMBULLET_TEST_KEY_{Guid.NewGuid():N}";
        Environment.SetEnvironmentVariable(envName, "test-key");
        try
        {
            using var handler = new CapturingHandler(
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent(
                        """
                        {
                          "choices": [
                            { "message": { "content": "ok" } }
                          ]
                        }
                        """)
                });
            using var httpClient = new HttpClient(handler);
            var profile = new AiProfile(
                Provider: "openai-compatible",
                Model: "gpt-4.1-mini",
                BaseUrl: "https://api.example.test/v1",
                ApiKeySource: "environment",
                ApiKeyEnv: envName);
            var provider = new OpenAiCompatiblePlanningProvider(httpClient, profile);

            await provider.SendAsync(CreateRequest());

            Assert.Equal("Bearer", handler.Request?.Headers.Authorization?.Scheme);
            Assert.Equal("test-key", handler.Request?.Headers.Authorization?.Parameter);
        }
        finally
        {
            Environment.SetEnvironmentVariable(envName, null);
        }
    }

    [Fact]
    public async Task SendAsync_does_not_force_json_for_conversation_requests()
    {
        using var handler = new CapturingHandler(
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent(
                    """
                    {
                      "choices": [
                        { "message": { "content": "Tell me more about the outcome." } }
                      ]
                    }
                    """)
            });
        using var httpClient = new HttpClient(handler);
        var provider = new OpenAiCompatiblePlanningProvider(httpClient, LocalProfile());

        await provider.SendAsync(CreateRequest(requireStructuredDraft: false));

        using var document = JsonDocument.Parse(Assert.IsType<string>(handler.RequestBody));
        Assert.False(document.RootElement.TryGetProperty("response_format", out _));
        Assert.Equal(300, document.RootElement.GetProperty("max_tokens").GetInt32());
    }

    [Fact]
    public async Task SendAsync_uses_request_max_output_tokens_when_provided()
    {
        using var handler = new CapturingHandler(
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent(
                    """
                    {
                      "choices": [
                        { "message": { "content": "OK" } }
                      ]
                    }
                    """)
            });
        using var httpClient = new HttpClient(handler);
        var provider = new OpenAiCompatiblePlanningProvider(httpClient, LocalProfile());

        await provider.SendAsync(CreateRequest(requireStructuredDraft: false, maxOutputTokens: 8));

        using var document = JsonDocument.Parse(Assert.IsType<string>(handler.RequestBody));
        Assert.Equal(8, document.RootElement.GetProperty("max_tokens").GetInt32());
    }

    [Fact]
    public async Task SendAsync_maps_assistant_history_messages()
    {
        using var handler = new CapturingHandler(
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent(
                    """
                    {
                      "choices": [
                        { "message": { "content": "OK" } }
                      ]
                    }
                    """)
            });
        using var httpClient = new HttpClient(handler);
        var provider = new OpenAiCompatiblePlanningProvider(httpClient, LocalProfile());

        await provider.SendAsync(new AiPlanningModelRequest(
            AiPlanningMode.NewProject,
            Tag: null,
            Messages:
            [
                new(AiPlanningMessageRole.Agent, "agent prompt"),
                new(AiPlanningMessageRole.User, "Create a roadmap."),
                new(AiPlanningMessageRole.Assistant, "Roadmap ready."),
                new(AiPlanningMessageRole.User, "Create the tasks.")
            ],
            ContextItems: [],
            RequireStructuredDraft: false));

        using var document = JsonDocument.Parse(Assert.IsType<string>(handler.RequestBody));
        var messages = document.RootElement.GetProperty("messages");
        Assert.Equal("assistant", messages[2].GetProperty("role").GetString());
        Assert.Equal("Roadmap ready.", messages[2].GetProperty("content").GetString());
    }

    [Fact]
    public async Task SendAsync_rejects_missing_environment_api_key()
    {
        using var httpClient = new HttpClient(new CapturingHandler(new HttpResponseMessage(HttpStatusCode.OK)));
        var profile = new AiProfile(
            Provider: "openai-compatible",
            Model: "gpt-4.1-mini",
            BaseUrl: "https://api.example.test/v1",
            ApiKeySource: "environment",
            ApiKeyEnv: $"TERMBULLET_MISSING_{Guid.NewGuid():N}");
        var provider = new OpenAiCompatiblePlanningProvider(httpClient, profile);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => provider.SendAsync(CreateRequest()));

        Assert.Contains("API key environment variable is not set", exception.Message);
    }

    [Fact]
    public async Task SendAsync_rejects_http_error_response()
    {
        using var handler = new CapturingHandler(
            new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = JsonContent("""{"error":{"message":"bad request"}}""")
            });
        using var httpClient = new HttpClient(handler);
        var provider = new OpenAiCompatiblePlanningProvider(httpClient, LocalProfile());

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => provider.SendAsync(CreateRequest()));

        Assert.Contains("AI provider request failed", exception.Message);
        Assert.Contains("400", exception.Message);
    }

    [Fact]
    public async Task SendAsync_rejects_empty_response_content()
    {
        using var handler = new CapturingHandler(
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent("""{"choices":[{"message":{"content":" "}}]}""")
            });
        using var httpClient = new HttpClient(handler);
        var provider = new OpenAiCompatiblePlanningProvider(httpClient, LocalProfile());

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => provider.SendAsync(CreateRequest()));

        Assert.Contains("empty", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SendAsync_rejects_malformed_response()
    {
        using var handler = new CapturingHandler(
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent("""{"unexpected":true}""")
            });
        using var httpClient = new HttpClient(handler);
        var provider = new OpenAiCompatiblePlanningProvider(httpClient, LocalProfile());

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => provider.SendAsync(CreateRequest()));

        Assert.Contains("malformed", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SendAsync_reports_timeout_when_request_is_cancelled()
    {
        using var handler = new CancellingHandler();
        using var httpClient = new HttpClient(handler);
        var provider = new OpenAiCompatiblePlanningProvider(httpClient, LocalProfile());
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => provider.SendAsync(CreateRequest(), cts.Token));

        Assert.Contains("timed out or was cancelled", exception.Message);
    }

    private static AiProfile LocalProfile() =>
        new(
            Provider: "openai-compatible",
            Model: "llama3.1",
            BaseUrl: "http://localhost:11434/v1",
            ApiKeySource: "none");

    private static AiPlanningModelRequest CreateRequest(
        bool requireStructuredDraft = true,
        int? maxOutputTokens = null) =>
        new(
            AiPlanningMode.NewProject,
            Tag: "estudo-java",
            Messages:
            [
                new(AiPlanningMessageRole.Agent, "agent prompt"),
                new(AiPlanningMessageRole.Context, "context"),
                new(AiPlanningMessageRole.User, "Plan Java studies.")
            ],
            ContextItems: [],
            RequireStructuredDraft: requireStructuredDraft,
            MaxOutputTokens: maxOutputTokens);

    private static StringContent JsonContent(string json) =>
        new(json, System.Text.Encoding.UTF8, "application/json");

    private sealed class CapturingHandler(HttpResponseMessage response) : HttpMessageHandler
    {
        public HttpRequestMessage? Request { get; private set; }

        public string? RequestBody { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Request = request;
            RequestBody = request.Content?.ReadAsStringAsync(cancellationToken).GetAwaiter().GetResult();
            return Task.FromResult(response);
        }
    }

    private sealed class CancellingHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return Task.FromCanceled<HttpResponseMessage>(cancellationToken);
        }
    }
}
