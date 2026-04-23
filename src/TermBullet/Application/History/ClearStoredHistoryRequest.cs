namespace TermBullet.Application.History;

public sealed record ClearStoredHistoryRequest(
    int? Month = null,
    int? Year = null,
    bool All = false);
