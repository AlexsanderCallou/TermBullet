namespace TermBullet.Application.Ai;

public static class AiPlanningDraftNormalizer
{
    public static string? NormalizeCollection(string? value)
    {
        var normalized = Normalize(value);
        return normalized switch
        {
            "today" or "todays" or "today_tasks" or "today-task" => "today",
            "week" or "weekly" or "this_week" or "this-week" or "this week" or "current_week" or "current-week" => "week",
            "month" or "monthly" or "this_month" or "this-month" or "this month" or "current_month" or "current-month" => "month",
            "backlog" or "later" or "future" => "backlog",
            _ => normalized
        };
    }

    public static string? NormalizePriority(string? value)
    {
        var normalized = Normalize(value);
        return normalized switch
        {
            "normal" => "medium",
            _ => normalized
        };
    }

    public static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToLowerInvariant();
}
