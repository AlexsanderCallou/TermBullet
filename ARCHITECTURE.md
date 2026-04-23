# TermBullet - Architecture

This document describes the concrete technical architecture for TermBullet V1.

TermBullet is a small terminal-first product, so the production codebase will use a **modular monolith** instead of a multi-project architecture. The system keeps clear internal boundaries through folders, namespaces, contracts, and tests, while shipping as one .NET executable.

## Architectural Style

TermBullet uses a modular monolith:

- one production .NET project;
- one deployable executable;
- internal modules separated by namespace and folder;
- one test project organized by module;
- clear dependency rules between internal modules.

This keeps the project simple enough for its size while preserving the ability to evolve toward AI, calendar integration, and sync/cloud later.

## Solution Layout

Target repository structure:

```text
TermBullet/
├── TermBullet.sln
├── src/
│   └── TermBullet/
│       ├── TermBullet.csproj
│       ├── Program.cs
│       ├── Bootstrap/
│       ├── Core/
│       ├── Application/
│       ├── Infrastructure/
│       ├── Cli/
│       └── Tui/
├── tests/
│   └── TermBullet.Tests/
│       ├── TermBullet.Tests.csproj
│       ├── Core/
│       ├── Application/
│       ├── Infrastructure/
│       ├── Cli/
│       └── Tui/
├── README.md
├── product-spec.md
├── ADR.md
├── AGENTS.md
├── DATA_MODEL.md
├── DEVELOPMENT_PLAN.md
└── CONTRIBUTING.md
```

## Production Modules

### Bootstrap

Namespace: `TermBullet.Bootstrap`

Responsibilities:

- application startup;
- dependency registration;
- configuration loading;
- command dispatch;
- deciding whether to open CLI flow or TUI flow.

Bootstrap is the composition root and may depend on all other internal modules.

### Core

Namespace: `TermBullet.Core`

Responsibilities:

- entities;
- value objects;
- enums;
- domain rules;
- public ref policy;
- item status transitions;
- validation that does not require storage or UI.

Core must not depend on Infrastructure, CLI, TUI, Terminal.Gui, System.CommandLine, JSON file storage, or PostgreSQL.

### Application

Namespace: `TermBullet.Application`

Responsibilities:

- use cases;
- input/output DTOs;
- application services;
- repository contracts;
- transaction boundaries;
- orchestration between Core and persistence contracts.

Application may depend on Core. It must not depend on CLI, TUI, Terminal.Gui, System.CommandLine, or concrete file/storage APIs.

### Infrastructure

Namespace: `TermBullet.Infrastructure`

Responsibilities:

- monthly JSON file repositories;
- safe file writer;
- backup/recovery services;
- local JSON index;
- local settings storage;
- import/export adapters;
- clock and ID generation adapters;
- future AI/calendar/sync adapters.

Infrastructure implements contracts defined by Application.

### CLI

Namespace: `TermBullet.Cli`

Responsibilities:

- System.CommandLine command definitions;
- argument and option mapping;
- command handlers;
- text output;
- future JSON output.

CLI calls Application use cases. It must not implement business rules directly.

### TUI

Namespace: `TermBullet.Tui`

Responsibilities:

- Terminal.Gui app startup;
- screens;
- panels/windows;
- keyboard navigation;
- focus behavior;
- screen view models;
- mapping user actions to Application use cases.

TUI calls Application use cases. It must not implement business rules directly.

## Dependency Direction

Allowed dependency direction:

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

Core is the most stable module. Bootstrap is the outermost module.

Forbidden dependencies:

- Core -> Application
- Core -> Infrastructure
- Core -> CLI
- Core -> TUI
- Application -> Infrastructure
- Application -> CLI
- Application -> TUI
- CLI -> Infrastructure directly
- TUI -> Infrastructure directly

If CLI or TUI needs data, it must request it through Application use cases.

## Namespace Conventions

Use namespaces that mirror internal modules:

```text
TermBullet.Core.Items
TermBullet.Core.Refs
TermBullet.Application.Items
TermBullet.Application.Ports
TermBullet.Infrastructure.Persistence.JsonFiles
TermBullet.Infrastructure.Export
TermBullet.Cli.Commands
TermBullet.Cli.Rendering
TermBullet.Tui.Screens
TermBullet.Tui.Navigation
TermBullet.Bootstrap
```

Avoid generic namespaces such as `Common`, `Helpers`, or `Utils` unless a clearer module name is not possible.

## CLI Flow

Expected CLI flow:

```text
Program
  -> Bootstrap
  -> System.CommandLine parser
  -> command handler
  -> Application use case
  -> repository contract
  -> JSON file implementation
  -> command output renderer
```

Example:

```text
termbullet add "fix jwt authentication"
  -> AddCommand
  -> CreateItemUseCase
  -> IItemRepository
  -> JsonFileItemRepository
  -> "[ok] task created: t-0426-1"
```

## TUI Flow

Expected TUI flow:

```text
Program
  -> Bootstrap
  -> Terminal.Gui application
  -> screen
  -> panel action
  -> Application use case
  -> repository contract
  -> JSON file implementation
  -> screen state refresh
```

The TUI should keep screen state and focus state, but domain state belongs to Core/Application and persistence.

## Persistence Flow

Monthly JSON files are the V1 operational data store.

Persistence rules:

- Application defines repository contracts.
- Infrastructure implements repository contracts with monthly JSON files.
- Core does not know JSON file storage exists.
- Timestamps must be stored consistently.
- Public refs must be persisted and never reused.
- Writes must use a temporary file and atomic replacement.
- One backup must be kept per monthly file.
- Corrupted files should be recovered from backup when possible.

## Testing Architecture

TermBullet follows TDD.

There is one test project organized by module:

```text
tests/TermBullet.Tests/
├── Core/
├── Application/
├── Infrastructure/
├── Cli/
└── Tui/
```

Testing focus:

- Core tests validate domain rules and state transitions.
- Application tests validate use cases with mocked repositories.
- Infrastructure tests validate JSON file persistence, backup/recovery, and index rebuilds.
- CLI tests validate command parsing, handlers, and output.
- TUI tests validate view models, navigation state, and action dispatch where practical.

Production implementation starts only after unit tests are written.

## Future Extraction Rule

Do not split production modules into separate projects until there is a concrete need.

Extraction may be considered only when:

- build time becomes a real issue;
- module boundaries need separate packaging;
- sync/cloud becomes a separate deployable service;
- contributors repeatedly violate internal dependency rules and project boundaries would materially help.

Until then, keep the production code as a modular monolith.
