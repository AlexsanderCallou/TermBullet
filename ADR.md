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
| 0010 | Rejected | Export/import are not part of TermBullet V1. |
| 0011 | Accepted | Project is open source and English-first. |
| 0012 | Accepted | Official stack is .NET 8/C#, Terminal.Gui, System.CommandLine, monthly JSON files, local JSON index, future PostgreSQL. |
| 0013 | Accepted | V1 uses a modular monolith. |
| 0014 | Accepted | V1 stores operational data in monthly JSON files. |
| 0015 | Accepted | License is Apache-2.0. |
| 0016 | Accepted | First run stores `conf.json` in the install directory. |

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

## ADR-0003 - Simple Folder Architecture

Use Domain, Application, Repositories, Services, CLI, TUI, and Bootstrap
boundaries.

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

Internal ID is the integrity basis for persistence and future sync.

Rejected: only sequential numbers, only UUIDs, and title-as-identifier.

## ADR-0005 - Initial Item Model

V1 uses:

- task;
- note;
- event.

Required initial collections:

- Today;
- Week;
- Month;
- Backlog.

Week and Month are task collections, not dated task schedules. Events use
`scheduled_at`.

Forgotten is a derived review list for unresolved open tasks from previous
monthly files. It is not a persisted item collection in V1.

Tasks and Events remain distinct; a task must not automatically become an event.
Priority is task metadata in V1. Notes and events do not expose priority and
are stored with `none`.

Official task statuses are `open`, `done`, and `cancelled`.
`migrate` is an intentional action that moves the same task to another
collection. It is not a status. The task keeps its internal ID and public ref.

Tasks are planned by collection. Quick Task creates a task in Today, and normal
task creation chooses Today, Week, Month, or Backlog. Dates belong to events,
not tasks. Open tasks from previous monthly files appear in Forgotten for manual
review.

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

## ADR-0010 - No Export or Import

TermBullet V1 does not provide export or import commands.

Rationale:

- local monthly JSON files are already user-accessible;
- explicit export/import flows add validation and conflict complexity that is
  not necessary for the product direction;
- future sync/cloud remains a separate optional capability.

Rejected: adding backup/restore style export/import flows to V1.

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

## ADR-0016 - Install Directory Configuration

TermBullet stores its runtime configuration in:

```text
<install-dir>/conf.json
```

The first execution asks the user to choose the base data directory. The
selected path is stored as `data_root` in `conf.json`. TermBullet then stores
operational data under:

```text
<data_root>/data
```

Consequences:

- the data directory no longer depends on the shell's current working directory;
- CLI and TUI startup use the same configuration flow;
- the install directory must be writable so `conf.json` can be created;
- if `conf.json` cannot be written, TermBullet fails with a clear permission
  error instead of falling back to another location;
- the selected data root is validated before it is saved.

Rejected: storing operational data relative to the current working directory,
and silently falling back to a user-profile config file when the install
directory is not writable.

## Future ADR Candidates

- release automation;
- package manager ownership;
- V4 conflict handling details;
- optional AI provider presets if BYOK evolves.
