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
v2.0.0 - Hybrid AI Profiles
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

## Uninstall

TermBullet does not currently include an uninstall command. To remove the
application safely, delete only the installed executable first and keep your
data directory unless you are sure you no longer need it.

Windows default install location:

```powershell
Remove-Item "$env:LOCALAPPDATA\TermBullet\bin\termbullet.exe"
```

If the installer added TermBullet to your user `PATH`, remove this entry from
your user environment variables:

```text
%LOCALAPPDATA%\TermBullet\bin
```

Linux default install location:

```bash
rm "$HOME/.local/bin/termbullet"
```

The runtime config is stored beside the installed executable:

```text
<install-dir>/conf.json
```

That config points to your selected `data_root`, where TermBullet stores
operational data under:

```text
<data_root>/data
```

Delete `conf.json` and `<data_root>/data` only when you intentionally want to
remove local TermBullet data.

macOS binaries and package manager distribution are planned for later releases.

## Data Location

On first execution, TermBullet asks where local data should be stored. The
choice is saved in:

```text
<install-dir>/conf.json
```

Example:

```json
{
  "data_root": "C:\\Users\\Alexsander\\Documents\\TermBullet"
}
```

TermBullet validates that it can create, read, and write the selected data
directory before saving the config. Operational files are stored under:

```text
<data_root>/data
```

The install directory must allow writing `conf.json`. If it does not,
TermBullet exits with a clear permission error so the user can adjust
permissions or reinstall into a writable directory.

AI Planning also installs the canonical planning agent prompt beside the
executable:

```text
<install-dir>/agents/planning-bulletjournal-agent.md
```

TermBullet must load that agent before every AI planning request. If the file is
missing or unreadable, AI planning fails before calling the configured provider.

Show the active paths:

```bash
termbullet path
```

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

## AI Planning Setup

AI Planning is optional. TermBullet keeps working without AI, internet
access, or external accounts.

The recommended AI setup is OpenCode Zen with the free
`deepseek-v4-flash-free` model. It exposes an OpenAI-compatible endpoint and
works well with TermBullet's structured planning flow.

Create an OpenCode Zen API key from the official documentation:

```text
https://opencode.ai/docs/zen/
```

Create or edit the AI configuration file:

```text
<data_root>/.aiconf
```

Running `termbullet test-ai` creates a commented template when `.aiconf` does
not exist.

Recommended OpenCode Zen profile:

```ini
[opencode-free]
provider=openai-compatible
model=deepseek-v4-flash-free
base_url=https://opencode.ai/zen/v1
api_key_env=OPENCODE_API_KEY
default=true
reasoning=true
test_max_tokens=128
chat_max_tokens=1200
planning_max_tokens=3000
timeout_seconds=240
```

Validate and switch profiles:

```bash
termbullet test-ai
termbullet set-ai opencode-free
```

Set the environment variable before validation:

```bash
export OPENCODE_API_KEY="your-api-key"
```

PowerShell:

```powershell
$env:OPENCODE_API_KEY="your-api-key"
```

Other local or hosted providers are still supported when they expose an
OpenAI-compatible API, but TermBullet does not recommend a local model by
default. For hosted keys, prefer `api_key_env` instead of writing a secret into
`.aiconf`.

## Product Summary

V1 delivers:

- tasks, notes, and events;
- Today, Week, Month, Backlog, Notes, and Events collections;
- Forgotten review;
- Week View;
- Notes, Calendar, and Tags views;
- CLI and TUI MVP;
- local monthly JSON persistence;
- local JSON index;
- search;
- basic editing;
- migration and movement;
- local data path discovery;
- optional AI-assisted guided planning with local or OpenAI-compatible profiles.

V1 does not include Google Calendar, machine sync, cloud accounts, or a
PostgreSQL runtime dependency.

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

Build local release assets:

```bash
VERSION=2.0.0 ./publish.sh
```

Windows PowerShell:

```powershell
.\publish.ps1 -Version 2.0.0
```

When running from source, `conf.json` is created beside the built executable
under `src/TermBullet/bin/...`, not in the directory where the command was
started.

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
