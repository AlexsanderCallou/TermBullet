# TermBullet - Agent Guide

This file defines how AI coding agents should work in TermBullet.

Official repository:

```text
https://github.com/AlexsanderCallou/TermBullet
```

Default development branch:

```text
Development
```

## Read Order

Before implementation decisions, read the documents relevant to the task:

1. [ADR.md](ADR.md) for accepted long-term decisions.
2. [PRODUCT.md](PRODUCT.md) for product scope and behavior.
3. [ARCHITECTURE.md](ARCHITECTURE.md) for module boundaries.
4. [DATA_MODEL.md](DATA_MODEL.md) for persistence and JSON contracts.
5. [CLI.md](CLI.md) for CLI changes.
6. [screens.md](screens.md) for TUI changes.
7. [BACKLOG.md](BACKLOG.md) for execution order, current status, and remaining
   work.

If documents conflict, follow the same priority order. Add or update an ADR for
new long-term architecture, dependency, storage, or workflow decisions.

## Language

TermBullet is English-first. Use English for:

- documentation;
- code comments;
- command names;
- CLI help text;
- TUI labels;
- error messages;
- examples;
- commit messages.

Maintainer conversation may happen in another language, but project artifacts
must remain English-first.

## Stack and Scope

Accepted stack:

- .NET 8 / C#;
- Terminal.Gui for TUI;
- System.CommandLine for CLI;
- monthly JSON files for V1 local storage;
- local JSON index;
- PostgreSQL only for future optional V4 sync/cloud backend.

V1 is local-first and offline. It includes tasks, notes, events, Today, Backlog,
Forgotten review, Week View, CLI, TUI MVP, monthly JSON persistence,
search, editing, migration, movement, and data path discovery.

V1 excludes AI execution, Google Calendar, machine sync, cloud accounts, and a
PostgreSQL runtime dependency.

## Architecture Rules

TermBullet is a single-project local-first application. Production code lives
under readable folders:

- `Bootstrap`
- `Application`
- `Domain`
- `Repositories`
- `Services`
- `Cli`
- `Tui`

Dependency rules:

- Domain depends on no internal outer folder.
- Application depends on Domain and repository/service interfaces, not concrete
  JSON repositories, CLI, or TUI.
- Repositories implement persistence contracts.
- Services implement technical services such as clock, IDs, data transfer, and
  maintenance.
- CLI and TUI call Application use cases.
- Bootstrap wires everything together.

Do not put business rules in CLI handlers, TUI screens, or JSON repositories.

## Identity Rules

Every relevant item has:

- internal global ID;
- persisted public ref.

Public ref format:

```text
<type>-<MMYY>-<sequence>
```

Prefixes: `t` task, `n` note, `e` event.

Examples:

```text
t-0426-1
n-0426-1
e-0426-1
```

The public ref is for humans. The internal ID is the real identity for
persistence and future sync.

## CLI Rules

- Use System.CommandLine.
- Follow [CLI.md](CLI.md).
- Keep help/output English-first.
- Use Application use cases.
- Verify parsing and help output when CLI behavior changes.

When no command is provided, the app opens the TUI.

## TUI Rules

- Use Terminal.Gui.
- Follow [screens.md](screens.md) for concrete layouts.
- Keep keyboard navigation central.
- Maintain visible focus and footer shortcuts.
- Avoid mouse-dependent flows.
- Keep layout dense but legible.
- Use Application use cases.

The TUI direction is a personal cockpit for planning and execution, visually
inspired by LazyDocker/LazyGit and dense like btop.

## Persistence Rules

Monthly JSON files are the V1 operational store.

Agents must preserve:

- offline local-first behavior;
- internal IDs and public refs;
- consistent timestamps;
- item versions;
- safe writes with temp file and atomic replacement;
- one backup per monthly file;
- backup recovery when possible;
- future whole-file sync compatibility.

See [DATA_MODEL.md](DATA_MODEL.md).

## TDD and Verification

TermBullet follows TDD.

Before production implementation:

1. Write unit tests first.
2. Cover successful paths with valid mocked/controlled data.
3. Cover invalid, missing, malformed, or conflicting data.
4. Confirm tests fail for the expected reason when practical.
5. Implement the smallest production change.
6. Run relevant tests again.

Preferred verification:

```bash
dotnet restore
dotnet build
dotnet test
```

Local run command:

```bash
dotnet run --project src/TermBullet -- [command] [arguments] [options]
```

For CLI changes, verify parsing and help. For TUI changes, verify navigation,
focus, and rendering where practical. For persistence changes, verify read/write
and backup/recovery.

If tests cannot be run, report why and state the remaining risk.

## Documentation Rules

Update documentation when behavior, commands, architecture, data model, or
workflow changes.

- Product scope: [PRODUCT.md](PRODUCT.md)
- CLI: [CLI.md](CLI.md)
- TUI: [screens.md](screens.md)
- Architecture: [ARCHITECTURE.md](ARCHITECTURE.md) and [ADR.md](ADR.md)
- Data model: [DATA_MODEL.md](DATA_MODEL.md)
- Plan/backlog: [BACKLOG.md](BACKLOG.md)
- Agent rules: this file

## File and Git Safety

- Do not revert user changes unless explicitly asked.
- Inspect existing files before editing.
- Keep changes scoped.
- Use `Development` as the default base branch.
- Do not run destructive git operations unless explicitly requested.

## Decision Checklist

Before implementing, confirm:

1. Is it in V1 scope?
2. Does it preserve local-first behavior?
3. Do CLI/TUI use Application use cases?
4. Does it preserve public ref and internal ID rules?
5. Does it fit the accepted stack?
6. Does it need an ADR?
7. Does documentation need an update?
