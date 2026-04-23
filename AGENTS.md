# TermBullet - Agent Development Guide

This file provides operating instructions for AI coding agents and automation tools working on TermBullet.

The goal is to keep development aligned with the product vision, architecture decisions, and open source direction already documented in this repository.

Official repository:

```text
https://github.com/AlexsanderCallou/TermBullet
```

The current base branch for development is:

```text
Development
```

## Source of Truth

Before making implementation decisions, agents must read and follow:

1. [README.md](README.md) for project overview and scope.
2. [product-spec.md](product-spec.md) for product requirements, command tree, and TUI behavior.
3. [ADR.md](ADR.md) for accepted architecture and technology decisions.
4. This file for agent-specific development rules.

If these documents conflict, follow this priority:

1. ADRs for accepted architecture and technology decisions.
2. Product spec for product behavior and UX.
3. README for summary-level project direction.
4. AGENTS.md for workflow and contribution rules.

When a change requires a new long-term decision, update or add an ADR instead of burying the decision only in code.

## Project Language

TermBullet is an English-first open source project.

Agents must write in English for:

- documentation;
- code comments;
- command names;
- CLI help text;
- TUI labels;
- error messages;
- examples;
- commit messages when commits are requested.

Conversation with maintainers may happen in another language, but project artifacts must remain English-first.

## Official Technology Stack

The accepted stack is:

- **.NET 8 / C#** as the main platform and implementation language.
- **Terminal.Gui** for the TUI.
- **System.CommandLine** for the CLI.
- **Monthly JSON files** as the local offline data store in V1.
- **Local JSON index** for faster lookup and search.
- **PostgreSQL** as the future backend database for sync/cloud in V4, storing the same JSON files.

Do not replace or bypass this stack without creating an ADR and getting explicit maintainer approval.

## Product Boundaries

### V1 Scope

V1 is local-first and offline.

V1 includes:

- tasks, notes, and events;
- Today, Week, and Backlog;
- CLI;
- TUI;
- local monthly JSON persistence;
- search;
- basic editing;
- migration and movement of items;
- local configuration;
- monthly JSON file storage;
- local JSON search index;
- basic export and import.

V1 does not include:

- AI execution;
- Google Calendar integration;
- machine sync;
- cloud accounts;
- PostgreSQL runtime dependency for local usage.

Future-facing seams are allowed, but agents must not implement V2/V3/V4 behavior unless explicitly requested.

## Architecture Rules

TermBullet follows a modular monolith architecture.

Production code should live in one .NET project and be separated by folders, namespaces, contracts, and tests:

- **Core**
  - entities;
  - value objects;
  - business rules;
  - state transitions;
  - identification policies.

- **Application**
  - use cases;
  - orchestration;
  - input/output contracts;
  - ports for persistence and integrations.

- **Infrastructure**
- monthly JSON file persistence;
- backup/recovery services;
- local JSON index;
  - export/import implementations;
  - future AI, calendar, sync, and PostgreSQL adapters.

- **CLI**
  - System.CommandLine commands;
  - command handlers;
  - text/JSON output formatting.

- **TUI**
  - Terminal.Gui screens;
  - panels/windows;
  - keyboard navigation;
  - focus and selection behavior.

- **Bootstrap**
  - application startup;
  - dependency registration;
  - CLI/TUI dispatch.

Core must not depend on CLI, TUI, Infrastructure, Terminal.Gui, System.CommandLine, JSON file storage, or PostgreSQL.

CLI and TUI must call Application use cases. They must not duplicate business rules.

Infrastructure implements contracts defined by Application or Core-facing abstractions. It must not leak storage-specific behavior into product rules.

## Entity Identification Rules

Every relevant item must have:

- an internal global ID;
- a persisted public ref.

Public ref format:

```text
<type>-<MMYY>-<sequence>
```

Prefixes:

- `t` = task;
- `n` = note;
- `e` = event.

Examples:

```text
t-0426-1
n-0426-1
e-0426-1
```

The public ref is for humans. The internal ID is the real identity for persistence, import/export, and future sync.

Do not use only numeric IDs as the user-facing identifier.

## CLI Rules

The CLI command tree is defined in [product-spec.md](product-spec.md).

Agents must:

- implement commands with System.CommandLine;
- keep command names and options aligned with the official command tree;
- keep help output clear and English-first;
- design output to support future `--json`;
- keep behavior consistent with equivalent TUI actions;
- call Application use cases instead of implementing logic in command handlers.

When no command is provided, the app should open the TUI.

## TUI Rules

The TUI is a first-class interface, not an afterthought.

Agents must:

- use Terminal.Gui;
- follow the screen and panel model in the product spec;
- keep keyboard navigation central;
- maintain clear active focus;
- support dense but legible layouts;
- keep shortcuts visible in footers;
- avoid mouse-dependent flows;
- call Application use cases instead of implementing business logic inside screens.

The main TUI direction is a personal cockpit for planning and execution, visually inspired by LazyDocker/LazyGit and dense like btop.

## Persistence Rules

Monthly JSON files are the V1 local operational store.

Agents must:

- preserve local-first behavior;
- keep local files usable offline;
- store internal IDs and public refs;
- store consistent creation/update timestamps;
- store item versions;
- use safe writes with temporary files and atomic replacement;
- keep one backup per monthly file;
- recover corrupted monthly files from backup when possible;
- keep future file-level sync possible.

Do not make PostgreSQL required for V1 local usage.

PostgreSQL is reserved for the optional V4 sync/cloud backend and should store the same JSON file content.

## Testing and Verification

TermBullet follows a TDD workflow.

Before starting any production implementation, agents must:

1. Write unit tests first.
2. Cover successful paths with valid mocked data.
3. Cover failure paths with invalid, missing, malformed, or conflicting mocked data.
4. Run the tests and confirm they fail for the expected reason before implementing behavior when practical.
5. Implement the smallest production change that satisfies the tests.
6. Run the full relevant test suite again.

Work is not considered complete until all relevant tests pass successfully.

When code exists, agents should prefer these verification steps:

```bash
dotnet restore
dotnet build
dotnet test
```

If the solution structure defines more specific commands, follow the repository scripts or documentation.

For changes affecting CLI behavior, verify command parsing and help output.

For changes affecting TUI behavior, verify keyboard navigation, focus behavior, and rendering where possible.

For persistence changes, verify backup/recovery, read/write flows, and import/export compatibility.

If tests cannot be run, agents must explicitly report why and describe the remaining risk.

## Documentation Rules

Agents must update documentation when behavior, architecture, or public commands change.

Use:

- `README.md` for user-facing project summary.
- `product-spec.md` for product requirements, command tree, and TUI behavior.
- `ADR.md` for accepted architectural or technology decisions.
- `AGENTS.md` for agent workflow and implementation guardrails.
- `ARCHITECTURE.md` for concrete modular monolith structure.
- `DATA_MODEL.md` for monthly JSON files, entities, history, and sync preparation.
- `DEVELOPMENT_PLAN.md` for V1 implementation order.
- `CONTRIBUTING.md` for open source contribution rules.

Do not introduce major architecture, dependency, storage, or workflow changes without updating ADRs.

## Dependency Rules

Use dependencies conservatively.

Before adding a dependency, verify that it:

- fits the official stack;
- solves a real project need;
- does not duplicate built-in .NET capabilities unnecessarily;
- is suitable for an open source project;
- does not compromise local-first usage.

Major dependencies should be documented in an ADR.

## Git and File Safety

Agents must not revert user changes unless explicitly asked.

Before editing existing files, inspect their current content.

Keep changes scoped to the requested task.

Use `Development` as the default base branch unless the maintainer explicitly asks for another branch.

Do not perform destructive git operations such as hard resets, forced checkouts, or branch rewrites unless explicitly requested by the maintainer.

## Decision Checklist

Before implementing a feature, agents should confirm:

1. Is it in V1 scope?
2. Does it preserve local-first behavior?
3. Does it keep CLI and TUI on the same Application use cases?
4. Does it preserve internal ID and public ref rules?
5. Does it fit the official .NET 8 / C# stack?
6. Does it need a new ADR?
7. Does documentation need to be updated?

If the answer to any of these is unclear, prefer a small, explicit implementation that follows the current ADRs and product spec.
