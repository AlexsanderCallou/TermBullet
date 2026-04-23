# TermBullet

TermBullet is a personal productivity application for the terminal, inspired by the Bullet Journal philosophy and designed especially for developers and users who live in the shell.

The project combines two first-class interfaces over the same functional core:

- **TUI**: the main visual interface, with screens and panels inspired by tools such as LazyDocker and LazyGit.
- **CLI**: a fast interface for capturing, querying, and manipulating items without opening the TUI.

> Status: this repository currently contains the product specification and architecture decisions. The commands and screens below represent the target product interface.

## Repository

Official repository:

```text
https://github.com/AlexsanderCallou/TermBullet
```

Main development branch:

```text
Development
```

## Open Source

TermBullet is intended to be an open source project for a global audience.

Documentation, command names, examples, and user-facing text should be English-first. TermBullet is released under the MIT License.

## Installation

TermBullet is planned to be distributed through GitHub Releases with prebuilt binaries for Windows, Linux, and macOS.

The preferred release artifact format is a self-contained executable per platform, so users do not need to install the .NET runtime manually.

### Install Script

Linux/macOS:

```bash
curl -fsSL https://raw.githubusercontent.com/AlexsanderCallou/TermBullet/Development/install.sh | sh
```

Windows PowerShell:

```powershell
irm https://raw.githubusercontent.com/AlexsanderCallou/TermBullet/Development/install.ps1 | iex
```

### .NET Tool

For users who already have the .NET SDK installed:

```bash
dotnet tool install --global TermBullet
```

Update:

```bash
dotnet tool update --global TermBullet
```

### Manual Installation

Download the archive for your platform from GitHub Releases, extract it, and place the `termbullet` executable in your `PATH`.

Planned release artifacts:

```text
termbullet_<version>_windows_x64.zip
termbullet_<version>_linux_x64.tar.gz
termbullet_<version>_linux_arm64.tar.gz
termbullet_<version>_macos_x64.tar.gz
termbullet_<version>_macos_arm64.tar.gz
```

### Planned Package Managers

Package manager support is planned after the first public releases:

- Homebrew
- Scoop
- Winget
- Chocolatey

## Data Location

TermBullet stores local monthly JSON files outside the executable directory.

Default data locations:

```text
Windows: %APPDATA%\TermBullet\data
macOS:   ~/Library/Application Support/TermBullet/data
Linux:   ~/.local/share/termbullet/data
```

The data directory can be overridden with:

```bash
termbullet --data <path>
```

## Objective

Allow users to organize tasks, notes, events, and personal reviews directly from the terminal with a fast, predictable, local-first workflow.

V1 must work fully offline, without depending on internet access, online accounts, AI, Google Calendar, or cloud services. Future integrations must remain optional and must not break the local-first foundation.

## Principles

- **Local-first**: local JSON files are the user's primary operational source.
- **Terminal-first**: optimized for keyboard, shell, and frequent terminal usage.
- **CLI + TUI**: both interfaces must reuse the same use cases.
- **Optional AI**: planning assistance, never a product dependency.
- **Optional integrations**: calendar, sync, and cloud are extensions.
- **Evolutionary architecture**: V1 must prepare the ground for V2, V3, and V4.
- **Open source by design**: documentation and structure should support external contributors.

## V1 Scope

The first version of TermBullet should deliver the offline core:

- creation and manipulation of tasks, notes, and events;
- Today, Week, and Backlog collections;
- main TUI with keyboard navigation;
- CLI for capture and fast operations;
- search;
- basic editing;
- item migration and movement;
- local configuration;
- monthly JSON file storage;
- local JSON search index;
- basic export and import.

V1 does not include AI, Google Calendar, machine sync, or cloud accounts.

## Item Types

The system must support three main item types:

- **Task**: an executable item or pending work.
- **Note**: a record, context, or observation.
- **Event**: an appointment or internal time-based marker.

Each item must record at least:

- internal global ID;
- public ref;
- type;
- content;
- status;
- creation and update dates;
- current collection;
- priority;
- tags.

## Public Refs

TermBullet must not expose only simple numeric IDs as the main user-facing reference.

Each entity has:

- **Internal global ID**: technical, stable, unique, and prepared for future sync.
- **Public Ref**: short, readable, and used in the CLI/TUI.

Official format:

```text
<type>-<MMYY>-<sequence>
```

Prefixes:

- `t` = task
- `n` = note
- `e` = event

Examples:

```text
t-0426-1
t-0426-2
n-0426-1
e-0426-1
```

Rules:

- the sequence is independent by type and month/year;
- the public ref must be persisted;
- the public ref must not be reused;
- the internal ID remains the real entity identity.

## CLI Interface

When no command is provided, TermBullet should open the main TUI:

```bash
termbullet
```

Planned main commands:

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
├── history
│   └── clear
├── export
├── import
└── config
    ├── list
    ├── get
    ├── set
    └── path
```

Target usage examples:

```bash
termbullet add "fix jwt authentication"
termbullet add --note "error happens when audience is empty"
termbullet today
termbullet done t-0426-1
termbullet show t-0426-1
termbullet search "jwt"
```

Planned global options:

- `-h`, `--help`: show help.
- `-v`, `--version`: show version.
- `--json`: JSON output when supported.
- `--no-color`: disable colors.
- `--data <path>`: use an alternative local data directory.
- `--profile <name>`: use a specific configuration profile.

## TUI Interface

The TUI should work as a personal cockpit for planning and execution:

- terminal-first and keyboard-first;
- inspired by LazyDocker/LazyGit;
- dense, clear, and operational;
- based on screens, panels, visual focus, and consistent shortcuts.

Required V1 screens:

- Main Dashboard;
- Daily Focus;
- Weekly Planning;
- Backlog Triage;
- Review;
- Search / Command Palette;
- Config.

Future screens:

- Calendar View in V3;
- Sync / Cloud in V4.

Expected cross-screen behaviors:

- `Tab` and `Shift+Tab` to move focus between panels;
- `Enter` to expand or open the active panel;
- `Esc` to go back;
- `/` for contextual filtering;
- `c` for quick capture;
- `?` for compact contextual help.

## Architecture

The project is a **modular monolith**.

Production code should live in a single .NET project, with clear internal separation by folders, namespaces, contracts, and tests. This keeps the project proportional to its size while preserving clean boundaries between domain, use cases, infrastructure, CLI, and TUI.

Recommended internal modules:

- **Core**: entities, business rules, states, identification policies, and migration policies.
- **Application**: use cases, orchestration services, and input/output contracts.
- **Infrastructure**: local persistence, import/export, AI, calendar, and future sync.
- **CLI**: commands, handlers, and text rendering.
- **TUI**: screens, navigation, shortcuts, panels, and visual context.
- **Bootstrap**: startup, dependency wiring, and dispatch between CLI and TUI.

Essential rule:

> CLI and TUI must reuse the same Application layer use cases.

## Development Method

TermBullet follows a TDD workflow.

Before production implementation starts, unit tests must be written first. Tests should cover successful paths with valid mocked data and failure paths with invalid, missing, malformed, or conflicting mocked data.

A development task is only considered complete after all relevant tests pass successfully.

## Technology Stack

The official development stack for TermBullet is:

- **.NET 8 / C#** as the main platform and implementation language.
- **Terminal.Gui** for the TUI, using a panel/window-based layout.
- **System.CommandLine** for the command-line interface.
- **Monthly JSON files** as the local offline data store in V1.
- **Local JSON index** for faster lookup and search.
- **PostgreSQL** as the future backend database for synchronization/cloud in V4, storing the same JSON files.

This combination fits the product goals: a local-first application, fast to use in the terminal, with a rich TUI, a robust CLI, and an architecture prepared to evolve with AI, calendar integration, and cross-device sync.

Official references:

- [.NET 8 / C#](https://learn.microsoft.com/pt-br/dotnet/core/whats-new/dotnet-8/overview)
- [Terminal.Gui](https://github.com/gui-cs/Terminal.Gui)
- [System.CommandLine](https://learn.microsoft.com/en-us/dotnet/standard/commandline/)
- [PostgreSQL](https://www.postgresql.org/docs/)

## Roadmap

### V1 - Offline Core

- TUI;
- CLI;
- tasks, notes, and events;
- today, week, and backlog;
- monthly JSON file persistence;
- local JSON index;
- export/import;
- human-readable identification.

### V2 - AI Planning

- AI configuration module;
- BYOK model;
- daily planning;
- daily review;
- task breakdown;
- backlog prioritization;
- preview before persisting suggestions.

### V3 - Google Calendar

- reading daily events;
- dashboard display;
- schedule context for AI;
- event creation from TermBullet.

### V4 - Sync + Cloud

- synchronization between machines;
- optional cloud;
- conflict resolution;
- sync history;
- preservation of the local-first philosophy.

## V1 Acceptance Criteria

V1 is considered adequate when:

1. users can use TermBullet locally without internet access;
2. the TUI offers consistent screen and panel navigation;
3. the CLI can manipulate the main items without opening the TUI;
4. tasks, notes, and events can be created, listed, edited, and changed;
5. the system uses readable public refs in the official format;
6. basic export and import are available;
7. the architecture is ready for future AI, calendar, and sync;
8. documentation is English-first and ready for open source publication.

## Documentation

- [Product specification](product-spec.md)
- [Architecture Decision Records](ADR.md)
- [Agent development guide](AGENTS.md)
- [Technical architecture](ARCHITECTURE.md)
- [Data model](DATA_MODEL.md)
- [Development plan](DEVELOPMENT_PLAN.md)
- [Contributing guide](CONTRIBUTING.md)
