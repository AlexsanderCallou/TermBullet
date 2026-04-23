using System.Text.Json;
using TermBullet.Application.Ports;
using TermBullet.Core.Items;
using TermBullet.Core.Refs;
using TermBullet.Infrastructure.Export;
using TermBullet.Infrastructure.Persistence.JsonFiles;

namespace TermBullet.Tests.Infrastructure.Export;

public sealed class JsonDataTransferServiceTests
{
    private static readonly DateTimeOffset CreatedAt = new(2026, 4, 23, 10, 30, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset ChangedAt = new(2026, 4, 23, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task ExportAsync_writes_package_for_empty_data_directory()
    {
        var context = CreateContext();
        var service = CreateService(context);
        var outputPath = Path.Combine(context.ProjectRootPath, "exports", "backup.json");

        await service.ExportAsync(outputPath);

        Assert.True(File.Exists(outputPath));

        var json = await File.ReadAllTextAsync(outputPath);
        using var doc = JsonDocument.Parse(json);
        Assert.Equal("termbullet-export", doc.RootElement.GetProperty("format").GetString());
        Assert.Equal(1, doc.RootElement.GetProperty("version").GetInt32());
        Assert.Empty(doc.RootElement.GetProperty("monthly_files").EnumerateArray());
    }

    [Fact]
    public async Task ExportAsync_writes_monthly_files_and_settings_to_package()
    {
        var context = CreateContext();
        var repository = CreateRepository(context);
        var settingsStore = new LocalSettingsStore(context.ProjectRootPath, new SafeJsonFileStore());
        var item = CreateItem(
            id: Guid.Parse("0f3a9d94-4df0-47f7-95c1-0f967c22f4db"),
            publicRef: "t-0426-1",
            collection: ItemCollection.Today);
        item.Edit("Refactor auth flow", ChangedAt, "Keep user session stable");
        item.MoveTo(ItemCollection.Week, ChangedAt);
        item.SetPriority(Priority.High, ChangedAt);
        await repository.AddAsync(item);
        await repository.UpdateAsync(item);
        await settingsStore.SetAsync("theme", "dark");
        await settingsStore.SetAsync("compact_lists", "true");

        var service = CreateService(context);
        var outputPath = Path.Combine(context.ProjectRootPath, "exports", "backup.json");

        await service.ExportAsync(outputPath);

        var json = await File.ReadAllTextAsync(outputPath);
        using var doc = JsonDocument.Parse(json);
        var monthlyFiles = doc.RootElement.GetProperty("monthly_files").EnumerateArray().ToArray();
        var monthlyFile = Assert.Single(monthlyFiles);
        Assert.Equal("data/2026/data_04_2026.json", monthlyFile.GetProperty("relative_path").GetString());
        Assert.Equal("2026-04", monthlyFile.GetProperty("period").GetString());

        var exportedItem = Assert.Single(monthlyFile.GetProperty("content").GetProperty("items").EnumerateArray());
        Assert.Equal("0f3a9d94-4df0-47f7-95c1-0f967c22f4db", exportedItem.GetProperty("id").GetString());
        Assert.Equal("t-0426-1", exportedItem.GetProperty("public_ref").GetString());
        Assert.Equal("task", exportedItem.GetProperty("type").GetString());
        Assert.Equal("Refactor auth flow", exportedItem.GetProperty("content").GetString());
        Assert.Equal("week", exportedItem.GetProperty("collection").GetString());
        Assert.Equal("high", exportedItem.GetProperty("priority").GetString());
        Assert.Equal(item.Version, exportedItem.GetProperty("version").GetInt32());
        Assert.Equal("auth", exportedItem.GetProperty("tags")[0].GetString());
        Assert.Equal(item.CreatedAt, exportedItem.GetProperty("created_at").GetDateTimeOffset());
        Assert.Equal(item.UpdatedAt, exportedItem.GetProperty("updated_at").GetDateTimeOffset());

        var settings = doc.RootElement.GetProperty("settings");
        var defaultProfile = settings.GetProperty("profiles").GetProperty("default");
        Assert.Equal("dark", defaultProfile.GetProperty("theme").GetString());
        Assert.Equal("true", defaultProfile.GetProperty("compact_lists").GetString());
    }

    [Fact]
    public async Task ImportAsync_restores_monthly_files_settings_and_rebuilds_index()
    {
        var context = CreateContext();
        var importPath = Path.Combine(context.ProjectRootPath, "imports", "backup.json");
        Directory.CreateDirectory(Path.GetDirectoryName(importPath)!);
        await File.WriteAllTextAsync(importPath, ValidPackageJson);
        var service = CreateService(context);

        await service.ImportAsync(importPath);

        Assert.True(File.Exists(context.MonthlyFilePath));
        Assert.True(File.Exists(context.IndexFilePath));

        var repository = CreateRepository(context);
        var found = await repository.FindByPublicRefAsync("t-0426-1");
        Assert.NotNull(found);
        Assert.Equal(Guid.Parse("0f3a9d94-4df0-47f7-95c1-0f967c22f4db"), found.Id);
        Assert.Equal("Fix authentication flow", found.Content);
        Assert.Equal(ItemCollection.Today, found.Collection);

        var settingsStore = new LocalSettingsStore(context.ProjectRootPath, new SafeJsonFileStore());
        Assert.Equal("dark", await settingsStore.GetAsync("theme"));
    }

    [Fact]
    public async Task ImportAsync_throws_for_malformed_json()
    {
        var context = CreateContext();
        var importPath = Path.Combine(context.ProjectRootPath, "imports", "backup.json");
        Directory.CreateDirectory(Path.GetDirectoryName(importPath)!);
        await File.WriteAllTextAsync(importPath, "{ invalid");
        var service = CreateService(context);

        await Assert.ThrowsAsync<InvalidDataException>(() => service.ImportAsync(importPath));
    }

    [Fact]
    public async Task ImportAsync_throws_for_duplicate_public_refs_in_same_period()
    {
        var context = CreateContext();
        var importPath = Path.Combine(context.ProjectRootPath, "imports", "duplicate-refs.json");
        Directory.CreateDirectory(Path.GetDirectoryName(importPath)!);
        await File.WriteAllTextAsync(importPath, DuplicatePublicRefsPackageJson);
        var service = CreateService(context);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.ImportAsync(importPath));
    }

    [Fact]
    public async Task ImportAsync_throws_for_duplicate_internal_ids()
    {
        var context = CreateContext();
        var importPath = Path.Combine(context.ProjectRootPath, "imports", "duplicate-ids.json");
        Directory.CreateDirectory(Path.GetDirectoryName(importPath)!);
        await File.WriteAllTextAsync(importPath, DuplicateInternalIdsPackageJson);
        var service = CreateService(context);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.ImportAsync(importPath));
    }

    [Fact]
    public async Task ExportAsync_and_ImportAsync_round_trip_preserves_item_contract()
    {
        var source = CreateContext();
        var sourceRepository = CreateRepository(source);
        var sourceSettingsStore = new LocalSettingsStore(source.ProjectRootPath, new SafeJsonFileStore());
        var item = CreateItem(
            id: Guid.Parse("0f3a9d94-4df0-47f7-95c1-0f967c22f4db"),
            publicRef: "t-0426-1",
            collection: ItemCollection.Today);
        item.Edit("Fix authentication flow", ChangedAt, "Keep tests green");
        item.SetPriority(Priority.High, ChangedAt);
        item.MarkDone(ChangedAt);
        await sourceRepository.AddAsync(item);
        await sourceRepository.UpdateAsync(item);
        await sourceSettingsStore.SetAsync("theme", "dark");

        var exportPath = Path.Combine(source.ProjectRootPath, "exports", "backup.json");
        await CreateService(source).ExportAsync(exportPath);

        var target = CreateContext();
        await CreateService(target).ImportAsync(exportPath);

        var importedRepository = CreateRepository(target);
        var imported = await importedRepository.FindByPublicRefAsync("t-0426-1");
        Assert.NotNull(imported);
        Assert.Equal(item.Id, imported.Id);
        Assert.Equal(item.PublicRef.Value, imported.PublicRef.Value);
        Assert.Equal(item.Content, imported.Content);
        Assert.Equal(item.Description, imported.Description);
        Assert.Equal(item.Status, imported.Status);
        Assert.Equal(item.Collection, imported.Collection);
        Assert.Equal(item.Priority, imported.Priority);
        Assert.Equal(item.Version, imported.Version);
        Assert.Equal(item.CreatedAt, imported.CreatedAt);
        Assert.Equal(item.UpdatedAt, imported.UpdatedAt);
        Assert.Equal(item.CompletedAt, imported.CompletedAt);
        Assert.Equal(item.Tags, imported.Tags);

        var importedSettingsStore = new LocalSettingsStore(target.ProjectRootPath, new SafeJsonFileStore());
        Assert.Equal("dark", await importedSettingsStore.GetAsync("theme"));
    }

    private static JsonDataTransferService CreateService(TestContext context)
    {
        return new JsonDataTransferService(
            context.ProjectRootPath,
            new SafeJsonFileStore(),
            new LocalJsonIndexService(context.ProjectRootPath));
    }

    private static JsonFileItemRepository CreateRepository(TestContext context)
    {
        return new JsonFileItemRepository(
            new FixedClock(CreatedAt),
            new MonthlyJsonFilePathResolver(context.ProjectRootPath),
            new SafeJsonFileStore(),
            new LocalJsonIndexService(context.ProjectRootPath));
    }

    private static Item CreateItem(Guid id, string publicRef, ItemCollection collection)
    {
        return Item.Create(
            id,
            PublicRef.Parse(publicRef),
            ItemType.Task,
            "Fix authentication flow",
            collection,
            CreatedAt,
            tags: ["auth"]);
    }

    private static TestContext CreateContext()
    {
        var projectRootPath = Path.Combine(Path.GetTempPath(), "TermBullet.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(projectRootPath);
        var monthlyFilePath = Path.Combine(projectRootPath, "data", "2026", "data_04_2026.json");
        var indexFilePath = Path.Combine(projectRootPath, "data", "index.json");
        return new TestContext(projectRootPath, monthlyFilePath, indexFilePath);
    }

    private sealed record TestContext(string ProjectRootPath, string MonthlyFilePath, string IndexFilePath);

    private sealed class FixedClock(DateTimeOffset utcNow) : IClock
    {
        public DateTimeOffset UtcNow { get; } = utcNow;
    }

    private const string ValidPackageJson =
        """
        {
          "format": "termbullet-export",
          "version": 1,
          "exported_at": "2026-04-23T12:00:00Z",
          "monthly_files": [
            {
              "relative_path": "data/2026/data_04_2026.json",
              "period": "2026-04",
              "content": {
                "period": "2026-04",
                "file_name": "data_04_2026.json",
                "public_ref_sequences": {
                  "task": 1,
                  "note": 0,
                  "event": 0
                },
                "items": [
                  {
                    "id": "0f3a9d94-4df0-47f7-95c1-0f967c22f4db",
                    "public_ref": "t-0426-1",
                    "type": "task",
                    "content": "Fix authentication flow",
                    "description": null,
                    "status": "open",
                    "collection": "today",
                    "priority": "none",
                    "tags": ["auth"],
                    "due_at": null,
                    "scheduled_at": null,
                    "estimate_minutes": null,
                    "version": 1,
                    "created_at": "2026-04-23T10:30:00Z",
                    "updated_at": "2026-04-23T10:30:00Z",
                    "completed_at": null,
                    "cancelled_at": null,
                    "migrated_at": null
                  }
                ],
                "history": [
                  {
                    "id": "7d5b9856-045f-43ef-a646-4ee9c86fe2d8",
                    "item_id": "0f3a9d94-4df0-47f7-95c1-0f967c22f4db",
                    "public_ref": "t-0426-1",
                    "event_type": "created",
                    "occurred_at": "2026-04-23T10:30:00Z",
                    "data": {
                      "content": "Fix authentication flow"
                    }
                  }
                ],
                "settings": {}
              }
            }
          ],
          "settings": {
            "profiles": {
              "default": {
                "theme": "dark"
              }
            }
          }
        }
        """;

    private const string DuplicatePublicRefsPackageJson =
        """
        {
          "format": "termbullet-export",
          "version": 1,
          "exported_at": "2026-04-23T12:00:00Z",
          "monthly_files": [
            {
              "relative_path": "data/2026/data_04_2026.json",
              "period": "2026-04",
              "content": {
                "period": "2026-04",
                "file_name": "data_04_2026.json",
                "public_ref_sequences": {
                  "task": 2,
                  "note": 0,
                  "event": 0
                },
                "items": [
                  {
                    "id": "0f3a9d94-4df0-47f7-95c1-0f967c22f4db",
                    "public_ref": "t-0426-1",
                    "type": "task",
                    "content": "One",
                    "description": null,
                    "status": "open",
                    "collection": "today",
                    "priority": "none",
                    "tags": [],
                    "version": 1,
                    "created_at": "2026-04-23T10:30:00Z",
                    "updated_at": "2026-04-23T10:30:00Z"
                  },
                  {
                    "id": "c4dbec0e-c42d-4f26-8659-05bfdb4db056",
                    "public_ref": "t-0426-1",
                    "type": "task",
                    "content": "Two",
                    "description": null,
                    "status": "open",
                    "collection": "today",
                    "priority": "none",
                    "tags": [],
                    "version": 1,
                    "created_at": "2026-04-23T10:31:00Z",
                    "updated_at": "2026-04-23T10:31:00Z"
                  }
                ],
                "history": [],
                "settings": {}
              }
            }
          ]
        }
        """;

    private const string DuplicateInternalIdsPackageJson =
        """
        {
          "format": "termbullet-export",
          "version": 1,
          "exported_at": "2026-04-23T12:00:00Z",
          "monthly_files": [
            {
              "relative_path": "data/2026/data_04_2026.json",
              "period": "2026-04",
              "content": {
                "period": "2026-04",
                "file_name": "data_04_2026.json",
                "public_ref_sequences": {
                  "task": 1,
                  "note": 0,
                  "event": 0
                },
                "items": [
                  {
                    "id": "0f3a9d94-4df0-47f7-95c1-0f967c22f4db",
                    "public_ref": "t-0426-1",
                    "type": "task",
                    "content": "April item",
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
                "history": [],
                "settings": {}
              }
            },
            {
              "relative_path": "data/2026/data_05_2026.json",
              "period": "2026-05",
              "content": {
                "period": "2026-05",
                "file_name": "data_05_2026.json",
                "public_ref_sequences": {
                  "task": 1,
                  "note": 0,
                  "event": 0
                },
                "items": [
                  {
                    "id": "0f3a9d94-4df0-47f7-95c1-0f967c22f4db",
                    "public_ref": "t-0526-1",
                    "type": "task",
                    "content": "May item",
                    "description": null,
                    "status": "open",
                    "collection": "week",
                    "priority": "low",
                    "tags": [],
                    "version": 1,
                    "created_at": "2026-05-01T08:00:00Z",
                    "updated_at": "2026-05-01T08:00:00Z"
                  }
                ],
                "history": [],
                "settings": {}
              }
            }
          ]
        }
        """;
}
