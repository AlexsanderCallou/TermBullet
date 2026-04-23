using System.Text.Json;
using TermBullet.Infrastructure.Persistence.JsonFiles;

namespace TermBullet.Tests.Infrastructure.Persistence.JsonFiles;

public sealed class LocalJsonIndexServiceTests
{
    [Fact]
    public async Task RebuildAsync_creates_data_index_with_items_from_monthly_files()
    {
        var root = Path.Combine(Path.GetTempPath(), "TermBullet.Tests", Guid.NewGuid().ToString("N"));
        var yearDir = Path.Combine(root, "data", "2026");
        Directory.CreateDirectory(yearDir);

        var monthFile = Path.Combine(yearDir, "data_04_2026.json");
        await File.WriteAllTextAsync(
            monthFile,
            """
            {
              "period": "2026-04",
              "file_name": "data_04_2026.json",
              "public_ref_sequences": { "task": 1, "note": 0, "event": 0 },
              "items": [
                {
                  "id": "0f3a9d94-4df0-47f7-95c1-0f967c22f4db",
                  "public_ref": "t-0426-1",
                  "type": "task",
                  "content": "fix auth",
                  "description": null,
                  "status": "open",
                  "collection": "today",
                  "priority": "high",
                  "tags": ["auth"],
                  "version": 1,
                  "created_at": "2026-04-23T10:30:00Z",
                  "updated_at": "2026-04-23T10:30:00Z"
                }
              ],
              "history": [],
              "settings": {}
            }
            """);

        var service = new LocalJsonIndexService(root);

        await service.RebuildAsync();

        var indexPath = Path.Combine(root, "data", "index.json");
        Assert.True(File.Exists(indexPath));

        var indexJson = await File.ReadAllTextAsync(indexPath);
        using var doc = JsonDocument.Parse(indexJson);
        var items = doc.RootElement.GetProperty("items").EnumerateArray().ToArray();
        var item = Assert.Single(items);
        Assert.Equal("t-0426-1", item.GetProperty("public_ref").GetString());
        Assert.Equal("2026-04", item.GetProperty("period").GetString());
        Assert.Equal("data/2026/data_04_2026.json", item.GetProperty("source_file").GetString());
    }
}
