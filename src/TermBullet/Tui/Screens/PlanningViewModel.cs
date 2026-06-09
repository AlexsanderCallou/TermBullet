namespace TermBullet.Tui.Screens;

public enum PlanningScreenMode
{
    NewPlanning
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
        ForNewPlanning();

    public static PlanningViewModel ForNewPlanning() =>
        new(
            PlanningScreenMode.NewPlanning,
            [
                "Topic: -",
                "Project tag: -",
                "Detail level: High",
                "Start today: Yes",
                "Distribution: today, week, month, backlog"
            ],
            ["Generate draft", "Apply plan", "Discard draft"],
            [
                "assistant> Fill the guided fields, then generate a structured draft.",
                "assistant> The model chooses task count; TermBullet controls tag and placement."
            ],
            ["Topic and tag are edited in the Setup panel."],
            " Tab/1-4 focus  ? help  Esc back  q quit");
}
