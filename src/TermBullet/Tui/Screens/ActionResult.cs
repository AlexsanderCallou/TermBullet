namespace TermBullet.Tui.Screens;

public sealed class ActionResult
{
    private ActionResult(bool success, string? errorMessage)
    {
        Success = success;
        ErrorMessage = errorMessage;
    }

    public bool Success { get; }

    public string? ErrorMessage { get; }

    public static ActionResult Ok() => new(true, null);

    public static ActionResult Fail(string errorMessage) => new(false, errorMessage);
}
