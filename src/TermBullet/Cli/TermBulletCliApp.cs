using System.CommandLine;
using TermBullet.Application.Configuration;
using TermBullet.Application.DataTransfer;
using TermBullet.Application.History;
using TermBullet.Application.Items;
using TermBullet.Core.Items;

namespace TermBullet.Cli;

public sealed class TermBulletCliApp(
    ListConfigurationUseCase listConfigurationUseCase,
    GetConfigurationUseCase getConfigurationUseCase,
    SetConfigurationUseCase setConfigurationUseCase,
    GetConfigurationPathUseCase getConfigurationPathUseCase,
    ExportDataUseCase exportDataUseCase,
    ImportDataUseCase importDataUseCase,
    ClearStoredHistoryUseCase clearStoredHistoryUseCase,
    TextWriter output,
    TextWriter error,
    CreateItemUseCase? createItemUseCase = null,
    ListItemsUseCase? listItemsUseCase = null,
    ShowItemUseCase? showItemUseCase = null,
    GetTodayItemsUseCase? getTodayItemsUseCase = null,
    GetWeekItemsUseCase? getWeekItemsUseCase = null,
    GetBacklogItemsUseCase? getBacklogItemsUseCase = null,
    EditItemUseCase? editItemUseCase = null,
    MarkDoneItemUseCase? markDoneItemUseCase = null,
    CancelItemUseCase? cancelItemUseCase = null,
    MoveItemUseCase? moveItemUseCase = null,
    SetItemPriorityUseCase? setItemPriorityUseCase = null,
    TagItemUseCase? tagItemUseCase = null,
    UntagItemUseCase? untagItemUseCase = null,
    MigrateItemUseCase? migrateItemUseCase = null)
{
    public Task<int> InvokeAsync(string[] args, CancellationToken cancellationToken = default)
    {
        var rootCommand = BuildRootCommand(output, error, cancellationToken);
        return rootCommand.Parse(args).InvokeAsync();
    }

    public RootCommand BuildRootCommand(
        TextWriter standardOutput,
        TextWriter standardError,
        CancellationToken cancellationToken = default)
    {
        var rootCommand = new RootCommand("TermBullet - Local-First Terminal Planner");

        if (createItemUseCase is not null)
        {
            rootCommand.Subcommands.Add(BuildAddCommand(standardOutput, standardError, cancellationToken));
        }

        if (listItemsUseCase is not null)
        {
            rootCommand.Subcommands.Add(BuildListCommand(standardOutput, standardError, cancellationToken));
        }

        if (showItemUseCase is not null)
        {
            rootCommand.Subcommands.Add(BuildShowCommand(standardOutput, standardError, cancellationToken));
        }

        if (getTodayItemsUseCase is not null)
        {
            rootCommand.Subcommands.Add(BuildCollectionCommand(
                "today",
                "Show today items.",
                getTodayItemsUseCase.ExecuteAsync,
                standardOutput,
                standardError,
                cancellationToken));
        }

        if (getWeekItemsUseCase is not null)
        {
            rootCommand.Subcommands.Add(BuildCollectionCommand(
                "week",
                "Show week items.",
                getWeekItemsUseCase.ExecuteAsync,
                standardOutput,
                standardError,
                cancellationToken));
        }

        if (getBacklogItemsUseCase is not null)
        {
            rootCommand.Subcommands.Add(BuildCollectionCommand(
                "backlog",
                "Show backlog items.",
                getBacklogItemsUseCase.ExecuteAsync,
                standardOutput,
                standardError,
                cancellationToken));
        }

        if (editItemUseCase is not null)
        {
            rootCommand.Subcommands.Add(BuildEditCommand(standardOutput, standardError, cancellationToken));
        }

        if (markDoneItemUseCase is not null)
        {
            rootCommand.Subcommands.Add(BuildSimpleMutationCommand(
                "done",
                "Mark an item as done",
                markDoneItemUseCase.ExecuteAsync,
                standardOutput,
                standardError,
                cancellationToken));
        }

        if (cancelItemUseCase is not null)
        {
            rootCommand.Subcommands.Add(BuildSimpleMutationCommand(
                "cancel",
                "Cancel an item",
                cancelItemUseCase.ExecuteAsync,
                standardOutput,
                standardError,
                cancellationToken));
        }

        if (moveItemUseCase is not null)
        {
            rootCommand.Subcommands.Add(BuildMoveCommand(standardOutput, standardError, cancellationToken));
        }

        if (setItemPriorityUseCase is not null)
        {
            rootCommand.Subcommands.Add(BuildPriorityCommand(standardOutput, standardError, cancellationToken));
        }

        if (tagItemUseCase is not null)
        {
            rootCommand.Subcommands.Add(BuildTagCommand("tag", "Add a tag to an item", true, standardOutput, standardError, cancellationToken));
        }

        if (untagItemUseCase is not null)
        {
            rootCommand.Subcommands.Add(BuildTagCommand("untag", "Remove a tag from an item", false, standardOutput, standardError, cancellationToken));
        }

        if (migrateItemUseCase is not null)
        {
            rootCommand.Subcommands.Add(BuildSimpleMutationCommand(
                "migrate",
                "Mark an item as migrated",
                migrateItemUseCase.ExecuteAsync,
                standardOutput,
                standardError,
                cancellationToken));
        }

        rootCommand.Subcommands.Add(BuildExportCommand(standardOutput, standardError, cancellationToken));
        rootCommand.Subcommands.Add(BuildImportCommand(standardOutput, standardError, cancellationToken));
        rootCommand.Subcommands.Add(BuildConfigCommand(standardOutput, standardError, cancellationToken));
        rootCommand.Subcommands.Add(BuildHistoryCommand(standardOutput, standardError, cancellationToken));

        return rootCommand;
    }

    private Command BuildExportCommand(
        TextWriter standardOutput,
        TextWriter standardError,
        CancellationToken cancellationToken)
    {
        var formatOption = new Option<string>("--format")
        {
            Description = "Format: json",
            DefaultValueFactory = _ => "json"
        };
        var outputOption = new Option<string>("--output")
        {
            Description = "Output file or directory",
            Required = true
        };

        var command = new Command("export", "Export local data.")
        {
            formatOption,
            outputOption
        };

        command.SetAction(async parseResult =>
        {
            try
            {
                var format = parseResult.GetValue(formatOption) ?? "json";
                var outputPath = parseResult.GetValue(outputOption)
                    ?? throw new InvalidOperationException("Output path is required.");

                await exportDataUseCase.ExecuteAsync(
                    new ExportDataRequest(outputPath, format),
                    cancellationToken);

                await standardOutput.WriteLineAsync(outputPath);
                return 0;
            }
            catch (Exception exception)
            {
                await standardError.WriteLineAsync(exception.Message);
                return 1;
            }
        });

        return command;
    }

    private Command BuildAddCommand(
        TextWriter standardOutput,
        TextWriter standardError,
        CancellationToken cancellationToken)
    {
        var textArgument = new Argument<string>("text")
        {
            Description = "Item content"
        };
        var noteOption = new Option<bool>("--note")
        {
            Description = "Create a note"
        };
        var eventOption = new Option<bool>("--event")
        {
            Description = "Create an event"
        };
        var priorityOption = new Option<string?>("--priority")
        {
            Description = "Priority: none, low, medium, high"
        };
        var collectionOption = new Option<string?>("--collection")
        {
            Description = "Collection: today, week, backlog, monthly, archived"
        };
        var tagOption = new Option<string[]>("--tag")
        {
            Description = "Repeatable tag option",
            Arity = ArgumentArity.ZeroOrMore,
            AllowMultipleArgumentsPerToken = true
        };

        var command = new Command("add", "Create a new item")
        {
            textArgument,
            noteOption,
            eventOption,
            priorityOption,
            collectionOption,
            tagOption
        };

        command.SetAction(async parseResult =>
        {
            try
            {
                var content = parseResult.GetValue(textArgument)
                    ?? throw new InvalidOperationException("Item content is required.");
                var itemType = ResolveItemType(
                    parseResult.GetValue(noteOption),
                    parseResult.GetValue(eventOption));
                var priority = ParsePriority(parseResult.GetValue(priorityOption));
                var collection = ParseCollection(parseResult.GetValue(collectionOption)) ?? ItemCollection.Today;
                var tags = parseResult.GetValue(tagOption);

                var result = await createItemUseCase!.ExecuteAsync(new CreateItemRequest
                {
                    Type = itemType,
                    Content = content,
                    Collection = collection,
                    Priority = priority,
                    Tags = tags
                }, cancellationToken);

                await standardOutput.WriteLineAsync($"{result.PublicRef} {content}");
                return 0;
            }
            catch (Exception exception)
            {
                await standardError.WriteLineAsync(exception.Message);
                return 1;
            }
        });

        return command;
    }

    private Command BuildListCommand(
        TextWriter standardOutput,
        TextWriter standardError,
        CancellationToken cancellationToken)
    {
        var collectionOption = new Option<string?>("--collection")
        {
            Description = "Collection filter"
        };
        var statusOption = new Option<string?>("--status")
        {
            Description = "Status filter"
        };

        var command = new Command("list", "List current month items")
        {
            collectionOption,
            statusOption
        };

        command.SetAction(async parseResult =>
        {
            try
            {
                var items = await listItemsUseCase!.ExecuteAsync(new ListItemsRequest
                {
                    Collection = ParseCollection(parseResult.GetValue(collectionOption)),
                    Status = ParseStatus(parseResult.GetValue(statusOption))
                }, cancellationToken);

                await WriteItemsAsync(items, standardOutput);
                return 0;
            }
            catch (Exception exception)
            {
                await standardError.WriteLineAsync(exception.Message);
                return 1;
            }
        });

        return command;
    }

    private Command BuildShowCommand(
        TextWriter standardOutput,
        TextWriter standardError,
        CancellationToken cancellationToken)
    {
        var publicRefArgument = new Argument<string>("ref")
        {
            Description = "Public ref"
        };

        var command = new Command("show", "Show item details")
        {
            publicRefArgument
        };

        command.SetAction(async parseResult =>
        {
            try
            {
                var publicRef = parseResult.GetValue(publicRefArgument)
                    ?? throw new InvalidOperationException("Public ref is required.");
                var item = await showItemUseCase!.ExecuteAsync(publicRef, cancellationToken);
                await WriteItemDetailAsync(item, standardOutput);
                return 0;
            }
            catch (Exception exception)
            {
                await standardError.WriteLineAsync(exception.Message);
                return 1;
            }
        });

        return command;
    }

    private Command BuildEditCommand(
        TextWriter standardOutput,
        TextWriter standardError,
        CancellationToken cancellationToken)
    {
        var publicRefArgument = new Argument<string>("ref") { Description = "Public ref" };
        var textArgument = new Argument<string>("text") { Description = "New content" };

        var command = new Command("edit", "Edit item content")
        {
            publicRefArgument,
            textArgument
        };

        command.SetAction(async parseResult =>
        {
            try
            {
                var publicRef = parseResult.GetValue(publicRefArgument)
                    ?? throw new InvalidOperationException("Public ref is required.");
                var content = parseResult.GetValue(textArgument)
                    ?? throw new InvalidOperationException("Content is required.");
                var item = await editItemUseCase!.ExecuteAsync(new EditItemRequest
                {
                    PublicRef = publicRef,
                    Content = content
                }, cancellationToken);
                await WriteItemDetailAsync(item, standardOutput);
                return 0;
            }
            catch (Exception exception)
            {
                await standardError.WriteLineAsync(exception.Message);
                return 1;
            }
        });

        return command;
    }

    private Command BuildMoveCommand(
        TextWriter standardOutput,
        TextWriter standardError,
        CancellationToken cancellationToken)
    {
        var publicRefArgument = new Argument<string>("ref") { Description = "Public ref" };
        var collectionArgument = new Argument<string>("collection") { Description = "Target collection" };

        var command = new Command("move", "Move an item to another collection")
        {
            publicRefArgument,
            collectionArgument
        };

        command.SetAction(async parseResult =>
        {
            try
            {
                var publicRef = parseResult.GetValue(publicRefArgument)
                    ?? throw new InvalidOperationException("Public ref is required.");
                var collection = ParseCollection(parseResult.GetValue(collectionArgument))
                    ?? throw new InvalidOperationException("Collection is required.");
                var item = await moveItemUseCase!.ExecuteAsync(new MoveItemRequest
                {
                    PublicRef = publicRef,
                    Collection = collection
                }, cancellationToken);
                await WriteItemDetailAsync(item, standardOutput);
                return 0;
            }
            catch (Exception exception)
            {
                await standardError.WriteLineAsync(exception.Message);
                return 1;
            }
        });

        return command;
    }

    private Command BuildPriorityCommand(
        TextWriter standardOutput,
        TextWriter standardError,
        CancellationToken cancellationToken)
    {
        var publicRefArgument = new Argument<string>("ref") { Description = "Public ref" };
        var priorityArgument = new Argument<string>("priority") { Description = "Priority value" };

        var command = new Command("priority", "Set item priority")
        {
            publicRefArgument,
            priorityArgument
        };

        command.SetAction(async parseResult =>
        {
            try
            {
                var publicRef = parseResult.GetValue(publicRefArgument)
                    ?? throw new InvalidOperationException("Public ref is required.");
                var priority = ParsePriority(parseResult.GetValue(priorityArgument));
                var item = await setItemPriorityUseCase!.ExecuteAsync(new SetItemPriorityRequest
                {
                    PublicRef = publicRef,
                    Priority = priority
                }, cancellationToken);
                await WriteItemDetailAsync(item, standardOutput);
                return 0;
            }
            catch (Exception exception)
            {
                await standardError.WriteLineAsync(exception.Message);
                return 1;
            }
        });

        return command;
    }

    private Command BuildTagCommand(
        string name,
        string description,
        bool addTag,
        TextWriter standardOutput,
        TextWriter standardError,
        CancellationToken cancellationToken)
    {
        var publicRefArgument = new Argument<string>("ref") { Description = "Public ref" };
        var tagArgument = new Argument<string>("tag") { Description = "Tag value" };

        var command = new Command(name, description)
        {
            publicRefArgument,
            tagArgument
        };

        command.SetAction(async parseResult =>
        {
            try
            {
                var publicRef = parseResult.GetValue(publicRefArgument)
                    ?? throw new InvalidOperationException("Public ref is required.");
                var tag = parseResult.GetValue(tagArgument)
                    ?? throw new InvalidOperationException("Tag is required.");

                var item = addTag
                    ? await tagItemUseCase!.ExecuteAsync(new TagItemRequest
                    {
                        PublicRef = publicRef,
                        Tag = tag
                    }, cancellationToken)
                    : await untagItemUseCase!.ExecuteAsync(new UntagItemRequest
                    {
                        PublicRef = publicRef,
                        Tag = tag
                    }, cancellationToken);

                await WriteItemDetailAsync(item, standardOutput);
                return 0;
            }
            catch (Exception exception)
            {
                await standardError.WriteLineAsync(exception.Message);
                return 1;
            }
        });

        return command;
    }

    private Command BuildSimpleMutationCommand(
        string name,
        string description,
        Func<string, CancellationToken, Task<ItemResult>> operation,
        TextWriter standardOutput,
        TextWriter standardError,
        CancellationToken cancellationToken)
    {
        var publicRefArgument = new Argument<string>("ref") { Description = "Public ref" };
        var command = new Command(name, description)
        {
            publicRefArgument
        };

        command.SetAction(async parseResult =>
        {
            try
            {
                var publicRef = parseResult.GetValue(publicRefArgument)
                    ?? throw new InvalidOperationException("Public ref is required.");
                var item = await operation(publicRef, cancellationToken);
                await WriteItemDetailAsync(item, standardOutput);
                return 0;
            }
            catch (Exception exception)
            {
                await standardError.WriteLineAsync(exception.Message);
                return 1;
            }
        });

        return command;
    }

    private Command BuildCollectionCommand(
        string name,
        string description,
        Func<CancellationToken, Task<IReadOnlyCollection<ItemResult>>> query,
        TextWriter standardOutput,
        TextWriter standardError,
        CancellationToken cancellationToken)
    {
        var command = new Command(name, description);
        command.SetAction(async _ =>
        {
            try
            {
                var items = await query(cancellationToken);
                await WriteItemsAsync(items, standardOutput);
                return 0;
            }
            catch (Exception exception)
            {
                await standardError.WriteLineAsync(exception.Message);
                return 1;
            }
        });

        return command;
    }

    private Command BuildImportCommand(
        TextWriter standardOutput,
        TextWriter standardError,
        CancellationToken cancellationToken)
    {
        var formatOption = new Option<string>("--format")
        {
            Description = "Format: json",
            DefaultValueFactory = _ => "json"
        };
        var inputArgument = new Argument<string>("path")
        {
            Description = "Input file"
        };

        var command = new Command("import", "Import data into the local data directory.")
        {
            formatOption,
            inputArgument
        };

        command.SetAction(async parseResult =>
        {
            try
            {
                var format = parseResult.GetValue(formatOption) ?? "json";
                var inputPath = parseResult.GetValue(inputArgument)
                    ?? throw new InvalidOperationException("Input path is required.");

                await importDataUseCase.ExecuteAsync(
                    new ImportDataRequest(inputPath, format),
                    cancellationToken);

                await standardOutput.WriteLineAsync(inputPath);
                return 0;
            }
            catch (Exception exception)
            {
                await standardError.WriteLineAsync(exception.Message);
                return 1;
            }
        });

        return command;
    }

    private Command BuildConfigCommand(
        TextWriter standardOutput,
        TextWriter standardError,
        CancellationToken cancellationToken)
    {
        var command = new Command("config", "Manage local application configuration.");
        command.Subcommands.Add(BuildConfigListCommand(standardOutput, standardError, cancellationToken));
        command.Subcommands.Add(BuildConfigGetCommand(standardOutput, standardError, cancellationToken));
        command.Subcommands.Add(BuildConfigSetCommand(standardOutput, standardError, cancellationToken));
        command.Subcommands.Add(BuildConfigPathCommand(standardOutput));
        return command;
    }

    private Command BuildConfigListCommand(
        TextWriter standardOutput,
        TextWriter standardError,
        CancellationToken cancellationToken)
    {
        var profileOption = CreateProfileOption();
        var command = new Command("list", "List configuration values")
        {
            profileOption
        };

        command.SetAction(async parseResult =>
        {
            try
            {
                var profile = parseResult.GetValue(profileOption) ?? "default";
                var settings = await listConfigurationUseCase.ExecuteAsync(profile, cancellationToken);
                foreach (var entry in settings.OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase))
                {
                    await standardOutput.WriteLineAsync($"{entry.Key}={entry.Value}");
                }

                return 0;
            }
            catch (Exception exception)
            {
                await standardError.WriteLineAsync(exception.Message);
                return 1;
            }
        });

        return command;
    }

    private Command BuildConfigGetCommand(
        TextWriter standardOutput,
        TextWriter standardError,
        CancellationToken cancellationToken)
    {
        var profileOption = CreateProfileOption();
        var keyArgument = new Argument<string>("key")
        {
            Description = "Show a configuration value"
        };

        var command = new Command("get", "Show a configuration value")
        {
            profileOption,
            keyArgument
        };

        command.SetAction(async parseResult =>
        {
            try
            {
                var profile = parseResult.GetValue(profileOption) ?? "default";
                var key = parseResult.GetValue(keyArgument)
                    ?? throw new InvalidOperationException("Configuration key is required.");
                var value = await getConfigurationUseCase.ExecuteAsync(key, profile, cancellationToken);
                await standardOutput.WriteLineAsync(value);
                return 0;
            }
            catch (Exception exception)
            {
                await standardError.WriteLineAsync(exception.Message);
                return 1;
            }
        });

        return command;
    }

    private Command BuildConfigSetCommand(
        TextWriter standardOutput,
        TextWriter standardError,
        CancellationToken cancellationToken)
    {
        var profileOption = CreateProfileOption();
        var keyArgument = new Argument<string>("key")
        {
            Description = "Configuration key"
        };
        var valueArgument = new Argument<string>("value")
        {
            Description = "Configuration value"
        };

        var command = new Command("set", "Set a configuration value")
        {
            profileOption,
            keyArgument,
            valueArgument
        };

        command.SetAction(async parseResult =>
        {
            try
            {
                var profile = parseResult.GetValue(profileOption) ?? "default";
                var key = parseResult.GetValue(keyArgument)
                    ?? throw new InvalidOperationException("Configuration key is required.");
                var value = parseResult.GetValue(valueArgument)
                    ?? throw new InvalidOperationException("Configuration value is required.");

                await setConfigurationUseCase.ExecuteAsync(key, value, profile, cancellationToken);
                await standardOutput.WriteLineAsync($"{key}={value}");
                return 0;
            }
            catch (Exception exception)
            {
                await standardError.WriteLineAsync(exception.Message);
                return 1;
            }
        });

        return command;
    }

    private Command BuildConfigPathCommand(TextWriter standardOutput)
    {
        var command = new Command("path", "Show paths used by the application");
        command.SetAction(_ =>
        {
            standardOutput.WriteLine(getConfigurationPathUseCase.Execute());
            return 0;
        });

        return command;
    }

    private Command BuildHistoryCommand(
        TextWriter standardOutput,
        TextWriter standardError,
        CancellationToken cancellationToken)
    {
        var command = new Command("history", "Manage stored history entries");
        command.Subcommands.Add(BuildHistoryClearCommand(standardOutput, standardError, cancellationToken));
        return command;
    }

    private Command BuildHistoryClearCommand(
        TextWriter standardOutput,
        TextWriter standardError,
        CancellationToken cancellationToken)
    {
        var monthOption = new Option<string?>("--month")
        {
            Description = "Clear history for a specific month file"
        };
        var allOption = new Option<bool>("--all")
        {
            Description = "Clear history from all month files"
        };
        var forceOption = new Option<bool>("--force")
        {
            Description = "Clear without confirmation"
        };

        var command = new Command("clear", "Clear stored history entries without deleting active items.")
        {
            monthOption,
            allOption,
            forceOption
        };

        command.SetAction(async parseResult =>
        {
            try
            {
                var monthValue = parseResult.GetValue(monthOption);
                var clearAll = parseResult.GetValue(allOption);

                if (clearAll && !string.IsNullOrWhiteSpace(monthValue))
                {
                    throw new ArgumentException("Use either --all or --month, not both.");
                }

                var request = clearAll
                    ? new ClearStoredHistoryRequest(All: true)
                    : TryParseMonthScope(monthValue, out var month, out var year)
                        ? new ClearStoredHistoryRequest(Month: month, Year: year)
                        : new ClearStoredHistoryRequest();

                await clearStoredHistoryUseCase.ExecuteAsync(request, cancellationToken);
                await standardOutput.WriteLineAsync(clearAll
                    ? "history cleared for all months"
                    : request.Month is not null
                        ? $"history cleared for {request.Month:00}_{request.Year:0000}"
                        : "history cleared for current month");
                return 0;
            }
            catch (Exception exception)
            {
                await standardError.WriteLineAsync(exception.Message);
                return 1;
            }
        });

        return command;
    }

    private static Option<string> CreateProfileOption() =>
        new("--profile")
        {
            Description = "Configuration profile to use",
            DefaultValueFactory = _ => "default"
        };

    private static bool TryParseMonthScope(string? value, out int month, out int year)
    {
        month = 0;
        year = 0;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var parts = value.Split('_', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length != 2
            || !int.TryParse(parts[0], out month)
            || !int.TryParse(parts[1], out year)
            || month is < 1 or > 12
            || year < 1)
        {
            throw new ArgumentException("Month scope must follow MM_YYYY.");
        }

        return true;
    }

    private static ItemType ResolveItemType(bool note, bool @event)
    {
        if (note && @event)
        {
            throw new ArgumentException("Use either --note or --event, not both.");
        }

        return note
            ? ItemType.Note
            : @event
                ? ItemType.Event
                : ItemType.Task;
    }

    private static ItemCollection? ParseCollection(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim().ToLowerInvariant() switch
        {
            "today" => ItemCollection.Today,
            "week" => ItemCollection.Week,
            "backlog" => ItemCollection.Backlog,
            "monthly" => ItemCollection.Monthly,
            "archived" => ItemCollection.Archived,
            _ => throw new ArgumentException($"Unsupported collection: {value}.")
        };
    }

    private static ItemStatus? ParseStatus(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim().ToLowerInvariant() switch
        {
            "open" => ItemStatus.Open,
            "in_progress" => ItemStatus.InProgress,
            "done" => ItemStatus.Done,
            "cancelled" => ItemStatus.Cancelled,
            "migrated" => ItemStatus.Migrated,
            _ => throw new ArgumentException($"Unsupported status: {value}.")
        };
    }

    private static Priority ParsePriority(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Priority.None;
        }

        return value.Trim().ToLowerInvariant() switch
        {
            "none" => Priority.None,
            "low" => Priority.Low,
            "medium" => Priority.Medium,
            "high" => Priority.High,
            _ => throw new ArgumentException($"Unsupported priority: {value}.")
        };
    }

    private static async Task WriteItemsAsync(
        IReadOnlyCollection<ItemResult> items,
        TextWriter standardOutput)
    {
        if (items.Count == 0)
        {
            await standardOutput.WriteLineAsync("No items found.");
            return;
        }

        foreach (var item in items.OrderBy(item => item.PublicRef, StringComparer.OrdinalIgnoreCase))
        {
            await standardOutput.WriteLineAsync(
                $"{item.PublicRef} [{item.Status.ToString().ToLowerInvariant()}] [{item.Collection.ToString().ToLowerInvariant()}] {item.Content}");
        }
    }

    private static async Task WriteItemDetailAsync(ItemResult item, TextWriter standardOutput)
    {
        await standardOutput.WriteLineAsync($"{item.PublicRef} {item.Content}");
        await standardOutput.WriteLineAsync($"type: {item.Type.ToString().ToLowerInvariant()}");
        await standardOutput.WriteLineAsync($"status: {item.Status.ToString().ToLowerInvariant()}");
        await standardOutput.WriteLineAsync($"collection: {item.Collection.ToString().ToLowerInvariant()}");
        await standardOutput.WriteLineAsync($"priority: {item.Priority.ToString().ToLowerInvariant()}");
        if (item.Tags.Count > 0)
        {
            await standardOutput.WriteLineAsync($"tags: {string.Join(", ", item.Tags)}");
        }
    }
}
