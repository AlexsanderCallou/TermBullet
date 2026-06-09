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
| 0017 | Accepted | Items have one tag, and non-default tagged work carries into the current month. |
| 0018 | Accepted | V2 AI planning uses approved structured drafts before applying changes. |
| 0019 | Accepted | AI provider profiles are stored in `<data_root>/.aiconf`. |
| 0020 | Accepted | Daily Review uses history-based Today rollover without task due dates. |
| 0021 | Accepted | TUI text-input screens use physical action buttons instead of letter shortcuts. |

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
- Backlog;
- Notes;
- Events.

Today, Week, Month, and Backlog are task collections, not dated task schedules.
Notes use the Notes collection. Events use the Events collection and
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
task creation chooses Today, Week, Month, or Backlog. Notes are stored in Notes.
Events are stored in Events. Dates belong to events, not tasks. Open tasks from
previous monthly files appear in Forgotten for manual review.

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

- named connection profiles;
- one active profile at a time;
- provider;
- model;
- optional base URL;
- API key source, preferably an environment variable.

The recommended user-facing setup is OpenCode Zen with the free
`deepseek-v4-flash-free` model. Hosted and local compatible services use the
same OpenAI-compatible profile contract, but TermBullet does not recommend a
local model by default.

AI connection profiles are configured in `<data_root>/.aiconf`. The CLI provides
`test-ai` for validation and `set-ai` for switching the default profile. The TUI
may show the active profile and configuration errors, but it does not edit AI
connection settings.

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

## ADR-0017 - Single Tag and Monthly Carry-Over

Each item has exactly one normalized tag stored as `tag`. When no tag is chosen,
the item uses the protected `default` tag.

Rationale:

- tags represent the planning context or long-running project for an item;
- an item belonging to multiple planning contexts would make monthly carry-over
  ambiguous;
- `default` is reserved for quick work that should be reviewed manually instead
  of silently continuing month after month.

Month rollover copies open tasks and notes with `tag != "default"` into the
current monthly file while preserving internal ID, public ref, type, collection,
content, description, priority, and tag. The copied current-month item increments
version, updates `updated_at`, and records a `carried_over` history event.

Events do not carry over. Open `default` tasks remain in their original monthly
file and appear in Forgotten. `default` notes remain searchable in archive data
but do not appear in Forgotten.

Rejected: item `tags` arrays, automatic carry-over for all open work, and event
carry-over.

## ADR-0018 - AI Planning With Approved Structured Execution

V2 introduces optional BYOK AI planning. AI is an assistant for planning and
review, not a direct persistence layer.

The planning and Bullet Journal specialist agent is a versioned product asset,
not a user-editable runtime setting. Source stores it at:

```text
src/TermBullet/Services/Ai/Agents/planning-bulletjournal-agent.md
```

Published installs place it at:

```text
<install-dir>/agents/planning-bulletjournal-agent.md
```

Every planning model request must include this agent prompt. If the prompt
cannot be loaded, TermBullet must fail the AI planning request with a clear error
instead of calling the provider without the agent.

AI connection settings use named profiles. The user can register multiple
profiles, such as the recommended OpenCode Zen profile and another
OpenAI-compatible profile, and select one active profile. V2 MVP manages these
profiles through CLI commands only.

The Planning TUI currently has one flow:

- New Planning: creates a new project plan or weekly personal plan from guided
  user inputs.

Reviewing existing plans is a future idea, not part of the current Planning
implementation. It is intentionally deferred because the current workflow is
optimized for narrow context, and broad historical review is not reliable enough
yet.

The CLI may expose the same planning modes through `termbullet ai chat`. CLI
chat uses the active AI profile unless the user selects another registered
profile. It must follow the same structured draft and explicit approval flow as
the TUI.

AI responses must become structured drafts before they can change data. The
application validates each draft and applies approved actions through
Application use cases. Monthly JSON repositories never execute AI output
directly.

V2 alpha uses preflight validation plus ordered execution for approved drafts.
It does not require transaction-level rollback for a failure after partial
application. Local monthly JSON files remain user-readable and safe writes keep
the existing backup/atomic replacement guarantees.

Allowed current V2 draft actions:

- create a tag for a new closed-scope project;
- create tasks;
- create notes.

Moving existing tasks, changing existing task priority, and cancelling stale
tasks from AI proposals are future open-work planning capabilities. They are
not part of the current published V2 apply surface.

Project Plan may create one project tag, one scope note, and the required tasks.
Weekly Plan creates tasks under the protected `default` tag.

When the user explicitly requests a tag or collection distribution, the draft
must preserve that request unless validation rejects it. Ordered plans are
represented by ordered draft actions and previews; V2 MVP does not add a new
persisted ordering field.

AI context must be filtered to the selected planning mode. TermBullet must not
send all monthly JSON files by default.

Rejected: direct AI writes to JSON files, free-form text execution without
validation, autonomous CLI execution without confirmation, deleting items from AI
proposals in V2 MVP, editing AI provider settings from the TUI in V2 MVP, and
making AI required for the local-first product.

## ADR-0019 - AI Profiles Use `.aiconf`

Status: Accepted.

TermBullet stores AI provider profiles in:

```text
<data_root>/.aiconf
```

The file is user-editable text. Lines starting with `#` are comments. Each
profile starts with `[profile-name]`, followed by `key=value` settings. The
active profile is selected by `default=true`; if only one profile exists, it is
active automatically.

Accepted settings:

- `provider`
- `model`
- `base_url`
- `api_key`
- `api_key_env`
- `default`
- `timeout_seconds`
- `reasoning`
- `test_max_tokens` (optional, for providers that require explicit limits)
- `chat_max_tokens` (optional, for providers that require explicit limits)
- `planning_max_tokens` (optional, for providers that require explicit limits)

Consequences:

- AI provider settings move out of install-directory `conf.json`;
- the data root contains both operational data and user-editable AI settings;
- direct-response models and hosted reasoning models can coexist in the same
  configuration file;
- token limits are optional and only needed for providers that require explicit
  max_tokens values;
- `termbullet test-ai` validates the selected profile and creates a commented
  template when `.aiconf` is missing;
- `termbullet set-ai <name>` switches the default profile;
- the TUI reads the active profile but does not edit provider settings.

Rejected: managing AI profiles primarily through many CLI mutation commands, and
storing AI provider settings in install-directory `conf.json`.

## ADR-0020 - Daily Review Uses History-Based Today Rollover

Status: Accepted.

TermBullet keeps task collections as Bullet Journal planning lanes:

- `today` is the current daily execution lane;
- `week` is the current weekly commitment lane;
- `month` is the current monthly intention lane;
- `backlog` is uncommitted work.

Tasks still do not persist task due dates in V2. Date traceability comes from
existing timestamps and item history:

- `created_at`;
- `updated_at`;
- `completed_at`;
- `cancelled_at`;
- history events such as `created`, `migrate`, `done`, `cancelled`, and the
  planned `daily_reviewed` event.

Today list behavior:

- tasks marked `done` or `cancelled` remain visible in the default Today view
  for the local day when the terminal status was applied;
- terminal Today tasks leave the default Today view on the next local day;
- completed and cancelled tasks remain available through item detail, history,
  search, and explicit completed/history views.

Daily rollover behavior:

- Daily Review is a manual Bullet Journal review flow, not automatic migration;
- stale open Today tasks are tasks whose latest Today placement/review happened
  before the current local date;
- Daily Review offers explicit decisions such as keep in Today, move to Week,
  move to Month, move to Backlog, mark done, or cancel;
- choosing keep in Today records history only and does not update `updated_at`;
- Forgotten remains separate because it reviews unresolved tasks from previous
  monthly files and may scan archive JSON files.

Consequences:

- the app needs a local-date-aware Today query or view model;
- the app needs a history query capable of finding the latest Today placement or
  `daily_reviewed` event;
- TUI should add a Daily Review surface instead of reusing Forgotten;
- CLI may add a manual Daily Review command after the TUI/product flow is
  settled.

Rejected: adding task due dates for V2 daily rollover, automatically migrating
stale Today tasks at startup, reusing Forgotten for daily rollover, and touching
`updated_at` when the user only reaffirms that a task stays in Today.

## ADR-0021 - Text Input Screens Use Physical Action Buttons

Status: Accepted.

Any TUI screen or popup that contains a text input control must prevent ordinary
letter keys from acting as commands while the user is typing. This includes
single-line and multiline inputs.

Rules:

- screens with `TextField` or `TextView` must perform actions through visible
  physical buttons such as `[ Save ]`, `[ Cancel ]`, `[ Generate ]`,
  `[ Apply ]`, `[ Discard ]`, or `[ Back ]`;
- ordinary letter-key actions such as `s`, `g`, `t`, `e`, `d`, `x`, and `n`
  must not trigger commands on those screens;
- `Enter` activates only the focused button or focused non-text control;
- when focus is inside a multiline text input, `Enter` keeps its text-editing
  behavior;
- list screens that need text input should open a focused popup instead of
  capturing text directly in a shortcut-heavy surface;
- checkbox and radio choices must be rendered with stable ASCII forms:
  `[ x ]`, `[   ]`, `( x )`, and `(   )`.

Consequences:

- Quick Task is the reference popup style for compact text-entry flows;
- Add Item, Edit Item, Create Tag, Search, and Planning must be audited for
  shortcut conflicts;
- screens without text inputs may keep letter shortcuts, but action panels with
  buttons are preferred for consistency;
- automated TUI tests should cover that typing letters in text fields does not
  dispatch screen actions.

Rejected: allowing letter shortcuts to remain active while a text field has
focus, relying on users to avoid shortcut letters while typing, and mixing
multiple checkbox/radio ASCII styles.

## Future ADR Candidates

- release automation;
- package manager ownership;
- V4 conflict handling details;
- optional AI provider presets if BYOK evolves.
