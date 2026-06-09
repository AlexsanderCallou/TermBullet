# TermBullet Architecture

TermBullet uses one production .NET project with clear folders and simple
names. The goal is readability first: domain rules, application actions,
repositories, services, CLI, and TUI live in predictable places without heavy
architecture vocabulary.

## Solution Layout

```text
TermBullet/
├── TermBullet.sln
├── src/TermBullet/
│   ├── Program.cs
│   ├── Bootstrap/
│   ├── Domain/
│   ├── Application/
│   ├── Repositories/
│   │   ├── Interfaces/
│   │   └── Json/
│   ├── Services/
│   ├── Cli/
│   └── Tui/
└── tests/TermBullet.Tests/
    ├── Domain/
    ├── Application/
    ├── Repositories/
    ├── Services/
    ├── Cli/
    └── Tui/
```

## Folders

### Bootstrap

Composition root. Handles startup, object wiring, startup maintenance, and
CLI/TUI dispatch. Bootstrap may depend on all production folders.

### Domain

Entities, value objects, enums, public refs, validation, and item status
transitions.

Domain must not depend on Application, Repositories, Services, CLI, TUI,
Terminal.Gui, System.CommandLine, JSON storage, or PostgreSQL.

### Application

Use cases, request/response models, orchestration, and business workflows.

Application may depend on Domain and repository/service interfaces. It must not
depend on concrete JSON repositories, System.CommandLine, Terminal.Gui, CLI, or
TUI.

### Repositories

Repository interfaces and local JSON repository implementations.

- `Repositories/Interfaces` contains repository contracts used by Application.
- `Repositories/Json` contains JSON-backed implementations and JSON storage
  models.

Repositories must not contain business rules. They persist and retrieve data.

### Services

Technical services that are not repositories, such as clocks, ID generation,
data transfer, history maintenance, and startup maintenance contracts.

### CLI

System.CommandLine commands, argument/option mapping, handlers, and text output.

CLI calls Application use cases and must not implement business rules.

### TUI

Terminal.Gui screens, panels, keyboard navigation, focus, view models, and
action dispatch.

TUI calls Application use cases and must not implement business rules.

## Dependency Direction

```text
Bootstrap
  ├── Cli
  ├── Tui
  ├── Repositories
  ├── Services
  └── Application

Cli ───────┐
Tui ───────┼──> Application ───> Domain
Services ──┤
Repositories ┘
```

Forbidden dependencies:

- Domain -> Application, Repositories, Services, CLI, or TUI
- Application -> concrete JSON repositories, CLI, or TUI
- CLI -> JSON repositories directly
- TUI -> JSON repositories directly

If CLI or TUI needs data, request it through Application use cases.

## Namespace Conventions

Use readable folder-based namespaces:

```text
TermBullet.Domain.Items
TermBullet.Domain.Refs
TermBullet.Application.Items
TermBullet.Repositories.Interfaces
TermBullet.Repositories.Json
TermBullet.Services.Clock
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
-> Application use case -> repository interface -> JSON repository -> output
```

TUI:

```text
Program -> Bootstrap -> Terminal.Gui -> screen/panel action
-> Application use case -> repository interface -> JSON repository -> screen refresh
```

Persistence:

```text
Application use case -> repository interface -> JSON repository -> safe write -> backup/index update
```

V2 AI planning:

```text
TUI/CLI -> Application planning use case
        -> AI agent prompt loader
        -> filtered context assembler
        -> AI service interface -> provider adapter
        -> structured draft validator
        -> Application apply use case
        -> repository interface -> JSON repository
```

The canonical planning agent prompt lives in source at:

```text
src/TermBullet/Services/Ai/Agents/planning-bulletjournal-agent.md
```

Published builds copy it to:

```text
<install-dir>/agents/planning-bulletjournal-agent.md
```

The AI planning runtime must load this agent prompt before every planning model
request. If the agent file is missing or unreadable, AI planning must fail with
a clear configuration error instead of calling the model without the agent.

AI provider adapters live in Services or a Services subfolder. Domain must not
know about AI providers, prompts, API keys, HTTP clients, or model-specific
response formats. TUI and CLI must not call AI providers directly.

## Persistence Constraints

Monthly JSON files are the V1 operational store.

- Application defines workflows.
- Repository interfaces describe persistence needs.
- JSON repositories implement those interfaces.
- Domain does not know storage exists.
- Public refs are persisted and never reused.
- Writes use temp files and atomic replacement.
- One backup is kept per monthly file.
- Corrupted files should recover from backup when possible.

See [DATA_MODEL.md](DATA_MODEL.md).

## Testing Architecture

Tests live in one test project organized by folder:

- Domain: domain rules and state transitions.
- Application: use cases with mocked repositories/clocks/IDs.
- Repositories: JSON persistence, backup/recovery, and indexes.
- Services: data transfer and technical service behavior.
- CLI: parsing, handlers, output, and representative help.
- TUI: view models, navigation state, focus, and action dispatch where practical.

Production implementation starts after tests are written when practical.

## Future Extraction Rule

Do not split production folders into separate projects until there is a concrete
need such as build time, separate packaging, a sync/cloud service, or repeated
boundary violations that project references would materially prevent.
