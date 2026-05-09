# TermBullet Architecture

TermBullet V1 uses a modular monolith: one production .NET project, one
executable, clear internal modules by folder/namespace, and one test project.

This keeps the product simple while preserving boundaries for future AI,
calendar, and sync/cloud work.

## Solution Layout

```text
TermBullet/
├── TermBullet.sln
├── src/TermBullet/
│   ├── Program.cs
│   ├── Bootstrap/
│   ├── Core/
│   ├── Application/
│   ├── Infrastructure/
│   ├── Cli/
│   └── Tui/
└── tests/TermBullet.Tests/
    ├── Core/
    ├── Application/
    ├── Infrastructure/
    ├── Cli/
    └── Tui/
```

## Modules

### Bootstrap

Composition root. Handles startup, dependency registration, startup maintenance,
and CLI/TUI dispatch. Bootstrap may depend on all modules.

### Core

Entities, value objects, enums, domain rules, public refs, validation, and item
status transitions.

Core must not depend on Application, Infrastructure, CLI, TUI, Terminal.Gui,
System.CommandLine, JSON storage, or PostgreSQL.

### Application

Use cases, request/response contracts, repository ports, orchestration, and
transaction boundaries.

Application may depend on Core. It must not depend on concrete persistence,
System.CommandLine, Terminal.Gui, CLI, or TUI.

### Infrastructure

Monthly JSON persistence, safe writes, backup/recovery, local index, data path
reporting, import/export, clocks, ID generation, and future AI/calendar/sync
adapters.

Infrastructure implements Application contracts.

### CLI

System.CommandLine commands, argument/option mapping, handlers, and text output.

CLI calls Application use cases and must not implement business rules.

### TUI

Terminal.Gui startup, screens, panels, keyboard navigation, focus, view models,
and action dispatch.

TUI calls Application use cases and must not implement business rules.

## Dependency Direction

```text
Bootstrap
  ├── Cli
  ├── Tui
  ├── Infrastructure
  └── Application

Cli ───────────────┐
Tui ───────────────┼──> Application ───> Core
Infrastructure ────┘
```

Forbidden dependencies:

- Core -> Application, Infrastructure, CLI, or TUI
- Application -> Infrastructure, CLI, or TUI
- CLI -> Infrastructure directly
- TUI -> Infrastructure directly

If CLI or TUI needs data, request it through Application use cases.

## Namespace Conventions

Use module-based namespaces:

```text
TermBullet.Core.Items
TermBullet.Core.Refs
TermBullet.Application.Items
TermBullet.Application.Ports
TermBullet.Infrastructure.Persistence.JsonFiles
TermBullet.Infrastructure.Export
TermBullet.Cli
TermBullet.Tui.Screens
TermBullet.Tui.Navigation
TermBullet.Bootstrap
```

Avoid vague namespaces such as `Common`, `Helpers`, or `Utils`.

## Runtime Flows

CLI:

```text
Program -> Bootstrap -> System.CommandLine -> handler
-> Application use case -> repository port -> Infrastructure -> output
```

TUI:

```text
Program -> Bootstrap -> Terminal.Gui -> screen/panel action
-> Application use case -> repository port -> Infrastructure -> screen refresh
```

Persistence:

```text
Application port -> JSON repository -> safe write -> backup/index update
```

## Persistence Constraints

Monthly JSON files are the V1 operational store.

- Application defines contracts.
- Infrastructure implements contracts.
- Core does not know storage exists.
- Public refs are persisted and never reused.
- Writes use temp files and atomic replacement.
- One backup is kept per monthly file.
- Corrupted files should recover from backup when possible.

See [DATA_MODEL.md](DATA_MODEL.md).

## Testing Architecture

Tests live in one test project organized by module:

- Core: domain rules and state transitions.
- Application: use cases with mocked repositories/clocks/IDs.
- Infrastructure: JSON persistence, backup/recovery, indexes, import/export.
- CLI: parsing, handlers, output, and representative help.
- TUI: view models, navigation state, focus, and action dispatch where practical.

Production implementation starts after tests are written when practical.

## Future Extraction Rule

Do not split production modules into separate projects until there is a concrete
need such as build time, separate packaging, a sync/cloud service, or repeated
boundary violations that project references would materially prevent.
