namespace TermBullet.Application.DataTransfer;

public sealed record ImportDataRequest(string InputPath, string Format = "json");
