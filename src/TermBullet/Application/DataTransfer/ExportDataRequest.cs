namespace TermBullet.Application.DataTransfer;

public sealed record ExportDataRequest(string OutputPath, string Format = "json");
