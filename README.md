# TermBullet

TermBullet is a local-first terminal planner for tasks, notes, events, and
personal review workflows.

It provides two first-class interfaces over the same Application use cases:

- **TUI:** keyboard-first visual interface with panel-based screens.
- **CLI:** fast capture, lookup, and item manipulation from the shell.

> Status: V1 offline core. The active TUI MVP includes Main Dashboard, Search,
> Item Detail, Planning placeholder, Week, Month, Backlog, Forgotten, Notes,
> Calendar, Tags, Migrate Item, and Add Item.

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
v1.0.0 - V1 Offline Core
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
termbullet_1.0.0_windows_x64.zip
termbullet_1.0.0_linux_x64.tar.gz
termbullet_1.0.0_checksums.txt
```

Build local release assets:

```bash
VERSION=1.0.0 ./publish.sh
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
- Today, Week, Month, and Backlog collections;
- Forgotten review;
- Week View;
- Notes, Calendar, and Tags views;
- CLI and TUI MVP;
- local monthly JSON persistence;
- local JSON index;
- search;
- basic editing;
- migration and movement;
- local data path discovery.

V1 does not include AI execution, Google Calendar, machine sync, cloud accounts,
or a PostgreSQL runtime dependency.

The product direction, roadmap, item model, and acceptance criteria are in
[PRODUCT.md](PRODUCT.md).

## TUI

The TUI is a personal cockpit for dated work, review, and execution:

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

TermBullet uses one production project with clear folders:

- `Domain`
- `Application`
- `Repositories`
- `Services`
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
- [RELEASE_NOTES.md](RELEASE_NOTES.md) - release notes and expected assets.
- [AGENTS.md](AGENTS.md) - AI agent development guide.
- [CONTRIBUTING.md](CONTRIBUTING.md) - contribution rules.
- [TRADEMARKS.md](TRADEMARKS.md) - legal and trademark wording.
