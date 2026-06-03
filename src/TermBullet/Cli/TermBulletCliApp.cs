using System.CommandLine;
using TermBullet.Application.Ai;
using TermBullet.Application.History;
using TermBullet.Application.Items;
using TermBullet.Application.Startup;
using TermBullet.Domain.Items;
using TermBullet.Services.Ai;
using TermBullet.Services.Configuration;

namespace TermBullet.Cli;

public sealed class TermBulletCliApp(
    ClearStoredHistoryUseCase clearStoredHistoryUseCase,
    TextWriter output,
    TextWriter error,
    CreateItemUseCase? createItemUseCase = null,
    ListItemsUseCase? listItemsUseCase = null,
    ShowItemUseCase? showItemUseCase = null,
    GetTodayItemsUseCase? getTodayItemsUseCase = null,
    GetWeekItemsUseCase? getWeekItemsUseCase = null,
    GetMonthItemsUseCase? getMonthItemsUseCase = null,
    GetBacklogItemsUseCase? getBacklogItemsUseCase = null,
    EditItemUseCase? editItemUseCase = null,
    MarkDoneItemUseCase? markDoneItemUseCase = null,
    CancelItemUseCase? cancelItemUseCase = null,
    MoveItemUseCase? moveItemUseCase = null,
    SetItemPriorityUseCase? setItemPriorityUseCase = null,
    TagItemUseCase? tagItemUseCase = null,
    UntagItemUseCase? untagItemUseCase = null,
    MigrateItemUseCase? migrateItemUseCase = null,
    DeleteItemUseCase? deleteItemUseCase = null,
    SearchItemsUseCase? searchItemsUseCase = null,
    TermBulletRuntimePaths? runtimePaths = null,
    Func<BuildAiPlanningRequest, CancellationToken, Task<GenerateAiPlanningDraftResult>>? generateAiPlanningDraft = null,
    Func<BuildAiPlanningRequest, CancellationToken, Task<GenerateAiPlanningResponseResult>>? generateAiPlanningResponse = null,
    Func<AiPlanningDraft, CancellationToken, Task<AiPlanningDraftApplyResult>>? applyAiPlanningDraft = null,
    Func<TermBulletConfig, string, CancellationToken, Task<AiPlanningProviderResponse>>? testAiProfileConnection = null,
    TextReader? input = null,
    Func<CancellationToken, Task>? startupAction = null)
{
    public Task<int> InvokeAsync(string[] args, CancellationToken cancellationToken = default)
    {
        return InvokeInternalAsync(args, cancellationToken);
    }

    public const string Version = "1.2.0";

    private async Task<int> InvokeInternalAsync(string[] args, CancellationToken cancellationToken)
    {
        if (HasVersionRequest(args))
        {
            await output.WriteLineAsync(Version);
            return 0;
        }

        if (startupAction is not null)
        {
            await startupAction(cancellationToken);
        }

        var rootCommand = BuildRootCommand(output, error, cancellationToken);
        var parseResult = rootCommand.Parse(args);

        if (HasHelpRequest(args))
        {
            await WriteHelpAsync(parseResult.CommandResult.Command, output);
            return 0;
        }

        if (parseResult.Errors.Count > 0)
        {
            foreach (var parseError in parseResult.Errors)
            {
                await error.WriteLineAsync(parseError.Message);
            }

            return 1;
        }

        return await parseResult.InvokeAsync(cancellationToken: cancellationToken);
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

        if (getMonthItemsUseCase is not null)
        {
            rootCommand.Subcommands.Add(BuildCollectionCommand(
                "month",
                "Show month items.",
                getMonthItemsUseCase.ExecuteAsync,
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
            rootCommand.Subcommands.Add(BuildMigrateCommand(standardOutput, standardError, cancellationToken));
        }

        if (deleteItemUseCase is not null)
        {
            rootCommand.Subcommands.Add(BuildDeleteCommand(standardOutput, standardError, cancellationToken));
        }

        if (searchItemsUseCase is not null)
        {
            rootCommand.Subcommands.Add(BuildSearchCommand(standardOutput, standardError, cancellationToken));
        }

        if (runtimePaths is not null)
        {
            rootCommand.Subcommands.Add(BuildPathCommand(standardOutput));
            rootCommand.Subcommands.Add(BuildAiCommand(standardOutput, standardError, cancellationToken));
        }

        rootCommand.Subcommands.Add(BuildHistoryCommand(standardOutput, standardError, cancellationToken));

        return rootCommand;
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
        var taskOption = new Option<bool>("--task")
        {
            Description = "Create as task (default)"
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
            Description = "Collection: today, week, month, backlog, notes, events"
        };
        var tagOption = new Option<string?>("--tag")
        {
            Description = "Item tag"
        };

        var command = new Command("add", "Create a new item")
        {
            textArgument,
            taskOption,
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
                    parseResult.GetValue(taskOption),
                    parseResult.GetValue(noteOption),
                    parseResult.GetValue(eventOption));
                var priority = ParsePriority(parseResult.GetValue(priorityOption));
                var collection = ParseCollection(parseResult.GetValue(collectionOption)) ?? DefaultCollectionFor(itemType);
                var tag = parseResult.GetValue(tagOption);

                var result = await createItemUseCase!.ExecuteAsync(new CreateItemRequest
                {
                    Type = itemType,
                    Content = content,
                    Collection = collection,
                    Priority = priority,
                    Tag = tag
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
        var toOption = new Option<string>("--to")
        {
            Description = "Destination collection",
            Required = true
        };

        var command = new Command("move", "Move an item to another collection")
        {
            publicRefArgument,
            toOption
        };

        command.SetAction(async parseResult =>
        {
            try
            {
                var publicRef = parseResult.GetValue(publicRefArgument)
                    ?? throw new InvalidOperationException("Public ref is required.");
                var collection = ParseCollection(parseResult.GetValue(toOption))
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

    private Command BuildMigrateCommand(
        TextWriter standardOutput,
        TextWriter standardError,
        CancellationToken cancellationToken)
    {
        var publicRefArgument = new Argument<string>("ref") { Description = "Public ref" };
        var collectionOption = new Option<string?>("--collection")
        {
            Description = "Destination collection: today, week, month, or backlog"
        };

        var command = new Command("migrate", "Migrate a task to a collection")
        {
            publicRefArgument,
            collectionOption
        };

        command.SetAction(async parseResult =>
        {
            try
            {
                var publicRef = parseResult.GetValue(publicRefArgument)
                    ?? throw new InvalidOperationException("Public ref is required.");
                var destination = parseResult.GetValue(collectionOption)
                    ?? throw new InvalidOperationException("Migration requires --collection <today|week|month|backlog>.");
                var destinationCollection = ParseCollection(destination)
                    ?? throw new InvalidOperationException($"Unsupported collection: {destination}.");

                var request = new MigrateItemRequest
                {
                    PublicRef = publicRef,
                    DestinationCollection = destinationCollection
                };

                var item = await migrateItemUseCase!.ExecuteAsync(request, cancellationToken);
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

    private Command BuildSearchCommand(
        TextWriter standardOutput,
        TextWriter standardError,
        CancellationToken cancellationToken)
    {
        var queryArgument = new Argument<string>("query")
        {
            Description = "Search query"
        };

        var command = new Command("search", "Search items in the current local data")
        {
            queryArgument
        };

        command.SetAction(async parseResult =>
        {
            try
            {
                var query = parseResult.GetValue(queryArgument)
                    ?? throw new InvalidOperationException("Search query is required.");
                var items = await searchItemsUseCase!.ExecuteAsync(new SearchItemsRequest
                {
                    Query = query
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

    private Command BuildDeleteCommand(
        TextWriter standardOutput,
        TextWriter standardError,
        CancellationToken cancellationToken)
    {
        var publicRefArgument = new Argument<string>("ref")
        {
            Description = "Public ref"
        };
        var forceOption = new Option<bool>("--force")
        {
            Description = "Remove without confirmation"
        };

        var command = new Command("delete", "Remove an item")
        {
            publicRefArgument,
            forceOption
        };

        command.SetAction(async parseResult =>
        {
            try
            {
                var publicRef = parseResult.GetValue(publicRefArgument)
                    ?? throw new InvalidOperationException("Public ref is required.");

                await deleteItemUseCase!.ExecuteAsync(publicRef, cancellationToken);
                await standardOutput.WriteLineAsync($"deleted: {publicRef}");
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

    private Command BuildPathCommand(TextWriter standardOutput)
    {
        var command = new Command("path", "Show local configuration and data paths");
        command.SetAction(async _ =>
        {
            await standardOutput.WriteLineAsync($"config: {runtimePaths!.ConfigPath}");
            await standardOutput.WriteLineAsync($"data_root: {runtimePaths.DataRoot}");
            await standardOutput.WriteLineAsync($"data: {runtimePaths.DataPath}");
            return 0;
        });

        return command;
    }

    private Command BuildAiCommand(
        TextWriter standardOutput,
        TextWriter standardError,
        CancellationToken cancellationToken)
    {
        var command = new Command("ai", "Manage AI planning configuration");
        if (generateAiPlanningDraft is not null)
        {
            command.Subcommands.Add(BuildAiPlanCommand(standardOutput, standardError, cancellationToken));
        }

        if (generateAiPlanningResponse is not null || generateAiPlanningDraft is not null)
        {
            command.Subcommands.Add(BuildAiChatCommand(standardOutput, standardError, cancellationToken));
        }

        var profileCommand = new Command("profile", "Manage AI connection profiles");
        profileCommand.Subcommands.Add(BuildAiProfileAddCommand(standardOutput, standardError, cancellationToken));
        profileCommand.Subcommands.Add(BuildAiProfileListCommand(standardOutput, standardError, cancellationToken));
        profileCommand.Subcommands.Add(BuildAiProfileUseCommand(standardOutput, standardError, cancellationToken));
        profileCommand.Subcommands.Add(BuildAiProfileShowCommand(standardOutput, standardError, cancellationToken));
        profileCommand.Subcommands.Add(BuildAiProfileTestCommand(standardOutput, standardError, cancellationToken));
        profileCommand.Subcommands.Add(BuildAiProfileRemoveCommand(standardOutput, standardError, cancellationToken));
        command.Subcommands.Add(profileCommand);
        return command;
    }

    private Command BuildAiChatCommand(
        TextWriter standardOutput,
        TextWriter standardError,
        CancellationToken cancellationToken)
    {
        var modeOption = new Option<string>("--mode")
        {
            Description = "Planning mode: new-project, new-weekly, revise-weekly, revise-project",
            DefaultValueFactory = _ => "new-project"
        };
        var tagOption = new Option<string?>("--tag")
        {
            Description = "Project tag for revise-project"
        };

        var command = new Command("chat", "Start an interactive AI planning chat")
        {
            modeOption,
            tagOption
        };

        command.SetAction(async parseResult =>
        {
            try
            {
                var mode = ParseAiPlanningMode(parseResult.GetValue(modeOption));
                var tag = NormalizeOptional(parseResult.GetValue(tagOption));
                if (mode == AiPlanningMode.ReviseProject && tag is null)
                {
                    throw new ArgumentException("Tag is required for revise-project planning.");
                }

                await RunAiChatAsync(mode, tag, standardOutput, cancellationToken);
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

    private async Task RunAiChatAsync(
        AiPlanningMode mode,
        string? tag,
        TextWriter standardOutput,
        CancellationToken cancellationToken)
    {
        AiPlanningDraft? currentDraft = null;
        var conversationHistory = new List<AiPlanningMessage>();
        await standardOutput.WriteLineAsync($"mode: {ToAiPlanningModeKey(mode)}");
        await standardOutput.WriteLineAsync("commands: /mode <mode> [tag], /apply, /discard, /exit");

        while (!cancellationToken.IsCancellationRequested)
        {
            await standardOutput.WriteAsync("you> ");
            var line = await (input ?? Console.In).ReadLineAsync();
            if (line is null)
            {
                break;
            }

            var message = line.Trim();
            if (message.Length == 0)
            {
                continue;
            }

            if (string.Equals(message, "/exit", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            if (string.Equals(message, "/discard", StringComparison.OrdinalIgnoreCase))
            {
                currentDraft = null;
                conversationHistory.Clear();
                await standardOutput.WriteLineAsync("draft discarded");
                continue;
            }

            if (message.StartsWith("/mode ", StringComparison.OrdinalIgnoreCase))
            {
                (mode, tag) = ParseAiChatModeCommand(message);
                currentDraft = null;
                conversationHistory.Clear();
                await standardOutput.WriteLineAsync($"mode: {ToAiPlanningModeKey(mode)}");
                continue;
            }

            if (string.Equals(message, "/apply", StringComparison.OrdinalIgnoreCase))
            {
                if (currentDraft is null)
                {
                    await standardOutput.WriteLineAsync("no draft to apply");
                    continue;
                }

                if (applyAiPlanningDraft is null)
                {
                    throw new InvalidOperationException("AI planning draft application is not available.");
                }

                if (!await ConfirmAiPlanningApplyAsync(standardOutput))
                {
                    await standardOutput.WriteLineAsync("apply cancelled");
                    continue;
                }

                var applyResult = await applyAiPlanningDraft(currentDraft, cancellationToken);
                await WriteAiPlanningApplyResultAsync(applyResult, standardOutput);
                currentDraft = null;
                conversationHistory.Clear();
                continue;
            }

            var historyForRequest = conversationHistory.ToArray();
            conversationHistory.Add(new AiPlanningMessage(AiPlanningMessageRole.User, message));
            var requireStructuredDraft = AiPlanningDraftIntent.RequiresStructuredDraft(message);
            var response = generateAiPlanningResponse is not null
                ? await generateAiPlanningResponse(new BuildAiPlanningRequest
                {
                    Mode = mode,
                    Tag = tag,
                    UserPrompt = message,
                    ConversationHistory = historyForRequest,
                    RequireStructuredDraft = requireStructuredDraft
                }, cancellationToken)
                : ToFlexibleResponse(await generateAiPlanningDraft!(new BuildAiPlanningRequest
                {
                    Mode = mode,
                    Tag = tag,
                    UserPrompt = message,
                    ConversationHistory = historyForRequest
                }, cancellationToken));

            if (response.Draft is null)
            {
                await standardOutput.WriteLineAsync($"ai> {response.AssistantMessage}");
                if (!string.IsNullOrWhiteSpace(response.AssistantMessage))
                {
                    conversationHistory.Add(new AiPlanningMessage(
                        AiPlanningMessageRole.Assistant,
                        response.AssistantMessage));
                }

                continue;
            }

            var result = new GenerateAiPlanningDraftResult(
                response.Draft,
                response.ProviderModel,
                response.ModelRequest);
            currentDraft = response.Draft;
            await standardOutput.WriteLineAsync("draft ready");
            await WriteAiPlanningDraftPreviewAsync(result, standardOutput);
            conversationHistory.Add(new AiPlanningMessage(
                AiPlanningMessageRole.Assistant,
                $"draft ready: {response.Draft.Actions.Count} actions. {response.Draft.Summary}"));
        }
    }

    private static GenerateAiPlanningResponseResult ToFlexibleResponse(GenerateAiPlanningDraftResult result) =>
        new(result.Draft, null, result.ProviderModel ?? string.Empty, result.ModelRequest);

    private Command BuildAiPlanCommand(
        TextWriter standardOutput,
        TextWriter standardError,
        CancellationToken cancellationToken)
    {
        var modeArgument = new Argument<string>("mode")
        {
            Description = "Planning mode: new-project, new-weekly, revise-weekly, revise-project"
        };
        var promptOption = new Option<string>("--prompt")
        {
            Description = "Planning prompt",
            Required = true
        };
        var tagOption = new Option<string?>("--tag")
        {
            Description = "Project tag for revise-project"
        };
        var applyOption = new Option<bool>("--apply")
        {
            Description = "Apply the generated draft after validation"
        };
        var yesOption = new Option<bool>("--yes")
        {
            Description = "Confirm draft application without an interactive prompt"
        };

        var command = new Command("plan", "Generate an AI planning draft preview")
        {
            modeArgument,
            promptOption,
            tagOption,
            applyOption,
            yesOption
        };

        command.SetAction(async parseResult =>
        {
            try
            {
                var mode = ParseAiPlanningMode(parseResult.GetValue(modeArgument));
                var tag = NormalizeOptional(parseResult.GetValue(tagOption));
                if (mode == AiPlanningMode.ReviseProject && tag is null)
                {
                    throw new ArgumentException("Tag is required for revise-project planning.");
                }

                var result = await generateAiPlanningDraft!(new BuildAiPlanningRequest
                {
                    Mode = mode,
                    Tag = tag,
                    UserPrompt = parseResult.GetValue(promptOption)
                        ?? throw new ArgumentException("Prompt is required.")
                }, cancellationToken);

                await WriteAiPlanningDraftPreviewAsync(result, standardOutput);
                if (parseResult.GetValue(applyOption))
                {
                    if (!parseResult.GetValue(yesOption)
                        && !await ConfirmAiPlanningApplyAsync(standardOutput))
                    {
                        await standardOutput.WriteLineAsync("apply cancelled");
                        return 0;
                    }

                    if (applyAiPlanningDraft is null)
                    {
                        throw new InvalidOperationException("AI planning draft application is not available.");
                    }

                    var applyResult = await applyAiPlanningDraft(result.Draft, cancellationToken);
                    await WriteAiPlanningApplyResultAsync(applyResult, standardOutput);
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

    private async Task<bool> ConfirmAiPlanningApplyAsync(TextWriter standardOutput)
    {
        await standardOutput.WriteAsync("confirm apply draft? ");
        var answer = await (input ?? Console.In).ReadLineAsync();
        return string.Equals(answer?.Trim(), "yes", StringComparison.OrdinalIgnoreCase)
            || string.Equals(answer?.Trim(), "y", StringComparison.OrdinalIgnoreCase);
    }

    private Command BuildAiProfileAddCommand(
        TextWriter standardOutput,
        TextWriter standardError,
        CancellationToken cancellationToken)
    {
        var nameArgument = new Argument<string>("name") { Description = "Profile name" };
        var providerOption = new Option<string>("--provider")
        {
            Description = "AI provider",
            Required = true
        };
        var modelOption = new Option<string>("--model")
        {
            Description = "AI model",
            Required = true
        };
        var baseUrlOption = new Option<string?>("--base-url")
        {
            Description = "Optional provider base URL"
        };
        var apiKeyEnvOption = new Option<string?>("--api-key-env")
        {
            Description = "Environment variable containing the API key"
        };
        var noApiKeyOption = new Option<bool>("--no-api-key")
        {
            Description = "Use a profile that does not require an API key"
        };

        var command = new Command("add", "Add or update an AI profile")
        {
            nameArgument,
            providerOption,
            modelOption,
            baseUrlOption,
            apiKeyEnvOption,
            noApiKeyOption
        };

        command.SetAction(async parseResult =>
        {
            try
            {
                var name = NormalizeProfileName(parseResult.GetValue(nameArgument));
                var provider = RequireNonEmpty(parseResult.GetValue(providerOption), "provider");
                var model = RequireNonEmpty(parseResult.GetValue(modelOption), "model");
                var baseUrl = NormalizeOptional(parseResult.GetValue(baseUrlOption));
                var apiKeyEnv = NormalizeOptional(parseResult.GetValue(apiKeyEnvOption));
                var noApiKey = parseResult.GetValue(noApiKeyOption);
                if (noApiKey && apiKeyEnv is not null)
                {
                    throw new ArgumentException("Use either --api-key-env or --no-api-key, not both.");
                }

                var config = await LoadConfigOrDefaultAsync(cancellationToken);
                var profiles = CopyProfiles(config);
                profiles[name] = new AiProfile(
                    provider,
                    model,
                    baseUrl,
                    noApiKey ? "none" : "environment",
                    noApiKey ? null : apiKeyEnv);
                var activeProfile = string.IsNullOrWhiteSpace(config.Ai?.ActiveProfile)
                    ? name
                    : config.Ai.ActiveProfile;
                await SaveConfigAsync(config with
                {
                    Ai = new AiConfiguration(activeProfile, profiles)
                }, cancellationToken);

                await standardOutput.WriteLineAsync($"profile saved: {name}");
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

    private Command BuildAiProfileListCommand(
        TextWriter standardOutput,
        TextWriter standardError,
        CancellationToken cancellationToken)
    {
        var command = new Command("list", "List AI profiles");
        command.SetAction(async _ =>
        {
            try
            {
                var config = await LoadConfigOrDefaultAsync(cancellationToken);
                var profiles = config.Ai?.Profiles ?? new Dictionary<string, AiProfile>();
                if (profiles.Count == 0)
                {
                    await standardOutput.WriteLineAsync("No AI profiles configured.");
                    return 0;
                }

                foreach (var pair in profiles.OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase))
                {
                    var marker = string.Equals(pair.Key, config.Ai?.ActiveProfile, StringComparison.OrdinalIgnoreCase)
                        ? "*"
                        : " ";
                    await standardOutput.WriteLineAsync(
                        $"{marker} {pair.Key}  {pair.Value.Provider}  {pair.Value.Model}");
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

    private Command BuildAiProfileUseCommand(
        TextWriter standardOutput,
        TextWriter standardError,
        CancellationToken cancellationToken)
    {
        var nameArgument = new Argument<string>("name") { Description = "Profile name" };
        var command = new Command("use", "Set the active AI profile")
        {
            nameArgument
        };

        command.SetAction(async parseResult =>
        {
            try
            {
                var name = NormalizeProfileName(parseResult.GetValue(nameArgument));
                var config = await LoadConfigOrDefaultAsync(cancellationToken);
                var profiles = CopyProfiles(config);
                if (!profiles.ContainsKey(name))
                {
                    throw new InvalidOperationException($"AI profile not found: {name}");
                }

                await SaveConfigAsync(config with
                {
                    Ai = new AiConfiguration(name, profiles)
                }, cancellationToken);
                await standardOutput.WriteLineAsync($"active profile: {name}");
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

    private Command BuildAiProfileShowCommand(
        TextWriter standardOutput,
        TextWriter standardError,
        CancellationToken cancellationToken)
    {
        var nameArgument = new Argument<string>("name") { Description = "Profile name" };
        var command = new Command("show", "Show an AI profile")
        {
            nameArgument
        };

        command.SetAction(async parseResult =>
        {
            try
            {
                var name = NormalizeProfileName(parseResult.GetValue(nameArgument));
                var (config, profile) = await LoadProfileAsync(name, cancellationToken);
                await WriteAiProfileAsync(name, profile, config.Ai?.ActiveProfile, standardOutput);
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

    private Command BuildAiProfileTestCommand(
        TextWriter standardOutput,
        TextWriter standardError,
        CancellationToken cancellationToken)
    {
        var nameArgument = new Argument<string>("name") { Description = "Profile name" };
        var command = new Command("test", "Validate an AI profile configuration")
        {
            nameArgument
        };

        command.SetAction(async parseResult =>
        {
            try
            {
                var name = NormalizeProfileName(parseResult.GetValue(nameArgument));
                var (_, profile) = await LoadProfileAsync(name, cancellationToken);
                ValidateAiProfile(name, profile);
                await standardOutput.WriteLineAsync($"profile valid: {name}");
                if (testAiProfileConnection is not null)
                {
                    var config = await LoadConfigOrDefaultAsync(cancellationToken);
                    var response = await testAiProfileConnection(config, name, cancellationToken);
                    await standardOutput.WriteLineAsync($"provider reachable: {response.Model ?? profile.Model}");
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

    private Command BuildAiProfileRemoveCommand(
        TextWriter standardOutput,
        TextWriter standardError,
        CancellationToken cancellationToken)
    {
        var nameArgument = new Argument<string>("name") { Description = "Profile name" };
        var command = new Command("remove", "Remove an AI profile")
        {
            nameArgument
        };

        command.SetAction(async parseResult =>
        {
            try
            {
                var name = NormalizeProfileName(parseResult.GetValue(nameArgument));
                var config = await LoadConfigOrDefaultAsync(cancellationToken);
                var profiles = CopyProfiles(config);
                if (!profiles.Remove(name))
                {
                    throw new InvalidOperationException($"AI profile not found: {name}");
                }

                var activeProfile = string.Equals(config.Ai?.ActiveProfile, name, StringComparison.OrdinalIgnoreCase)
                    ? null
                    : config.Ai?.ActiveProfile;
                await SaveConfigAsync(config with
                {
                    Ai = profiles.Count == 0
                        ? null
                        : new AiConfiguration(activeProfile, profiles)
                }, cancellationToken);

                await standardOutput.WriteLineAsync($"profile removed: {name}");
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

    private async Task<TermBulletConfig> LoadConfigOrDefaultAsync(CancellationToken cancellationToken)
    {
        var service = CreateConfigService();
        return await service.LoadAsync(cancellationToken)
            ?? new TermBulletConfig(runtimePaths!.DataRoot);
    }

    private async Task SaveConfigAsync(TermBulletConfig config, CancellationToken cancellationToken)
    {
        var service = CreateConfigService();
        await service.SaveAsync(config, cancellationToken);
    }

    private async Task<(TermBulletConfig Config, AiProfile Profile)> LoadProfileAsync(
        string name,
        CancellationToken cancellationToken)
    {
        var config = await LoadConfigOrDefaultAsync(cancellationToken);
        var profiles = config.Ai?.Profiles ?? new Dictionary<string, AiProfile>();
        if (!profiles.TryGetValue(name, out var profile))
        {
            throw new InvalidOperationException($"AI profile not found: {name}");
        }

        return (config, profile);
    }

    private TermBulletConfigService CreateConfigService()
    {
        var installDirectory = Path.GetDirectoryName(runtimePaths!.ConfigPath)
            ?? throw new InvalidOperationException("Runtime config path is invalid.");
        return new TermBulletConfigService(installDirectory);
    }

    private static Dictionary<string, AiProfile> CopyProfiles(TermBulletConfig config) =>
        new(config.Ai?.Profiles ?? new Dictionary<string, AiProfile>(), StringComparer.OrdinalIgnoreCase);

    private static string NormalizeProfileName(string? name)
    {
        var normalized = NormalizeOptional(name);
        if (normalized is null)
        {
            throw new ArgumentException("Profile name is required.");
        }

        return normalized;
    }

    private static string RequireNonEmpty(string? value, string name) =>
        NormalizeOptional(value) ?? throw new ArgumentException($"{name} is required.");

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static AiPlanningMode ParseAiPlanningMode(string? value)
    {
        var normalized = NormalizeOptional(value)?.ToLowerInvariant();
        return normalized switch
        {
            "new-project" => AiPlanningMode.NewProject,
            "new-weekly" => AiPlanningMode.NewWeekly,
            "revise-weekly" => AiPlanningMode.ReviseWeekly,
            "revise-project" => AiPlanningMode.ReviseProject,
            _ => throw new ArgumentException($"Unsupported AI planning mode: {value}.")
        };
    }

    private static (AiPlanningMode Mode, string? Tag) ParseAiChatModeCommand(string command)
    {
        var parts = command.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length < 2)
        {
            throw new ArgumentException("Mode command must be /mode <mode> [tag].");
        }

        var mode = ParseAiPlanningMode(parts[1]);
        var tag = parts.Length > 2 ? parts[2] : null;
        if (mode == AiPlanningMode.ReviseProject && string.IsNullOrWhiteSpace(tag))
        {
            throw new ArgumentException("Tag is required for revise-project planning.");
        }

        return (mode, tag);
    }

    private static string ToAiPlanningModeKey(AiPlanningMode mode) =>
        mode switch
        {
            AiPlanningMode.NewProject => "new-project",
            AiPlanningMode.NewWeekly => "new-weekly",
            AiPlanningMode.ReviseWeekly => "revise-weekly",
            AiPlanningMode.ReviseProject => "revise-project",
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, "Unsupported AI planning mode.")
        };

    private static async Task WriteAiPlanningDraftPreviewAsync(
        GenerateAiPlanningDraftResult result,
        TextWriter standardOutput)
    {
        if (!string.IsNullOrWhiteSpace(result.ProviderModel))
        {
            await standardOutput.WriteLineAsync($"model: {result.ProviderModel}");
        }

        await standardOutput.WriteLineAsync($"mode: {result.Draft.Mode}");
        await standardOutput.WriteLineAsync($"summary: {result.Draft.Summary}");
        await standardOutput.WriteLineAsync("actions:");
        for (var index = 0; index < result.Draft.Actions.Count; index++)
        {
            var action = result.Draft.Actions[index];
            await standardOutput.WriteLineAsync($"{index + 1}. {action.Type}");
            await WriteOptionalDraftFieldAsync("name", action.Name, standardOutput);
            await WriteOptionalDraftFieldAsync("public_ref", action.PublicRef, standardOutput);
            await WriteOptionalDraftFieldAsync("tag", action.Tag, standardOutput);
            await WriteOptionalDraftFieldAsync("collection", action.Collection, standardOutput);
            await WriteOptionalDraftFieldAsync("priority", action.Priority, standardOutput);
            await WriteOptionalDraftFieldAsync("content", action.Content, standardOutput);
            await WriteOptionalDraftFieldAsync("description", action.Description, standardOutput);
        }
    }

    private static async Task WriteOptionalDraftFieldAsync(
        string name,
        string? value,
        TextWriter standardOutput)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            await standardOutput.WriteLineAsync($"   {name}: {value}");
        }
    }

    private static async Task WriteAiPlanningApplyResultAsync(
        AiPlanningDraftApplyResult result,
        TextWriter standardOutput)
    {
        await standardOutput.WriteLineAsync("applied:");
        for (var index = 0; index < result.Actions.Count; index++)
        {
            var action = result.Actions[index];
            var details = new[]
                {
                    action.PublicRef,
                    action.Tag is null ? null : $"tag={action.Tag}",
                    action.Collection is null ? null : $"collection={action.Collection}"
                }
                .Where(value => !string.IsNullOrWhiteSpace(value));

            await standardOutput.WriteLineAsync(
                $"{index + 1}. {action.Type} {string.Join(' ', details)}".TrimEnd());
        }
    }

    private static void ValidateAiProfile(string name, AiProfile profile)
    {
        _ = RequireNonEmpty(profile.Provider, "provider");
        _ = RequireNonEmpty(profile.Model, "model");
        var keySource = NormalizeOptional(profile.ApiKeySource) ?? "environment";
        if (string.Equals(keySource, "none", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (!string.Equals(keySource, "environment", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Unsupported API key source for profile {name}: {profile.ApiKeySource}");
        }

        var envName = RequireNonEmpty(profile.ApiKeyEnv, "api_key_env");
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(envName)))
        {
            throw new InvalidOperationException($"API key environment variable is not set for profile {name}: {envName}");
        }
    }

    private static async Task WriteAiProfileAsync(
        string name,
        AiProfile profile,
        string? activeProfile,
        TextWriter standardOutput)
    {
        await standardOutput.WriteLineAsync($"profile: {name}");
        await standardOutput.WriteLineAsync($"active: {string.Equals(name, activeProfile, StringComparison.OrdinalIgnoreCase).ToString().ToLowerInvariant()}");
        await standardOutput.WriteLineAsync($"provider: {profile.Provider}");
        await standardOutput.WriteLineAsync($"model: {profile.Model}");
        if (!string.IsNullOrWhiteSpace(profile.BaseUrl))
        {
            await standardOutput.WriteLineAsync($"base_url: {profile.BaseUrl}");
        }

        await standardOutput.WriteLineAsync($"api_key_source: {profile.ApiKeySource}");
        if (!string.IsNullOrWhiteSpace(profile.ApiKeyEnv))
        {
            await standardOutput.WriteLineAsync($"api_key_env: {profile.ApiKeyEnv}");
        }
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

    private static ItemType ResolveItemType(bool task, bool note, bool @event)
    {
        if ((task && note) || (task && @event) || (note && @event))
        {
            throw new ArgumentException("Use only one type flag: --task, --note, or --event.");
        }

        return note
            ? ItemType.Note
            : @event
                ? ItemType.Event
                : ItemType.Task;
    }

    private static bool HasHelpRequest(string[] args) =>
        args.Any(arg => string.Equals(arg, "--help", StringComparison.Ordinal) || string.Equals(arg, "-h", StringComparison.Ordinal));

    private static bool HasVersionRequest(string[] args) =>
        args.Any(arg => string.Equals(arg, "--version", StringComparison.Ordinal) || string.Equals(arg, "-v", StringComparison.Ordinal));

    private static async Task WriteHelpAsync(Command command, TextWriter writer)
    {
        await writer.WriteLineAsync(command.Description);
        await writer.WriteLineAsync();
        await writer.WriteLineAsync("Usage:");
        await writer.WriteLineAsync($"  {BuildUsage(command)}");

        if (command.Arguments.Count > 0)
        {
            await writer.WriteLineAsync();
            await writer.WriteLineAsync("Arguments:");
            foreach (var argument in command.Arguments)
            {
                await writer.WriteLineAsync($"  <{argument.Name}>");
            }
        }

        if (command.Subcommands.Count > 0)
        {
            await writer.WriteLineAsync();
            await writer.WriteLineAsync("Subcommands:");
            foreach (var subcommand in command.Subcommands.OrderBy(subcommand => subcommand.Name, StringComparer.OrdinalIgnoreCase))
            {
                await writer.WriteLineAsync($"  {subcommand.Name}");
            }
        }

        var visibleOptions = command.Options
            .Where(option => !option.Aliases.Any(alias =>
                string.Equals(alias, "--help", StringComparison.OrdinalIgnoreCase)
                || string.Equals(alias, "-h", StringComparison.OrdinalIgnoreCase)))
            .OrderBy(option => option.Name.TrimStart('-'), StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (visibleOptions.Length > 0)
        {
            await writer.WriteLineAsync();
            await writer.WriteLineAsync("Options:");
            foreach (var option in visibleOptions)
            {
                var aliases = string.Join(", ", option.Aliases
                    .Append(option.Name)
                    .Select(FormatAlias)
                    .Where(alias => !string.IsNullOrWhiteSpace(alias))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(alias => alias.Length)
                    .ThenBy(alias => alias, StringComparer.OrdinalIgnoreCase));
                await writer.WriteLineAsync($"  {aliases}    {GetOptionDescription(option)}");
            }
        }

        await writer.WriteLineAsync();
        await writer.WriteLineAsync("  -h, --help    Show help");
    }

    private static string? GetOptionDescription(Option option) =>
        IsVersionOption(option)
            ? "Show version"
            : option.Description;

    private static bool IsVersionOption(Option option) =>
        string.Equals(option.Name, "version", StringComparison.OrdinalIgnoreCase)
        || string.Equals(option.Name, "--version", StringComparison.OrdinalIgnoreCase)
        || option.Aliases.Any(alias =>
            string.Equals(alias, "--version", StringComparison.OrdinalIgnoreCase)
            || string.Equals(alias, "-v", StringComparison.OrdinalIgnoreCase));

    private static string BuildUsage(Command command)
    {
        var segments = new List<string> { "termbullet" };
        var commandPath = GetCommandPath(command);

        if (commandPath.Count == 0)
        {
            segments.Add("[command]");
        }
        else
        {
            segments.AddRange(commandPath);
        }

        foreach (var argument in command.Arguments)
        {
            segments.Add($"<{argument.Name}>");
        }

        if (command.Options.Count > 0)
        {
            segments.Add("[options]");
        }

        return string.Join(' ', segments.Where(segment => !string.IsNullOrWhiteSpace(segment) && segment != "root"));
    }

    private static IReadOnlyList<string> GetCommandPath(Command command)
    {
        if (command is RootCommand)
        {
            return [];
        }

        var names = new List<string>();
        Symbol? current = command;

        while (current is Command currentCommand)
        {
            if (currentCommand is RootCommand)
            {
                break;
            }

            if (!string.IsNullOrWhiteSpace(currentCommand.Name) && !string.Equals(currentCommand.Name, "root", StringComparison.OrdinalIgnoreCase))
            {
                names.Add(currentCommand.Name);
            }

            current = currentCommand.Parents.OfType<Symbol>().FirstOrDefault();
        }

        names.Reverse();
        return names;
    }

    private static string FormatAlias(string alias)
    {
        if (string.IsNullOrWhiteSpace(alias))
        {
            return alias;
        }

        if (alias.StartsWith("--", StringComparison.Ordinal) || alias.StartsWith("-", StringComparison.Ordinal))
        {
            return alias;
        }

        return alias.Length == 1 ? $"-{alias}" : $"--{alias}";
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
            "month" => ItemCollection.Month,
            "backlog" => ItemCollection.Backlog,
            "note" or "notes" => ItemCollection.Notes,
            "event" or "events" => ItemCollection.Events,
            _ => throw new ArgumentException($"Unsupported collection: {value}.")
        };
    }

    private static ItemCollection DefaultCollectionFor(ItemType type) =>
        type switch
        {
            ItemType.Note => ItemCollection.Notes,
            ItemType.Event => ItemCollection.Events,
            _ => ItemCollection.Today
        };

    private static ItemStatus? ParseStatus(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim().ToLowerInvariant() switch
        {
            "open" => ItemStatus.Open,
            "done" => ItemStatus.Done,
            "cancelled" => ItemStatus.Cancelled,
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
        await standardOutput.WriteLineAsync($"tag: {item.Tag}");
    }
}
