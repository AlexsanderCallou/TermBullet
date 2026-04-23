using System.Text.Json;
using TermBullet.Infrastructure.Persistence.JsonFiles;

namespace TermBullet.Tests.Infrastructure.Persistence.JsonFiles;

public sealed class LocalHistoryMaintenanceServiceTests
{
    [Fact]
    public async Task ClearMonthAsync_removes_history_from_selected_month_and_keeps_items()
    {
        var root = CreateRoot();
        var monthlyPath = Path.Combine(root, "data", "2026", "data_04_2026.json");
        Directory.CreateDirectory(Path.GetDirectoryName(monthlyPath)!);
        await File.WriteAllTextAsync(monthlyPath, CreateMonthlyJson("2026-04", "data_04_2026.json", "t-0426-1"));
        var service = CreateService(root);

        await service.ClearMonthAsync(4, 2026);

        var json = await File.ReadAllTextAsync(monthlyPath);
        using var document = JsonDocument.Parse(json);
        Assert.Single(document.RootElement.GetProperty("items").EnumerateArray());
        Assert.Empty(document.RootElement.GetProperty("history").EnumerateArray());
        Assert.True(File.Exists(Path.Combine(root, "data", "2026", "data_04_2026.backup.json")));
    }

    [Fact]
    public async Task ClearAllAsync_removes_history_from_all_monthly_files()
    {
        var root = CreateRoot();
        var aprilPath = Path.Combine(root, "data", "2026", "data_04_2026.json");
        var mayPath = Path.Combine(root, "data", "2026", "data_05_2026.json");
        Directory.CreateDirectory(Path.GetDirectoryName(aprilPath)!);
        await File.WriteAllTextAsync(aprilPath, CreateMonthlyJson("2026-04", "data_04_2026.json", "t-0426-1"));
        await File.WriteAllTextAsync(mayPath, CreateMonthlyJson("2026-05", "data_05_2026.json", "t-0526-1"));
        var service = CreateService(root);

        await service.ClearAllAsync();

        AssertHistoryCleared(aprilPath);
        AssertHistoryCleared(mayPath);
    }

    private static LocalHistoryMaintenanceService CreateService(string root)
    {
        return new LocalHistoryMaintenanceService(
            root,
            new MonthlyJsonFilePathResolver(root),
            new SafeJsonFileStore());
    }

    private static string CreateRoot()
    {
        var root = Path.Combine(Path.GetTempPath(), "TermBullet.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        return root;
    }

    private static void AssertHistoryCleared(string path)
    {
        var json = File.ReadAllText(path);
        using var document = JsonDocument.Parse(json);
        Assert.Empty(document.RootElement.GetProperty("history").EnumerateArray());
    }

    private static string CreateMonthlyJson(string period, string fileName, string publicRef) =>
        $$"""
        {
          "period": "{{period}}",
          "file_name": "{{fileName}}",
          "public_ref_sequences": {
            "task": 1,
            "note": 0,
            "event": 0
          },
          "items": [
            {
              "id": "0f3a9d94-4df0-47f7-95c1-0f967c22f4db",
              "public_ref": "{{publicRef}}",
              "type": "task",
              "content": "Fix authentication flow",
              "description": null,
              "status": "open",
              "collection": "today",
              "priority": "none",
              "tags": [],
              "version": 1,
              "created_at": "2026-04-23T10:30:00Z",
              "updated_at": "2026-04-23T10:30:00Z"
            }
          ],
          "history": [
            {
              "id": "7d5b9856-045f-43ef-a646-4ee9c86fe2d8",
              "item_id": "0f3a9d94-4df0-47f7-95c1-0f967c22f4db",
              "public_ref": "{{publicRef}}",
              "event_type": "created",
              "occurred_at": "2026-04-23T10:30:00Z",
              "data": {
                "content": "Fix authentication flow"
              }
            }
          ],
          "settings": {}
        }
        """;
}
