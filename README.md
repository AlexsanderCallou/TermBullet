# TermBullet

TermBullet is a local-first terminal planner for tasks, notes, events, and personal review workflows.

It provides both a TUI and a CLI over the same core, with an emphasis on offline use, keyboard-first interaction, and transparent local data.

The project combines two first-class interfaces over the same functional core:

- **TUI**: the main visual interface, with screens and panels inspired by tools such as LazyDocker and LazyGit.
- **CLI**: a fast interface for capturing, querying, and manipulating items without opening the TUI.

> Status: TermBullet is approaching an experimental MVP. The CLI and local JSON persistence are the most complete areas. The active TUI MVP is intentionally limited to Main Dashboard, Search, and Add Item while broader TUI screens remain planned.

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

Documentation, command names, examples, and user-facing text should be English-first.

## Study Disclaimer

TermBullet is part of the author's ongoing study on using AI to support software coding and project delivery.

This project is recommended for personal use, experimentation, and learning purposes only.

The author does not recommend using TermBullet in professional or production-critical environments at this stage.

## Legal

Legal policy and trademark usage are centralized in [TRADEMARKS.md](TRADEMARKS.md).

The project license is Apache License 2.0 in [LICENSE](LICENSE).

## Installation

TermBullet is distributed through GitHub Releases.

Current release:

```text
v0.1.0 - Experimental MVP
```

Release page:

```text
https://github.com/AlexsanderCallou/TermBullet/releases/latest
```

### Windows x64

Install with PowerShell:

```powershell
irm https://raw.githubusercontent.com/AlexsanderCallou/TermBullet/main/install.ps1 | iex
```

The installer downloads the latest Windows x64 release, verifies the SHA256 checksum, installs `termbullet.exe` into:

```text
%LOCALAPPDATA%\TermBullet\bin
```

and adds that directory to the user `PATH`.

After installation, open a new terminal and run:

```powershell
termbullet --help
```

### Linux x64

Install with `curl`:

```bash
curl -fsSL https://raw.githubusercontent.com/AlexsanderCallou/TermBullet/main/install.sh | sh
```

The installer downloads the latest Linux x64 release, verifies the SHA256 checksum, installs `termbullet` into:

```text
~/.local/bin
```

The script requires `curl`, `tar`, and either `sha256sum` or `shasum`.

After installation, run:

```bash
termbullet --help
```

If `termbullet` is not found, add the install directory to your shell `PATH`.

### Update

For future Windows releases, run the PowerShell installer again:

```powershell
irm https://raw.githubusercontent.com/AlexsanderCallou/TermBullet/main/install.ps1 | iex
```

The installer always resolves the latest GitHub Release by default and replaces the local `termbullet.exe`.

For future Linux releases, run the shell installer again:

```bash
curl -fsSL https://raw.githubusercontent.com/AlexsanderCallou/TermBullet/main/install.sh | sh
```

The Linux installer always resolves the latest GitHub Release by default and replaces the local `termbullet`.

To install a specific version on Windows:

```powershell
& ([scriptblock]::Create((irm https://raw.githubusercontent.com/AlexsanderCallou/TermBullet/main/install.ps1))) -Version v0.1.0
```

To install a specific version on Linux:

```bash
VERSION=v0.1.0 sh -c "$(curl -fsSL https://raw.githubusercontent.com/AlexsanderCallou/TermBullet/main/install.sh)"
```

### Uninstall

On Windows, remove the installed executable directory:

```powershell
Remove-Item "$env:LOCALAPPDATA\TermBullet\bin" -Recurse -Force
```

Then remove this entry from the user `PATH`:

```text
%LOCALAPPDATA%\TermBullet\bin
```

TermBullet data is stored separately and is not removed by deleting the executable.

To remove local data as well:

```powershell
Remove-Item "$env:APPDATA\TermBullet" -Recurse -Force
```

On Linux, remove the installed executable:

```bash
rm -f "$HOME/.local/bin/termbullet"
```

TermBullet data is stored separately and is not removed by deleting the executable.

To remove local data as well:

```bash
rm -rf "$HOME/.local/share/termbullet"
```

### Manual Download

Download:

```text
termbullet_0.1.0_windows_x64.zip
termbullet_0.1.0_linux_x64.tar.gz
```

On Windows, extract the archive and run:

```powershell
.\termbullet.exe --help
```

On Linux, extract the archive and run:

```bash
tar -xzf termbullet_0.1.0_linux_x64.tar.gz
chmod +x ./termbullet
./termbullet --help
```

Optionally, add the extracted folder to your `PATH`.

### Verify Checksum

Download the checksum file from the same release:

```text
termbullet_0.1.0_checksums.txt
```

Then compare the SHA256 hash:

```powershell
Get-FileHash .\termbullet_0.1.0_windows_x64.zip -Algorithm SHA256
```

```bash
sha256sum termbullet_0.1.0_linux_x64.tar.gz
```

### Other Platforms

macOS binaries are planned for future releases.

### Planned Install Methods

The following install methods are planned after the first public releases:

- install script for macOS
- .NET global tool
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

Custom data directory support is planned after the first MVP. For now, TermBullet uses the platform default data directory.

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
- main TUI MVP with keyboard navigation;
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

Global options:

- `-h`, `--help`: show help.
- `-v`, `--version`: show version.

Deferred global options:

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

Active MVP screens:

- Main Dashboard;
- Search / Command Palette;
- Add Item as an auxiliary keyboard-only flow.

Deferred TUI screens:

- Daily Focus;
- Weekly Planning;
- Backlog Triage;
- Review;
- Config;
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

### V1 - Offline Core MVP

- TUI MVP with Main Dashboard, Search, and Add Item;
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
2. the TUI MVP offers consistent keyboard navigation for Main Dashboard, Search, and Add Item;
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
