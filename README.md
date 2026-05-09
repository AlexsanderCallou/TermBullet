# TermBullet

TermBullet is a local-first terminal planner for tasks, notes, events, and
personal review workflows.

It provides two first-class interfaces over the same Application use cases:

- **TUI:** keyboard-first visual interface with panel-based screens.
- **CLI:** fast capture, lookup, and item manipulation from the shell.

> Status: experimental MVP. CLI and local JSON persistence are the most complete
> areas. The active TUI MVP is limited to Main Dashboard, Search, and Add Item.

## Repository

Official repository:

```text
https://github.com/AlexsanderCallou/TermBullet
```

Main development branch:

```text
Development
```

TermBullet is English-first and intended for a global open source audience.

## Disclaimer

TermBullet is part of the author's study on using AI to support software coding
and project delivery. It is recommended for personal use, experimentation, and
learning only. Professional or production-critical usage is not recommended at
this stage.

Legal policy and trademark usage are in [TRADEMARKS.md](TRADEMARKS.md). The
license is Apache License 2.0 in [LICENSE](LICENSE).

## Install

Current release:

```text
v0.1.0 - Experimental MVP
```

Latest release:

```text
https://github.com/AlexsanderCallou/TermBullet/releases/latest
```

Windows x64:

```powershell
irm https://raw.githubusercontent.com/AlexsanderCallou/TermBullet/main/install.ps1 | iex
```

Linux x64:

```bash
curl -fsSL https://raw.githubusercontent.com/AlexsanderCallou/TermBullet/main/install.sh | sh
```

After installing, open a new terminal and run:

```bash
termbullet --help
```

Manual release artifacts:

```text
termbullet_0.1.0_windows_x64.zip
termbullet_0.1.0_linux_x64.tar.gz
termbullet_0.1.0_checksums.txt
```

macOS binaries and package manager distribution are planned for later releases.

## Data Location

TermBullet stores local monthly JSON files outside the executable directory.

```text
Windows: %APPDATA%\TermBullet\data
macOS:   ~/Library/Application Support/TermBullet/data
Linux:   ~/.local/share/termbullet/data
```

Custom data directory support is planned after the first MVP. For now,
TermBullet uses the platform default data directory.

## Quick Use

Open the TUI:

```bash
termbullet
```

CLI examples:

```bash
termbullet add "fix jwt authentication"
termbullet add "error happens when audience is empty" --note
termbullet today
termbullet done t-0426-1
termbullet show t-0426-1
termbullet search "jwt"
```

The full command tree is documented in [CLI.md](CLI.md).

## Product Summary

V1 delivers:

- tasks, notes, and events;
- Today, Backlog, and Forgotten collections;
- Week planning view;
- CLI and TUI MVP;
- local monthly JSON persistence;
- local JSON index;
- search;
- basic editing;
- migration and movement;
- local data path discovery;
- export and import.

V1 does not include AI execution, Google Calendar, machine sync, cloud accounts,
or a PostgreSQL runtime dependency.

The product direction, roadmap, item model, and acceptance criteria are in
[PRODUCT.md](PRODUCT.md).

## TUI

The TUI is a personal cockpit for planning and execution:

- terminal-first and keyboard-first;
- panel-based;
- dense and legible;
- inspired by LazyDocker/LazyGit style workflows;
- driven by visible footer shortcuts.

The concrete editable screen reference is [screens.md](screens.md).

## Development

Expected local commands:

```bash
dotnet restore
dotnet build
dotnet test
dotnet run --project src/TermBullet -- [command] [arguments] [options]
```

TermBullet uses a modular monolith:

- `Core`
- `Application`
- `Infrastructure`
- `Cli`
- `Tui`
- `Bootstrap`

Architecture details are in [ARCHITECTURE.md](ARCHITECTURE.md).

## Documentation Map

- [PRODUCT.md](PRODUCT.md) - product scope, principles, roadmap.
- [CLI.md](CLI.md) - command tree and CLI behavior.
- [screens.md](screens.md) - TUI screen models.
- [DATA_MODEL.md](DATA_MODEL.md) - monthly JSON data contract.
- [ARCHITECTURE.md](ARCHITECTURE.md) - module boundaries and flows.
- [ADR.md](ADR.md) - accepted architecture decisions.
- [BACKLOG.md](BACKLOG.md) - execution backlog and post-V1 work.
- [AGENTS.md](AGENTS.md) - AI agent development guide.
- [CONTRIBUTING.md](CONTRIBUTING.md) - contribution rules.
- [TRADEMARKS.md](TRADEMARKS.md) - legal and trademark wording.
