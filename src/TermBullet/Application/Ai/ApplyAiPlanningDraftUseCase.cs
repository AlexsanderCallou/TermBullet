using TermBullet.Application.Items;
using TermBullet.Application.Tags;
using TermBullet.Domain.Items;

namespace TermBullet.Application.Ai;

public sealed class ApplyAiPlanningDraftUseCase(
    AiPlanningDraftValidator validator,
    CreateTagUseCase createTagUseCase,
    CreateItemUseCase createItemUseCase)
{
    public async Task<AiPlanningDraftApplyResult> ExecuteAsync(
        AiPlanningDraft draft,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(draft);

        var validation = validator.Validate(draft);
        if (!validation.IsValid)
        {
            throw new InvalidOperationException(
                $"AI planning draft is invalid: {string.Join(" ", validation.Errors)}");
        }

        var appliedActions = new List<AiPlanningDraftAppliedAction>();

        foreach (var action in draft.Actions)
        {
            var type = NormalizeRequired(action.Type, "type");
            switch (type)
            {
                case "create_tag":
                    var tag = await createTagUseCase.ExecuteAsync(
                        new CreateTagRequest
                        {
                            Name = NormalizeRequired(action.Name, "name"),
                            Description = action.Description
                        },
                        cancellationToken);
                    appliedActions.Add(new AiPlanningDraftAppliedAction(type, Tag: tag.Name));
                    break;

                case "create_task":
                    var task = await createItemUseCase.ExecuteAsync(
                        new CreateItemRequest
                        {
                            Type = ItemType.Task,
                            Content = NormalizeRequired(action.Content, "content"),
                            Collection = ParseTaskCollection(action.Collection),
                            Priority = ParsePriority(action.Priority),
                            Description = action.Description,
                            Tag = NormalizeRequired(action.Tag, "tag")
                        },
                        cancellationToken);
                    appliedActions.Add(new AiPlanningDraftAppliedAction(
                        type,
                        task.PublicRef,
                        NormalizeRequired(action.Tag, "tag"),
                        ToCollectionKey(task.Collection)));
                    break;

                case "create_note":
                    var note = await createItemUseCase.ExecuteAsync(
                        new CreateItemRequest
                        {
                            Type = ItemType.Note,
                            Content = NormalizeRequired(action.Content, "content"),
                            Collection = ItemCollection.Notes,
                            Description = action.Description,
                            Tag = NormalizeRequired(action.Tag, "tag")
                        },
                        cancellationToken);
                    appliedActions.Add(new AiPlanningDraftAppliedAction(
                        type,
                        note.PublicRef,
                        NormalizeRequired(action.Tag, "tag"),
                        ToCollectionKey(note.Collection)));
                    break;

                default:
                    throw new InvalidOperationException($"Unsupported action type '{type}'.");
            }
        }

        return new AiPlanningDraftApplyResult(appliedActions);
    }

    private static ItemCollection ParseTaskCollection(string? value) =>
<<<<<<< HEAD
        NormalizeCollectionRequired(value) switch
=======
        NormalizeRequired(value, "collection") switch
>>>>>>> 31d6ba16bacfc3554d22ce88aea847e70d502125
        {
            "today" => ItemCollection.Today,
            "week" => ItemCollection.Week,
            "month" => ItemCollection.Month,
            "backlog" => ItemCollection.Backlog,
            var collection => throw new InvalidOperationException($"Unsupported task collection '{collection}'.")
        };

    private static Priority ParsePriority(string? value) =>
<<<<<<< HEAD
        AiPlanningDraftNormalizer.NormalizePriority(value) switch
=======
        Normalize(value) switch
>>>>>>> 31d6ba16bacfc3554d22ce88aea847e70d502125
        {
            null => Priority.None,
            "none" => Priority.None,
            "low" => Priority.Low,
            "medium" => Priority.Medium,
            "high" => Priority.High,
            var priority => throw new InvalidOperationException($"Unsupported priority '{priority}'.")
        };

    private static string ToCollectionKey(ItemCollection collection) =>
        collection.ToString().ToLowerInvariant();

    private static string NormalizeRequired(string? value, string fieldName) =>
<<<<<<< HEAD
        AiPlanningDraftNormalizer.Normalize(value)
        ?? throw new InvalidOperationException($"{fieldName} is required.");

    private static string NormalizeCollectionRequired(string? value) =>
        AiPlanningDraftNormalizer.NormalizeCollection(value)
        ?? throw new InvalidOperationException("collection is required.");
=======
        Normalize(value) ?? throw new InvalidOperationException($"{fieldName} is required.");

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToLowerInvariant();
>>>>>>> 31d6ba16bacfc3554d22ce88aea847e70d502125
}
