namespace TermBullet.Application.Ai;

public sealed class AiPlanningDraftValidator
{
    private static readonly HashSet<string> AllowedModes = new(StringComparer.OrdinalIgnoreCase)
    {
        "new_project",
        "new_weekly"
    };

    private static readonly HashSet<string> AllowedActionTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "create_tag",
        "create_task",
        "create_note"
    };

    private static readonly HashSet<string> TaskCollections = new(StringComparer.OrdinalIgnoreCase)
    {
        "today",
        "week",
        "month",
        "backlog"
    };

    private static readonly HashSet<string> Priorities = new(StringComparer.OrdinalIgnoreCase)
    {
        "none",
        "low",
        "medium",
        "high"
    };

    public AiPlanningDraftValidationResult Validate(AiPlanningDraft draft)
    {
        ArgumentNullException.ThrowIfNull(draft);

        var errors = new List<string>();
<<<<<<< HEAD
        var mode = AiPlanningDraftNormalizer.Normalize(draft.Mode);
=======
        var mode = Normalize(draft.Mode);
>>>>>>> 31d6ba16bacfc3554d22ce88aea847e70d502125

        if (mode is null)
        {
            errors.Add("Draft mode is required.");
        }
        else if (!AllowedModes.Contains(mode))
        {
            errors.Add($"Unsupported planning mode '{mode}'.");
        }

        if (string.IsNullOrWhiteSpace(draft.Summary))
        {
            errors.Add("Draft summary is required.");
        }

        if (draft.Actions.Count == 0)
        {
            errors.Add("Draft must include at least one action.");
        }

        var createdOrUsedNonDefaultTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (var index = 0; index < draft.Actions.Count; index++)
        {
            var action = draft.Actions[index];
            var actionNumber = index + 1;
<<<<<<< HEAD
            var type = AiPlanningDraftNormalizer.Normalize(action.Type);
=======
            var type = Normalize(action.Type);
>>>>>>> 31d6ba16bacfc3554d22ce88aea847e70d502125

            if (type is null)
            {
                errors.Add($"Action {actionNumber}: type is required.");
                continue;
            }

            if (!AllowedActionTypes.Contains(type))
            {
                errors.Add($"Action {actionNumber}: Unsupported action type '{type}'.");
                continue;
            }

            ValidateAction(action, actionNumber, type, errors);

<<<<<<< HEAD
            var tag = AiPlanningDraftNormalizer.Normalize(action.Tag);
            if (type == "create_tag")
            {
                tag = AiPlanningDraftNormalizer.Normalize(action.Name);
=======
            var tag = Normalize(action.Tag);
            if (type == "create_tag")
            {
                tag = Normalize(action.Name);
>>>>>>> 31d6ba16bacfc3554d22ce88aea847e70d502125
            }

            if (IsNonDefaultTag(tag))
            {
                createdOrUsedNonDefaultTags.Add(tag!);
            }

            ValidateModeRules(mode, action, actionNumber, type, errors);
        }

        if (mode is "new_project" && createdOrUsedNonDefaultTags.Count == 0)
        {
            errors.Add("new_project drafts must create or use one non-default tag.");
        }

        if (mode is "new_project" && createdOrUsedNonDefaultTags.Count > 1)
        {
            errors.Add("Project planning drafts must use one non-default tag.");
        }

        return new AiPlanningDraftValidationResult(errors);
    }

    private static void ValidateAction(
        AiPlanningDraftAction action,
        int actionNumber,
        string type,
        List<string> errors)
    {
        switch (type)
        {
            case "create_tag":
                Require(action.Name, actionNumber, "name", errors);
                if (IsDefaultTag(action.Name))
                {
                    errors.Add($"Action {actionNumber}: create_tag must use a non-default tag name.");
                }

                break;

            case "create_task":
                Require(action.Content, actionNumber, "content", errors);
                Require(action.Tag, actionNumber, "tag", errors);
                ValidateTaskCollection(action.Collection, actionNumber, errors);
                ValidateOptionalPriority(action.Priority, actionNumber, errors);
                break;

            case "create_note":
                Require(action.Content, actionNumber, "content", errors);
                Require(action.Tag, actionNumber, "tag", errors);
                break;

        }
    }

    private static void ValidateModeRules(
        string? mode,
        AiPlanningDraftAction action,
        int actionNumber,
        string type,
        List<string> errors)
    {
        if (mode is null)
        {
            return;
        }

        if (mode == "new_weekly" && type == "create_tag")
        {
            errors.Add($"Action {actionNumber}: new_weekly drafts must not create project tags.");
        }

        if (mode == "new_weekly" && type is "create_task" or "create_note" && !IsDefaultTag(action.Tag))
        {
            errors.Add($"Action {actionNumber}: new_weekly drafts must use the default tag.");
        }

        if (mode == "new_project" && type is "create_task" or "create_note" && !IsNonDefaultTag(action.Tag))
        {
            errors.Add($"Action {actionNumber}: new_project drafts must use a non-default tag.");
        }

    }

    private static void ValidateTaskCollection(string? collection, int actionNumber, List<string> errors)
    {
<<<<<<< HEAD
        var normalized = AiPlanningDraftNormalizer.NormalizeCollection(collection);
=======
        var normalized = Normalize(collection);
>>>>>>> 31d6ba16bacfc3554d22ce88aea847e70d502125
        if (normalized is null)
        {
            errors.Add($"Action {actionNumber}: collection is required.");
            return;
        }

        if (!TaskCollections.Contains(normalized))
        {
            errors.Add($"Action {actionNumber}: unsupported task collection '{normalized}'.");
        }
    }

    private static void ValidateOptionalPriority(string? priority, int actionNumber, List<string> errors)
    {
<<<<<<< HEAD
        var normalized = AiPlanningDraftNormalizer.NormalizePriority(priority);
=======
        var normalized = Normalize(priority);
>>>>>>> 31d6ba16bacfc3554d22ce88aea847e70d502125
        if (normalized is not null && !Priorities.Contains(normalized))
        {
            errors.Add($"Action {actionNumber}: unsupported priority '{normalized}'.");
        }
    }

    private static void Require(string? value, int actionNumber, string fieldName, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors.Add($"Action {actionNumber}: {fieldName} is required.");
        }
    }

    private static bool IsDefaultTag(string? tag) =>
<<<<<<< HEAD
        string.Equals(AiPlanningDraftNormalizer.Normalize(tag), "default", StringComparison.OrdinalIgnoreCase);

    private static bool IsNonDefaultTag(string? tag) =>
        AiPlanningDraftNormalizer.Normalize(tag) is { } normalized
        && !string.Equals(normalized, "default", StringComparison.OrdinalIgnoreCase);
=======
        string.Equals(Normalize(tag), "default", StringComparison.OrdinalIgnoreCase);

    private static bool IsNonDefaultTag(string? tag) =>
        Normalize(tag) is { } normalized && !string.Equals(normalized, "default", StringComparison.OrdinalIgnoreCase);

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToLowerInvariant();
>>>>>>> 31d6ba16bacfc3554d22ce88aea847e70d502125
}
