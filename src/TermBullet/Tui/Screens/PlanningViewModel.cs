namespace TermBullet.Tui.Screens;

public enum PlanningScreenMode
{
    Hub,
    NewPlanning,
    RevisePlanning
}

public sealed class PlanningViewModel
{
    private PlanningViewModel(
        PlanningScreenMode mode,
        IReadOnlyList<string> primaryLines,
        IReadOnlyList<string> secondaryLines,
        IReadOnlyList<string> conversationLines,
        IReadOnlyList<string> promptLines,
        string footer)
    {
        Mode = mode;
        PrimaryLines = primaryLines;
        SecondaryLines = secondaryLines;
        ConversationLines = conversationLines;
        PromptLines = promptLines;
        Footer = footer;
    }

    public PlanningScreenMode Mode { get; }

    public IReadOnlyList<string> PrimaryLines { get; }

    public IReadOnlyList<string> SecondaryLines { get; }

    public IReadOnlyList<string> ConversationLines { get; }

    public IReadOnlyList<string> PromptLines { get; }

    public string Footer { get; }

    public static PlanningViewModel ForHub() =>
        new(
            PlanningScreenMode.Hub,
            ["> New Planning", "  Revise Planning"],
            [
                "New Planning creates a fresh AI draft from user intent.",
                "Revise Planning reviews existing work and proposes changes before applying them."
            ],
            [],
            [],
            " Enter open  Tab/1-2 focus  ? help  Esc back  q quit");

    public static PlanningViewModel ForNewPlanning() =>
        new(
            PlanningScreenMode.NewPlanning,
            [
                "> Project Plan",
                "  Weekly Plan",
                "output: tasks + notes",
                "tag: project tag or default",
                "scope: new work"
            ],
            ["Apply plan", "Discard draft"],
            [
                "assistant> Choose Project Plan for closed-scope work.",
                "assistant> Choose Weekly Plan for personal default-tag planning."
            ],
            ["Write a message..."],
            " Enter send/open  a apply  d discard  Up/Down scroll  PgUp/PgDn page  Tab/1-4 focus  ? help  Esc back  q quit");

    public static PlanningViewModel ForRevisePlanning() =>
        new(
            PlanningScreenMode.RevisePlanning,
            [
                "> Weekly Review",
                "  Project Review",
                "selected tag: -",
                "allowed actions: create, move, prioritize, cancel"
            ],
            ["Apply changes", "Discard draft"],
            [
                "assistant> Select Weekly Review for default-tag work.",
                "assistant> Select Project Review to focus one project tag."
            ],
            ["Write a message..."],
            " Enter send/open  a apply  d discard  Up/Down scroll  PgUp/PgDn page  Tab/1-4 focus  ? help  Esc back  q quit");
}
