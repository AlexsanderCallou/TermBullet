namespace TermBullet.Application.Ai;

public static class AiPlanningDraftIntent
{
    private static readonly string[] DraftActionTerms =
    [
        "create",
        "crie",
        "criar",
        "add",
        "adicione",
        "adicionar",
        "generate",
        "gere",
        "gerar",
        "build",
        "monte",
        "montar",
        "planeje",
        "planejar",
        "faça",
        "faca"
    ];

    private static readonly string[] DraftObjectTerms =
    [
        "task",
        "tasks",
        "tarefa",
        "tarefas",
        "plano",
        "planejamento",
        "roadmap",
        "draft"
    ];

    public static bool RequiresStructuredDraft(string prompt)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            return false;
        }

        var normalized = prompt.Trim().ToLowerInvariant();
        return DraftActionTerms.Any(term => normalized.Contains(term, StringComparison.Ordinal))
            && DraftObjectTerms.Any(term => normalized.Contains(term, StringComparison.Ordinal));
    }
}
