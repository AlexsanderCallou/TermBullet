namespace TermBullet.Application.Ai;

public sealed record AiPlanningDraftValidationResult(IReadOnlyList<string> Errors)
{
    public bool IsValid => Errors.Count == 0;
}
