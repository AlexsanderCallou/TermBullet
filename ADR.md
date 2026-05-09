# TermBullet Architecture Decision Records

This file is the concise index of accepted product and architecture decisions.
Use it as the highest-priority source when documents conflict.

Long-form ADRs may be split into `docs/adr/` later if more detail is needed.

## Decision Index

| ADR | Status | Decision |
| --- | --- | --- |
| 0001 | Accepted | TermBullet is local-first. |
| 0002 | Accepted | CLI and TUI are first-class interfaces. |
| 0003 | Accepted | Use layered architecture boundaries. |
| 0004 | Accepted | Use global internal ID plus public ref. |
| 0005 | Accepted | V1 item model is task, note, event. |
| 0006 | Accepted | Local persistence is the operational source. |
| 0007 | Accepted | AI, calendar, and sync are optional modules. |
| 0008 | Superseded | Future sync at JSON file level; superseded by ADR-0014. |
| 0009 | Accepted | TUI is based on screens, panels, and keyboard. |
| 0010 | Accepted | Export/import are basic V1 portability features. |
| 0011 | Accepted | Project is open source and English-first. |
| 0012 | Accepted | Official stack is .NET 8/C#, Terminal.Gui, System.CommandLine, monthly JSON files, local JSON index, future PostgreSQL. |
| 0013 | Accepted | V1 uses a modular monolith. |
| 0014 | Accepted | V1 stores operational data in monthly JSON files. |
| 0015 | Accepted | License is Apache-2.0. |

## ADR-0001 - Local-First Product

V1 must work without internet, accounts, AI, external calendars, or cloud.
External services are optional future extensions.

Consequences:

- local JSON files are the operational source;
- domain must not depend on external providers;
- future integrations attach through ports/adapters.

Rejected: cloud-first and local-as-cache.

## ADR-0002 - CLI and TUI as First-Class Interfaces

The executable opens the TUI when no command is provided, and the CLI remains a
complete interface for essential operations.

Consequences:

- CLI and TUI call the same Application use cases;
- command design must be documented and predictable;
- business rules do not live in either interface.

Rejected: TUI-only and CLI-only.

## ADR-0003 - Layered Architecture

Use Core, Application, Infrastructure, CLI, TUI, and Bootstrap boundaries.

Central rule:

```text
CLI and TUI must reuse Application use cases.
```

Rejected: procedural monolith and framework-driven architecture.

## ADR-0004 - Internal ID and Public Ref

Each item has:

- stable internal global ID;
- human-facing public ref.

Public ref format:

```text
<type>-<MMYY>-<sequence>
```

Examples:

```text
t-0426-1
n-0426-1
e-0426-1
```

Internal ID is the integrity basis for persistence, import/export, and future
sync.

Rejected: only sequential numbers, only UUIDs, and title-as-identifier.

## ADR-0005 - Initial Item Model

V1 uses:

- task;
- note;
- event.

Required initial collections:

- Today;
- Backlog;
- Forgotten.

Week is not a persisted collection. It is a planning view derived from task
`planned_for` dates.

Tasks and Events remain distinct; a task must not automatically become an event.

Official task statuses are `open`, `done`, `canceled`, and `migrated`.
`migrated` represents an intentional move out of a previous planned placement.
The source item remains stored as migrated, and the destination is a new open
task that records the source item.

Tasks created for Today are planned for Today by default. Future planned dates
must be intentional. Open tasks planned before today move into Forgotten for
manual review when the app starts or begins a new day.

Rejected: generic-only item model and many V1 item types.

## ADR-0006 and ADR-0014 - Local Monthly JSON Storage

V1 uses monthly JSON files as the local operational store.

File layout:

```text
data/<year>/data_<month>_<year>.json
```

Rules:

- local and offline;
- consistent timestamps;
- item versioning;
- persisted IDs and public refs;
- safe writes with temp file and atomic replacement;
- one backup per monthly file;
- recovery from backup when possible;
- local JSON index for lookup/search;
- V1 assumes one active machine at a time.

V4 sync/cloud will synchronize whole JSON files. PostgreSQL is future optional
backend storage for the same JSON file content, not a V1 dependency.

Rejected: single JSON file, SQLite for V1, mandatory remote database,
PostgreSQL-only storage, and entity-level cloud storage for V1.

## ADR-0007 - Optional Integrations

AI, Google Calendar, sync, and cloud are optional modules.

V2 AI follows BYOK:

- provider;
- model;
- API key;
- optional base URL;
- internal profiles such as `plan-day`, `review-day`, `breakdown-task`, and
  `prioritize-backlog`.

Rejected: mandatory AI and mandatory cloud sync from V1.

## ADR-0008 - Future Sync at JSON File Level

Superseded by ADR-0014, but the direction remains: future sync works over whole
monthly JSON files, keeps complete local data on each machine, and treats
conflicts as normal.

## ADR-0009 - TUI Screens, Panels, and Keyboard

The TUI is terminal-first, panel-based, and keyboard-driven. It should feel dense
and operational, close to LazyDocker/LazyGit/K9s/btop patterns.

Cross-screen behaviors:

- `Tab` and `Shift+Tab` move focus;
- `Enter` opens or expands active context;
- `Esc` returns;
- `/` filters/searches;
- `c` captures;
- `?` opens compact help.

Concrete layouts are maintained in [screens.md](screens.md).

Rejected: sequential prompt interface and mouse-dependent interface.

## ADR-0010 - Export and Import

V1 provides JSON export/import for backup, migration, and portability.

Export/import must preserve IDs, refs, types, statuses, collections, priorities,
tags, timestamps, and relevant history.

Rejected: postponing export/import until sync and exporting only rendered text.

## ADR-0011 - Open Source and English-First

Documentation, commands, examples, user-facing text, and architecture records are
English-first. License is Apache-2.0 and legal/trademark wording is centralized
in [TRADEMARKS.md](TRADEMARKS.md).

Rejected: non-English-first docs and publishing without explicit legal posture.

## ADR-0012 - Official Technology Stack

Accepted stack:

- .NET 8 / C#;
- Terminal.Gui;
- System.CommandLine;
- monthly JSON files;
- local JSON index;
- future PostgreSQL for optional V4 sync/cloud.

Rejected alternatives: Go/Bubble Tea, Rust/ratatui, Node terminal UI stack,
SQLite for V1, and PostgreSQL-only storage.

## ADR-0013 - Modular Monolith

Production code lives in:

```text
src/TermBullet/TermBullet.csproj
```

Internal modules are separated by folders/namespaces. Tests live in:

```text
tests/TermBullet.Tests/TermBullet.Tests.csproj
```

Rejected: one production project per layer for V1 and unstructured single-folder
code.

## ADR-0015 - Apache-2.0 License

TermBullet uses Apache License 2.0 with:

```text
Copyright (c) 2026 TermBullet contributors
```

Rejected: MIT, GPL-family licenses, and no license.

## Future ADR Candidates

- release automation;
- package manager ownership;
- V4 conflict handling details;
- optional AI provider presets if BYOK evolves.
