# TermBullet - Product Spec

> Consolidated product requirements, official command tree, and TUI screen specification.
>
> TermBullet is intended to be developed, documented, and distributed as an open source project for a global audience.

## Table of Contents

- [Part I - Product Requirements](#part-i---product-requirements)
- [Part II - Official Command Tree](#part-ii---official-command-tree)
- [Part III - TUI Screens Spec](#part-iii---tui-screens-spec)

---

## Part I - Product Requirements

# TermBullet - Requirements Document

## 1. Product Vision

**TermBullet** is a personal productivity application inspired by the **Bullet Journal** philosophy, built for the terminal and designed especially for developers and technical users.

The product provides two primary interaction modes over the same functional core:

- **TUI** as the main visual interface, with screens and panels similar to tools such as LazyDocker and LazyGit.
- **CLI** as a fast interface for capture, manipulation, and lookup without opening the TUI.

The project should be built as an open source tool, with public documentation, contribution-friendly architecture, and global naming/content in English.

The product evolution must follow an architecture prepared from the beginning to support:

- local-first offline operation;
- optional AI integration;
- optional Google Calendar integration;
- synchronization between machines and a future optional cloud layer.

---

## 2. Product Objective

Allow users to organize tasks, notes, events, and personal reviews directly from the terminal with a fast, predictable, and pleasant workflow, without initially depending on internet access, an online account, or external services.

The product must be useful in its initial purely local version, and then grow progressively without requiring structural rewrites at each new version.

---

## 3. Product Principles

TermBullet development must follow these principles:

1. **Local-first**
   - The system must work fully offline in V1.
   - The local database must be the user's primary operational source.

2. **CLI + TUI as first-class interfaces**
   - The TUI is not the only entry point.
   - Everything essential in the system must also be possible through the CLI.

3. **Optional AI**
   - The product must not depend on AI to be useful.
   - AI acts as a planning assistant, not as the application core.

4. **Optional integrations**
   - Google Calendar, sync, and cloud are future extensions.
   - The absence of those integrations must not compromise local usage.

5. **Evolutionary architecture**
   - The system must be prepared for V2, V3, and V4 from the beginning.
   - Functional growth must not require architectural rupture.

6. **Terminal-first experience**
   - Usage must be fast, legible, and friendly for people who live in a shell.
   - The TUI must feel like a serious terminal tool.

7. **Open source by design**
   - The project must be understandable and maintainable by contributors.
   - Documentation, command names, examples, and user-facing language must be English-first.
   - License, contribution rules, and governance should be added before the first public release.

---

## 4. Target Audience

### 4.1 Primary Audience

- Developers.
- Technical professionals who use the terminal frequently.
- Users who prefer local, fast, keyboard-driven tools.

### 4.2 Secondary Audience

- Advanced productivity users.
- People who want a digital bullet journal without a heavy graphical interface.

---

## 5. Product Evolution Scope

## 5.1 V1 - Pure Offline TermBullet

### Goal

Deliver the product core as a fully local, offline, functional tool.

### Includes

- Local data store.
- Main TUI.
- Basic and robust CLI.
- Creation and manipulation of internal tasks, notes, and events.
- Daily context.
- Weekly context.
- Backlog.
- Search.
- Basic editing.
- Item migration and movement.
- Local configuration.
- Basic export and import.

### Does Not Include

- AI.
- Google Calendar.
- Synchronization between machines.
- Cloud account.

---

## 5.2 V2 - AI Integration

### Goal

Add intelligent planning and review assistance without compromising local operation.

### Includes

- AI configuration module.
- BYOK support.
- Provider, model, key, and endpoint configuration.
- Daily planning.
- Daily review.
- Task breakdown.
- Backlog prioritization.
- Brain dump transformation into structured items.
- Preview before persisting suggestions.

### Does Not Necessarily Include

- A complex user-configurable agent.
- Autonomous workflows.
- AI dependency for common usage.

---

## 5.3 V3 - Google Calendar Integration

### Goal

Add real schedule context to the user's planning workflow.

### Includes

- Optional calendar integration module.
- Google Calendar connection.
- Reading events for the day.
- Showing calendar events in the TUI.
- Using calendar events as context for AI planning.
- Creating events from TermBullet.

### Does Not Initially Include

- Complete bidirectional synchronization.
- Complex reconciliation automations.

---

## 5.4 V4 - Machine Sync + Cloud System

### Goal

Enable safe and consistent usage across multiple machines while preserving the local-first philosophy.

### Includes

- Synchronization engine.
- Entity-level sync model.
- Optional sync/cloud server.
- Authentication.
- Push/pull.
- Conflict resolution.
- Sync history.

### Must Not Do

- Make cloud mandatory.
- Turn the local database into a disposable cache.
- Synchronize the physical local database file directly.

---

## 6. High-Level Architecture

The system must be structured with clear separation between domain, use cases, and interfaces.

### Recommended Layers

- **Core**
  - Entities.
  - Business rules.
  - States.
  - Identification policies.
  - Migration policies.

- **Application**
  - Use cases.
  - Orchestration services.
  - Input and output contracts.

- **Infrastructure**
  - Local persistence.
  - Export/import.
  - AI integration.
  - Google Calendar integration.
  - Future sync.

- **CLI**
  - Commands.
  - Handlers.
  - Text rendering.

- **TUI**
  - Screens.
  - Navigation.
  - Shortcuts.
  - Panels and visual context.

### Essential Rule

CLI and TUI must reuse the same Application layer use cases.

### Official Technology Stack

The official implementation stack for TermBullet is:

- **.NET 8 / C#**
  - Main platform and implementation language.
  - Provides a mature runtime, strong tooling, and cross-platform distribution options.

- **Terminal.Gui**
  - TUI framework.
  - Used to build the panel/window-based interface described in this specification.

- **System.CommandLine**
  - CLI framework.
  - Used to implement the official command tree, argument parsing, options, and help output.

- **SQLite**
  - Local offline database for V1.
  - Serves as the local-first operational store.

- **PostgreSQL**
  - Future backend database for V4 sync/cloud.
  - Used by the optional server-side synchronization/cloud layer, not as a replacement for the local database.

This stack supports the product goals: local-first operation, fast terminal usage, a rich TUI, a robust CLI, and an architecture prepared for AI, calendar integration, and cross-device synchronization.

Official references:

- [.NET 8 / C#](https://learn.microsoft.com/pt-br/dotnet/core/whats-new/dotnet-8/overview)
- [Terminal.Gui](https://github.com/gui-cs/Terminal.Gui)
- [System.CommandLine](https://learn.microsoft.com/en-us/dotnet/standard/commandline/)
- [SQLite](https://www.sqlite.org/docs.html)
- [PostgreSQL](https://www.postgresql.org/docs/)

---

## 7. V1 Functional Requirements

## 7.1 Supported Items

The system must support these primary item types:

- **Task**
- **Note**
- **Event**

## 7.2 Basic Operations

The system must allow users to:

- Create an item.
- List items.
- Show item details.
- Edit an item.
- Mark an item as done.
- Mark an item as cancelled.
- Migrate an item.
- Move an item between collections.
- Delete an item.
- Add and remove tags.
- Set priority.
- Search items.

## 7.3 Minimum Collections

The system must support at least:

- Today.
- Week.
- Backlog.

## 7.4 Navigation

The system must allow users to:

- Open the TUI as the main experience.
- Manipulate items directly through the CLI.
- Move easily between visual usage and fast command usage.

## 7.5 Export and Import

The system must support local export and import in simple formats for backup or future migration.

---

## 8. TUI Requirements

## 8.1 Visual Principle

The TUI must use the concept of **screens and panels**, inspired by applications such as LazyDocker.

## 8.2 Expected Characteristics

- Panel-based layout.
- Keyboard navigation.
- Clear focus by panel.
- Visual item selection.
- Item list with related details.
- Operational dashboard feeling.

## 8.3 Suggested Screen Structure

Conceptual example:

- Left panel: collections or navigation.
- Center panel: items in the current collection.
- Right panel: selected item details.
- Footer: available shortcuts and actions.

## 8.4 TUI UX Requirements

- Respond well to heavy keyboard usage.
- Show short, readable identifiers.
- Show temporal context clearly.
- Support continuous usage without a mouse.

---

## 9. CLI Requirements

## 9.1 CLI Role

The CLI must be a primary product interface, not a secondary feature.

## 9.2 The CLI Must Support in V1

- Fast capture of new items.
- Daily context lookup.
- Weekly context lookup.
- Backlog lookup.
- Item editing and state changes.
- Search.
- Opening the TUI.

## 9.3 CLI UX Requirements

- Short syntax.
- Predictable commands.
- Legible output.
- Future JSON output support.
- Behavior consistent with the TUI.

---

## 10. Entity Identification

## 10.1 General Guideline

The system must not use only simple numbers as the primary identifier in the user experience.

The identification model must separate:

1. **Global internal ID**
   - Unique.
   - Stable.
   - Immutable.
   - Prepared for future sync.

2. **Public Ref**
   - Short.
   - Readable.
   - Designed for CLI and TUI usage.

## 10.2 Official Public Ref

The public reference must follow this format:

```text
<type>-<MMDD>-<sequence>
```

### Prefixes

- `t` = task
- `n` = note
- `e` = event

### Examples

- `t-0422-1`
- `t-0422-2`
- `n-0422-1`
- `e-0422-1`

## 10.3 Generation Rules

- The sequence must be independent by type and day.
- The public reference must be persisted.
- The public reference must not be reused.
- The internal ID remains the real entity identity.

## 10.4 Future Rule

In versions with synchronization between machines, the internal ID must be the integrity basis and the public ref must remain only the main human-facing reference.

---

## 11. Persistence Requirements

## 11.1 Initial Persistence

In V1, the system must operate with local storage.

## 11.2 Minimum Persistence Requirements

Each relevant entity must record:

- Internal ID.
- `public_ref`.
- Type.
- Content.
- Status.
- Creation date.
- Update date.
- Current collection.
- Priority.
- Tags.

## 11.3 Future-Ready Architectural Requirements

Even in V1, the model must consider:

- Basic versioning.
- Consistent timestamps.
- Separation between internal identity and public reference.
- Future possibility of entity-level synchronization.

---

## 12. AI Integration Requirements (V2)

## 12.1 Adopted Practice

The platform should provide only the **AI configuration module**.

The user must configure their own provider/model manually.

## 12.2 Adopted Model

**BYOK - Bring Your Own Key**

## 12.3 The Platform Must Allow Configuration Of

- Provider.
- Model.
- API key.
- Optional base URL.
- Basic parameters.

## 12.4 The Platform Must Not Require

- A central product account for AI usage.
- A complex agent manually created by the user.
- Mandatory AI dependency.

## 12.5 Expected Internal Profiles

Internal profiles controlled by the app, such as:

- `plan-day`
- `review-day`
- `breakdown-task`
- `prioritize-backlog`

---

## 13. Google Calendar Requirements (V3)

## 13.1 Strategy

Google Calendar integration must be optional and modular.

## 13.2 Expected Capabilities

### Reading

- Bring in daily events.
- Bring in schedule context.
- Show day occupancy.

### Future Writing

- Create an event from a local item.
- Keep a basic link between a local item and an external event.

## 13.3 Product Rule

A task must not be automatically converted into an event.

Tasks and Events must remain distinct concepts.

---

## 14. Sync and Cloud Requirements (V4)

## 14.1 Philosophy

Synchronization must follow a **local-first** model.

## 14.2 Guidelines

- Each machine keeps a complete local database.
- Sync is an additional layer.
- Cloud is not mandatory for product usage.

## 14.3 Technical Rules

- Entity-level synchronization.
- Do not synchronize the physical local database file directly.
- Support conflicts as a normal part of operation.

## 14.4 Conflict Requirements

The system must be prepared for:

- Concurrent changes.
- Automatic resolution in simple cases.
- Manual review in relevant cases.

---

## 15. Non-Functional Requirements

## 15.1 Performance

- Fast startup.
- Responsive basic commands.
- Fluid TUI navigation.

## 15.2 Usability

- Low learning curve for main commands.
- Pleasant terminal experience.
- High legibility in TUI and CLI.

## 15.3 Reliability

- Stable offline operation.
- Local data consistency.
- Viable export for backup.

## 15.4 Evolution

- Modular structure.
- Easy expansion for new integrations.
- Low coupling between domain and external providers.

## 15.5 Open Source Maintainability

- Clear internal boundaries for contributors.
- English-first user-facing text and documentation.
- Public architectural decisions for significant design choices.
- Contribution and licensing documentation before public release.

---

## 16. Architectural Evolution Requirements

V1 development must already prepare the ground for V2, V3, and V4.

### This Means V1 Must Already Start With

- Layer separation.
- Growth-oriented modeling.
- Robust internal identity.
- Short public reference.
- Extension points for integrations.
- Persistence ready for future versioning.
- Use cases independent from interfaces.

### It Is Not Acceptable That

- AI requires domain refactoring.
- Google Calendar requires rebuilding the event model.
- Sync requires replacing the entire entity identification model.

---

## 17. V1 Acceptance Criteria

V1 will be considered adequate when:

1. Users can use TermBullet locally without internet access.
2. The TUI offers consistent screen and panel navigation.
3. The CLI allows manipulation of the main items without opening the TUI.
4. Tasks, notes, and events can be created, listed, edited, and changed.
5. The system uses readable public refs in the official format.
6. Basic export and import are available.
7. The architecture is prepared for future AI, calendar, and sync.
8. Project documentation is English-first and ready for open source publication.

---

## 18. Consolidated Roadmap

## V1 - Offline Core

- TUI.
- CLI.
- Tasks, notes, and events.
- Today, Week, and Backlog.
- Local persistence.
- Export/import.
- Human-readable identification.

## V2 - AI Planning

- AI configuration module.
- `plan-day`.
- `review-day`.
- Breakdown.
- Prioritization.

## V3 - Google Calendar

- Reading daily events.
- Displaying events in the dashboard.
- Using schedule context for AI.
- Creating events.

## V4 - Sync + Cloud

- Synchronization between machines.
- Local-first.
- Optional cloud.
- Conflicts.
- Sync history.

---

## 19. Executive Summary

TermBullet should start as a local tool, strong in the terminal, with hybrid operation between TUI and CLI.

V1 must be useful by itself, without depending on AI, Google Calendar, or cloud.

At the same time, its architecture must be planned from the beginning to naturally absorb the next product phases:

- AI for planning.
- External calendar for context.
- Sync between machines for multi-device usage.

The correct foundation is: **local-first, terminal-first, open source, evolutionary, and modular**.

---

## Part II - Official Command Tree

# TermBullet - Official Command Tree

## Overview

TermBullet must provide two main usage modes:

- **TUI**, opened by default when no command is provided.
- **CLI**, for fast capture and manipulation without opening the visual interface.

---

## `termbullet --help`

```text
TermBullet - Bullet Journal for Terminal

Usage:
  termbullet [command] [arguments] [options]

If no command is provided, the main TUI is opened.

Main commands:
  tui                    Open the TUI interface
  add                    Create a new item
  list                   List items
  today                  Show today's context
  week                   Show the week context
  backlog                Show backlog items
  show                   Show item details
  edit                   Edit an existing item
  done                   Mark an item as done
  cancel                 Mark an item as cancelled
  migrate                Migrate an item to another collection/period
  move                   Move an item to another collection
  delete                 Remove an item
  tag                    Add a tag to an item
  untag                  Remove a tag from an item
  priority               Set item priority
  search                 Search items
  export                 Export data
  import                 Import data
  config                 Manage local configuration

Global options:
  -h, --help             Show help
  -v, --version          Show version
      --json             JSON output when supported
      --no-color         Disable colors
      --db <path>        Alternative path for the local database
      --profile <name>   Configuration profile to use

Examples:
  termbullet
  termbullet add "fix jwt authentication"
  termbullet add --note "error happens when audience is empty"
  termbullet today
  termbullet done t-0422-1
  termbullet search "jwt"
```

---

## Official Tree

```text
termbullet
├── tui
├── add
├── list
├── today
├── week
├── backlog
├── show
├── edit
├── done
├── cancel
├── migrate
├── move
├── delete
├── tag
├── untag
├── priority
├── search
├── export
├── import
└── config
    ├── list
    ├── get
    ├── set
    └── path
```

---

## Item Identification in CLI and TUI

TermBullet must not expose only simple numeric IDs as the user's main reference.

### Adopted Model

Each entity must have:

- **Global internal ID**, technical and immutable.
- **Public Ref**, short and readable, used in CLI and TUI.

### Official Public Ref Format

```text
<type>-<MMDD>-<sequence>
```

### Prefixes

- `t` = task
- `n` = note
- `e` = event

### Examples

- `t-0422-1`
- `t-0422-2`
- `n-0422-1`
- `e-0422-1`

### Rules

- The sequence is independent by type and day.
- The Public Ref is persisted.
- The Public Ref does not replace the real internal ID.

### Example Creation Output

```text
[ok] task created: t-0422-1
     fix jwt authentication
```

### Example Later Usage

```bash
termbullet done t-0422-1
termbullet show t-0422-1
termbullet edit t-0422-1
```

---

## `termbullet tui --help`

```text
Open the main TUI interface.

Usage:
  termbullet tui

Description:
  Starts TermBullet's visual terminal interface.

Examples:
  termbullet tui
```

---

## `termbullet add --help`

```text
Create a new item.

Usage:
  termbullet add <text> [options]

Arguments:
  <text>                      Main item content

Options:
      --task                  Create as task (default)
      --note                  Create as note
      --event                 Create as event
      --collection <name>     Set destination collection
      --priority <value>      Set priority: low, medium, high
      --tag <name>            Add a tag (repeatable)
      --due <date>            Set due date
      --when <datetime>       Set event date/time
      --estimate <minutes>    Set estimate in minutes
  -h, --help                  Show help

Examples:
  termbullet add "fix jwt authentication"
  termbullet add "investigate stacktrace" --note
  termbullet add "team daily" --event --when "2026-04-22 09:00"
  termbullet add "adjust migrations" --priority high --tag backend
```

---

## `termbullet list --help`

```text
List items with optional filters.

Usage:
  termbullet list [options]

Options:
      --status <value>        Filter by status: open, done, cancelled, migrated
      --type <value>          Filter by type: task, note, event
      --collection <name>     Filter by collection
      --tag <name>            Filter by tag
      --priority <value>      Filter by priority
      --limit <n>             Limit result count
      --all                   Include archived/closed items
  -h, --help                  Show help

Examples:
  termbullet list
  termbullet list --status open
  termbullet list --type task --tag api
  termbullet list --collection backlog
```

---

## `termbullet today --help`

```text
Show today's context.

Usage:
  termbullet today [options]

Options:
      --details               Show full details
      --json                  JSON output
  -h, --help                  Show help

Examples:
  termbullet today
  termbullet today --details
```

---

## `termbullet week --help`

```text
Show the week context.

Usage:
  termbullet week [options]

Options:
      --details               Show full details
      --json                  JSON output
  -h, --help                  Show help

Examples:
  termbullet week
```

---

## `termbullet backlog --help`

```text
Show backlog items.

Usage:
  termbullet backlog [options]

Options:
      --status <value>        Filter by status
      --tag <name>            Filter by tag
      --priority <value>      Filter by priority
  -h, --help                  Show help

Examples:
  termbullet backlog
  termbullet backlog --priority high
```

---

## `termbullet show --help`

```text
Show item details.

Usage:
  termbullet show <ref>

Arguments:
  <ref>                       Item public reference

Options:
  -h, --help                  Show help

Examples:
  termbullet show t-0422-1
```

---

## `termbullet edit --help`

```text
Edit an existing item.

Usage:
  termbullet edit <ref> [options]

Arguments:
  <ref>                       Item public reference

Options:
      --text <text>           Change content
      --priority <value>      Change priority
      --due <date>            Change due date
      --when <datetime>       Change date/time
      --collection <name>     Change collection
  -h, --help                  Show help

Examples:
  termbullet edit t-0422-1 --text "fix authentication and validate roles"
  termbullet edit t-0422-1 --priority high
```

---

## `termbullet done --help`

```text
Mark an item as done.

Usage:
  termbullet done <ref>

Arguments:
  <ref>                       Item public reference

Options:
  -h, --help                  Show help

Examples:
  termbullet done t-0422-1
```

---

## `termbullet cancel --help`

```text
Mark an item as cancelled.

Usage:
  termbullet cancel <ref>

Arguments:
  <ref>                       Item public reference

Options:
  -h, --help                  Show help

Examples:
  termbullet cancel t-0422-1
```

---

## `termbullet migrate --help`

```text
Migrate an item to another period or collection.

Usage:
  termbullet migrate <ref> [options]

Arguments:
  <ref>                       Item public reference

Options:
      --to <destination>      Migration destination (today, week, backlog, monthly)
  -h, --help                  Show help

Examples:
  termbullet migrate t-0422-1
  termbullet migrate t-0422-1 --to backlog
```

---

## `termbullet move --help`

```text
Move an item to another collection.

Usage:
  termbullet move <ref> --to <collection>

Arguments:
  <ref>                       Item public reference

Options:
      --to <collection>       Destination collection
  -h, --help                  Show help

Examples:
  termbullet move t-0422-1 --to backlog
  termbullet move n-0422-1 --to today
```

---

## `termbullet delete --help`

```text
Remove an item.

Usage:
  termbullet delete <ref> [options]

Arguments:
  <ref>                       Item public reference

Options:
      --force                 Remove without confirmation
  -h, --help                  Show help

Examples:
  termbullet delete t-0422-1
  termbullet delete t-0422-1 --force
```

---

## `termbullet tag --help`

```text
Add a tag to an item.

Usage:
  termbullet tag <ref> <tag>

Arguments:
  <ref>                       Item public reference
  <tag>                       Tag name

Options:
  -h, --help                  Show help

Examples:
  termbullet tag t-0422-1 backend
```

---

## `termbullet untag --help`

```text
Remove a tag from an item.

Usage:
  termbullet untag <ref> <tag>

Arguments:
  <ref>                       Item public reference
  <tag>                       Tag name

Options:
  -h, --help                  Show help

Examples:
  termbullet untag t-0422-1 backend
```

---

## `termbullet priority --help`

```text
Set item priority.

Usage:
  termbullet priority <ref> <value>

Arguments:
  <ref>                       Item public reference
  <value>                     low, medium, high

Options:
  -h, --help                  Show help

Examples:
  termbullet priority t-0422-1 high
```

---

## `termbullet search --help`

```text
Search items by text.

Usage:
  termbullet search <text> [options]

Arguments:
  <text>                      Search text

Options:
      --status <value>        Filter by status
      --type <value>          Filter by type
      --tag <name>            Filter by tag
      --limit <n>             Limit result count
  -h, --help                  Show help

Examples:
  termbullet search "jwt"
  termbullet search "docker" --status open
```

---

## `termbullet export --help`

```text
Export local data.

Usage:
  termbullet export [options]

Options:
      --format <value>        Format: json, markdown
      --output <path>         Output file or directory
  -h, --help                  Show help

Examples:
  termbullet export --format json --output ./backup.json
```

---

## `termbullet import --help`

```text
Import data into the local database.

Usage:
  termbullet import <path> [options]

Arguments:
  <path>                      Input file

Options:
      --format <value>        Format: json, markdown
  -h, --help                  Show help

Examples:
  termbullet import ./backup.json --format json
```

---

## `termbullet config --help`

```text
Manage local application configuration.

Usage:
  termbullet config <subcommand>

Subcommands:
  list                       List configuration values
  get <key>                  Show a configuration value
  set <key> <value>          Set a configuration value
  path                       Show paths used by the application

Examples:
  termbullet config list
  termbullet config get theme
  termbullet config set theme dark
  termbullet config path
```

---

## Product Note

This tree represents the official V1 base, designed to:

- Make the CLI genuinely useful from the beginning.
- Keep syntax short and predictable.
- Reflect the product's terminal-first philosophy.
- Coexist naturally with the TUI.

---

## Part III - TUI Screens Spec

## Objective

This document describes the desired appearance of TermBullet's final TUI.

The visual and operational direction comes from the modern terminal TUI ecosystem, with emphasis on:

- **LazyDocker**: panel overview, instant state reading, footer with quick actions.
- **LazyGit**: panel focus, keyboard-first navigation, filter, and contextual actions.
- **K9s**: specialized views, panels that can become fullscreen, persistent context.
- **btop**: dense dashboards, clean visual structure, well-separated blocks, strong use of status bars.

The goal is not to reproduce any of these tools literally, but to apply their best patterns to bullet journaling, planning, and personal productivity.

---

## Visual Direction Change

The previous direction was correct in philosophy, but too generic.

The new direction assumes an identity closer to what works best in modern TUIs:

- **Named and numbered panels**.
- **Main dashboard as cockpit**.
- **Layout with 6 useful blocks on the same screen**.
- **Fixed footer with short verbs**.
- **Top bar with system state**.
- **Content organized as an operational console, not as a form**.

In short: the main screen should feel more like a **command center** than a list manager.

---

## Final Visual Principles

### 1. Cockpit First

The initial screen must create the feeling of "I am operating my system."

The user must see within a few seconds:

- Where they are.
- What must be done today.
- What is overdue.
- What is relevant now.
- What the suggested next step is.

### 2. Useful Multi-Panel Layout

Each panel must exist for a clear reason.

No panel should be decorative only.

Every block must answer a practical user question.

### 3. Density Without Clutter

The interface must be dense, but not confusing.

Priority goes to:

- Short titles.
- Compact lists.
- Little ornamentation.
- Consistent alignment.
- Clear selection.

### 4. Keyboard at the Center

The TUI must be usable without a mouse.

Frequent actions must always be visible in the footer.

Navigation through `Tab`, numbers, `Enter`, `/`, `Esc`, and action keys must feel natural.

### 5. Contextual Information

Selecting an item in one panel must update the others.

Example:

- Selecting a task in the central panel updates preview, project, related backlog, and suggested plan.

### 6. Compatible With Product Evolution

The V1 TUI must already look ready for V2, V3, and V4.

That means the visual structure must naturally accept:

- AI.
- Calendar.
- Sync/cloud.
- Remote status.

Even if those modules are not active yet.

---

## Required Base Structure

Every main screen must follow this frame:

```text
┌ TermBullet ─ <screen> ─ <date> ─ db:<profile> ─ ai:<state> ─ sync:<state> ─ mode:<mode> ┐
│ [main screen panels]                                                                      │
├────────────────────────────────────────────────────────────────────────────────────────────┤
│ / filter  c capture  e edit  x done  > migrate  Enter zoom  Tab focus  ? help  q quit    │
└────────────────────────────────────────────────────────────────────────────────────────────┘
```

### Top Bar

It must always show:

- Product name.
- Active screen.
- Current date.
- Current database/profile.
- AI state.
- Sync state.
- Current mode.

Examples:

- `db:local`
- `ai:off`
- `ai:on`
- `sync:idle`
- `sync:offline`
- `sync:error`
- `mode:normal`
- `mode:filter`
- `mode:capture`

### Footer

It must always show:

- Universal commands.
- Most important local actions.
- Current focus implied by the active panel.

---

## Visual Language

### Borders

- Simple box drawing border.
- No excessive visual variation.
- Clear and short titles.
- Panel numbers integrated into the title.

### Hierarchy

- Active panel with stronger border, highlighted title, or different color.
- Selected item with inverted background or strong highlight.
- Important states with color and symbol, but not color alone.

### Item Symbols

Suggestion:

- `[ ]` open task
- `[~]` task in progress
- `[x]` completed task
- `[-]` cancelled task
- `[>]` migrated task
- `(.)` note
- `(o)` event

### Public Reference

Refs must appear whenever space allows.

Suggested official format:

- `t-0422-1`
- `n-0422-1`
- `e-0422-1`

In very dense lists, the ref may appear only in the preview.

---

## Screen 01 - Main Dashboard

### Role

This is the main product screen.

It must be TermBullet's most memorable experience.

It combines:

- Day view.
- Structural navigation.
- Contextual backlog.
- Projects/tags.
- Item preview.
- Suggested plan.

This screen must be the main evolution of the original concept.

### Official Layout

```text
┌ TermBullet ─ Daily 2026-04-22 ─ db:local ─ ai:on ─ sync:idle ─ mode:normal ──────────────┐
│ 1 Collections       │ 2 Day Items                        │ 3 Preview / AI                │
│ > Daily             │ > [ ] t-0422-1 Fix auth JWT       │ t-0422-1                      │
│   Weekly            │   [ ] t-0422-2 Review migrations  │ type: task                    │
│   Monthly           │   (o) e-0422-1 Review 16:00       │ status: open                  │
│   Backlog           │   (.) n-0422-1 Investigate error  │ priority: high                │
│   Review            │                                    │ project: api                  │
│   Search            │ today                              │ tags: jwt, auth               │
│─────────────────────┼────────────────────────────────────┼───────────────────────────────│
│ 4 Projects / Tags   │ 5 Filtered Backlog                │ 6 Suggested Plan              │
│ > api               │ jwt                               │ focus: authentication         │
│   infra             │ docker                            │ 09:00 validate claims         │
│   client-app        │ client-app                        │ 10:00 review roles            │
│   auth              │ tests                             │ 11:00 test real token         │
│   devops            │ compose                           │ avoid: broad refactor         │
├────────────────────────────────────────────────────────────────────────────────────────────┤
│ / filter  c capture  e edit  x done  > migrate  a AI  Enter zoom  Tab focus  q quit      │
└────────────────────────────────────────────────────────────────────────────────────────────┘
```

### Role of Each Panel

#### 1. Collections

The product's main navigation.

It must contain at least:

- Daily.
- Weekly.
- Monthly.
- Backlog.
- Review.
- Search.
- Config.

In V1, this column exists even without AI, sync, or calendar.

#### 2. Day Items

The operational heart of the screen.

It shows items relevant to the current slice.

Usually includes:

- Open tasks.
- Useful daily notes.
- Internal events.

This is the most important panel in the upper half.

#### 3. Preview / AI

Shows details for the active item and, when applicable, AI context.

In V1, it must already work well without AI by showing:

- Ref.
- Type.
- Status.
- Priority.
- Project.
- Tags.
- Short description.
- Basic history.

In V2, it also shows:

- Contextual suggestion.
- Decomposition.
- Next steps.

#### 4. Projects / Tags

Fast side filter.

It changes context without opening a separate screen.

This is especially useful in real developer usage.

#### 5. Filtered Backlog

Shows backlog related to the current context.

Example:

- If selected project is `api`, show relevant `api` backlog.
- If selected item is about `auth`, bring correlated backlog.

This panel creates continuity between "what I am doing today" and "what still exists."

#### 6. Suggested Plan

Planning panel.

In V1 it can show:

- Manual focus.
- Basic next steps.
- Local heuristics.

In V2 it becomes one of the most valuable interface blocks.

### Desired Feeling

This screen should feel like a mix of:

- LazyDocker.
- Operational console.
- Personal execution dashboard.

It must not feel like a conventional calendar.

---

## Screen 02 - Daily Focus

### Role

Detailed day operation screen.

While Main Dashboard is the general cockpit, this screen is the workbench.

### Official Layout

```text
┌ TermBullet ─ Today 2026-04-22 ─ db:local ─ ai:off ─ sync:idle ─ mode:normal ──────────────┐
│ 1 Sections          │ 2 Daily Log                        │ 3 Details                     │
│ > Open              │ > [ ] t-0422-1 Fix auth JWT       │ t-0422-1                      │
│   In Progress       │   [ ] t-0422-2 Review migrations  │ type: task                    │
│   Done              │   (.) n-0422-1 Empty audience bug  │ created: 08:14                │
│   Cancelled         │   (o) e-0422-1 Review 16:00       │ priority: high                │
│   Migrated          │                                    │ collection: daily             │
│─────────────────────┼────────────────────────────────────┼───────────────────────────────│
│ 4 Quick Capture     │ 5 Short History                   │ 6 Actions                     │
│ - review swagger    │ 2026-04-21 migrated from backlog  │ x done                        │
│ . empty audience    │ 2026-04-22 priority set high      │ > migrate                     │
│ o sync 16:00        │                                    │ e edit                        │
│ [Enter to insert]   │                                    │ d delete                      │
├────────────────────────────────────────────────────────────────────────────────────────────┤
│ / filter  c capture  e edit  x done  > migrate  Enter detail  Tab focus  q quit          │
└────────────────────────────────────────────────────────────────────────────────────────────┘
```

### Notes

- This is the closest screen to an operational editor.
- The `Quick Capture` panel is very important in V1.
- The preview must be fast, not a giant form.
- The actions column reinforces the keyboard-first feel.

---

## Screen 03 - Weekly Planning

### Role

Turn the week into a practical view without becoming a heavy calendar.

### Official Layout

```text
┌ TermBullet ─ Weekly 2026-W17 ─ db:local ─ ai:on ─ sync:idle ─ mode:normal ────────────────┐
│ 1 Buckets           │ 2 Week                             │ 3 Context / AI                │
│ > Must              │ Mon  [ ] t-0420-1 API auth        │ weekly focus: V1 core         │
│   Should            │ Tue  [ ] t-0421-2 Compose         │ risk: large scope             │
│   Could             │ Wed  [ ] t-0422-1 JWT             │ suggestion: finish auth       │
│   Events            │ Thu  [ ] t-0423-1 Seeds           │ before starting refactor      │
│─────────────────────┼────────────────────────────────────┼───────────────────────────────│
│ 4 Metrics           │ 5 Week Backlog                    │ 6 Notes                       │
│ open: 17            │ t-0424-1 Adjust docker            │ demo on Friday                │
│ done: 9             │ t-0424-2 Review tests             │ avoid overload on Thursday    │
│ migrated: 4         │ n-0421-1 Review scope             │                               │
│ events: 6           │                                    │                               │
├────────────────────────────────────────────────────────────────────────────────────────────┤
│ / filter  c capture  p plan  > migrate  Enter zoom  Tab focus  q quit                    │
└────────────────────────────────────────────────────────────────────────────────────────────┘
```

### Important Rule

The week screen must not copy the traditional visual calendar paradigm.

It is a view of intention, distribution, and focus, not a colorful calendar.

---

## Screen 04 - Backlog Triage

### Role

Triage, filter, and clean the backlog.

This screen should strongly resemble tools such as LazyGit: large lists, side context, fast action.

### Official Layout

```text
┌ TermBullet ─ Backlog ─ db:local ─ ai:off ─ sync:idle ─ mode:filter ───────────────────────┐
│ 1 Filters           │ 2 Backlog Items                    │ 3 Preview                     │
│ > project: api      │ > [ ] t-0419-1 Adjust compose     │ t-0419-1                      │
│   tag: jwt          │   [ ] t-0419-2 Review roles       │ priority: medium              │
│   status: open      │   (.) n-0418-1 Review scope       │ project: infra                │
│   priority: all     │   [ ] t-0417-1 Clean migrations   │ origin: backlog               │
│─────────────────────┼────────────────────────────────────┼───────────────────────────────│
│ 4 Group By          │ 5 Next Candidates                 │ 6 Suggestion                  │
│ > project           │ auth                              │ move 2 items to today         │
│   priority          │ docker                            │ cancel 1 old item             │
│   age               │ tests                             │ review 1 context note         │
│   type              │ compose                           │                               │
├────────────────────────────────────────────────────────────────────────────────────────────┤
│ / filter  m move  > migrate  x done  c cancel  Enter detail  Tab focus  q quit           │
└────────────────────────────────────────────────────────────────────────────────────────────┘
```

### Desired Feeling

- Fast triage.
- Clarity.
- Heavy keyboard operation.
- Context without leaving the screen.

---

## Screen 05 - Review

### Role

Enable daily and weekly review without leaving the TUI.

### Official Layout

```text
┌ TermBullet ─ Review ─ 2026-04-22 ─ db:local ─ ai:on ─ sync:idle ─ mode:normal ────────────┐
│ 1 Period            │ 2 Summary                         │ 3 Insights                    │
│ > Daily             │ done: 4                           │ pattern: auth moves early     │
│   Weekly            │ open: 3                           │ docker always migrates        │
│   Monthly           │ migrated: 2                       │ scope grows after lunch       │
│─────────────────────┼────────────────────────────────────┼───────────────────────────────│
│ 4 What Moved        │ 5 What Blocked                    │ 6 Next Cycle                  │
│ auth jwt            │ docker compose                    │ prioritize tests tomorrow     │
│ migrations review   │ broad refactor                    │ move compose to backlog       │
│ meeting 16:00       │                                    │                               │
├────────────────────────────────────────────────────────────────────────────────────────────┤
│ r generate review  a AI  > migrate pending  Enter zoom  Tab focus  q quit                │
└────────────────────────────────────────────────────────────────────────────────────────────┘
```

### Note

In V1, this screen can be mostly manual and heuristic-based.

In V2, it absorbs significant value from AI.

---

## Screen 06 - Search / Command Palette

### Role

Fast access to items, collections, and actions.

A mix of search and command palette.

### Official Layout

```text
┌ TermBullet ─ Search ─ db:local ─ ai:off ─ sync:idle ─ mode:search ────────────────────────┐
│ query: jwt                                                                                │
│────────────────────────────────────────────────────────────────────────────────────────────│
│ 1 Results                                  │ 2 Preview                                    │
│ > t-0422-1 Fix auth JWT                    │ ref: t-0422-1                               │
│   n-0422-1 Empty audience bug              │ collection: daily                           │
│   t-0419-2 Review roles                    │ tags: jwt, auth                             │
│   command: add                             │                                              │
├────────────────────────────────────────────────────────────────────────────────────────────┤
│ / search  Enter open  Ctrl+e edit  Ctrl+x done  Esc back                                  │
└────────────────────────────────────────────────────────────────────────────────────────────┘
```

### Note

This screen must be very fast.

Ideal for frictionless navigation.

---

## Screen 07 - Calendar View (V3)

### Role

Show the day's schedule without turning the product into a full-featured calendar.

### Official Layout

```text
┌ TermBullet ─ Calendar ─ 2026-04-22 ─ db:local ─ ai:on ─ sync:idle ─ mode:normal ──────────┐
│ 1 Calendars         │ 2 Day Agenda                       │ 3 Bullet Relation             │
│ > Google Primary    │ 09:00 Daily                        │ free window: 10:00-11:00      │
│   Work              │ 11:00 API meeting                  │ suggest: t-0422-1             │
│   Personal          │ 16:00 Review                       │ create block for tests        │
│─────────────────────┼────────────────────────────────────┼───────────────────────────────│
│ 4 Local Events      │ 5 Schedulable Tasks               │ 6 Actions                     │
│ e-0422-1 Review     │ t-0422-1 Fix auth JWT             │ i import event                │
│                     │ t-0422-2 Review migrations        │ o create in calendar          │
│                     │                                    │                               │
├────────────────────────────────────────────────────────────────────────────────────────────┤
│ / filter  i import  o export event  Enter detail  Tab focus  q quit                      │
└────────────────────────────────────────────────────────────────────────────────────────────┘
```

### Rule

Calendar is auxiliary context, not the product center.

---

## Screen 08 - Sync / Cloud (V4)

### Role

Show synchronization state between machines without polluting the product.

### Official Layout

```text
┌ TermBullet ─ Sync ─ db:local ─ ai:on ─ sync:idle ─ mode:normal ───────────────────────────┐
│ 1 State             │ 2 Devices                          │ 3 Latest Changes              │
│ sync: idle          │ desktop-main                       │ t-0422-1 updated              │
│ last sync: 08:14    │ laptop-dev                         │ n-0422-1 created              │
│ conflicts: 0        │ workstation                        │ backlog filter changed        │
│─────────────────────┼────────────────────────────────────┼───────────────────────────────│
│ 4 Local Queue       │ 5 Conflicts                        │ 6 Actions                     │
│ 2 pending ops       │ none                               │ s sync now                    │
│ 0 failed            │                                    │ r retry failed                │
│                     │                                    │                               │
├────────────────────────────────────────────────────────────────────────────────────────────┤
│ s sync  r retry  Enter details  Tab focus  q quit                                        │
└────────────────────────────────────────────────────────────────────────────────────────────┘
```

---

## Screen 09 - Config

### Role

Local configuration center.

### Official Layout

```text
┌ TermBullet ─ Config ─ db:local ─ ai:on ─ sync:idle ─ mode:normal ─────────────────────────┐
│ 1 Sections          │ 2 Options                         │ 3 Value / Preview             │
│ > General           │ theme                             │ dark                          │
│   TUI               │ date format                       │ YYYY-MM-DD                    │
│   AI                │ refs visible                      │ true                          │
│   Calendar          │ compact lists                     │ true                          │
│   Sync              │                                   │                               │
├────────────────────────────────────────────────────────────────────────────────────────────┤
│ Enter edit  Tab focus  q quit                                                            │
└────────────────────────────────────────────────────────────────────────────────────────────┘
```

---

## Cross-Screen Behaviors

### Panel Focus

- `Tab` advances focus.
- `Shift+Tab` moves focus back.
- Numbers may jump to the corresponding panel when appropriate.

### Zoom / Fullscreen

- `Enter` expands the active panel.
- `Esc` returns to the previous layout.
- Inspired by the behavior of dedicated views in tools such as K9s.

### Quick Filter

- `/` opens contextual filtering in the active panel.
- Mode changes to `mode:filter`.
- `Esc` clears or exits.

### Quick Capture

- `c` opens capture mode.
- `Ctrl+n` creates a note.
- `Ctrl+t` creates a task.
- `Ctrl+e` creates an event.
- The system must allow capture without leaving the current context.

### Help

- `?` opens a compact help screen with shortcuts for the current context.
- Help must be fast, not a giant documentation screen.

---

## Screen Hierarchy by Version

### V1

Required:

- Main Dashboard.
- Daily Focus.
- Weekly Planning.
- Backlog Triage.
- Review.
- Search.
- Config.

### V2

Enhancements:

- AI panels in Preview / AI.
- Enriched suggested plan.
- Review with insights.

### V3

New screens/modules:

- Calendar View.
- Integration with daily events.

### V4

New screens/modules:

- Sync / Cloud.
- Multi-device state.

---

## Final Direction

TermBullet's final TUI should be described as:

- **A personal cockpit for planning and execution**.
- **Terminal-first and keyboard-first**.
- **Visually inspired by LazyDocker/LazyGit**.
- **Dense and clear, close to btop**.
- **Ready for AI, calendar, and sync without breaking the base**.

The main improvement over the previous proposal is this:

**The main screen stops being only a split dashboard and becomes a 6-panel operational center with the feel of a mature TUI product.**
