# TermBullet Product

TermBullet is a local-first terminal planner for tasks, notes, events, and
personal review workflows. It provides a TUI and a CLI over the same
Application use cases.

TermBullet is English-first, open source, terminal-first, and designed for
developers and technical users who prefer fast local tools.

## Principles

- **Local-first:** V1 works fully offline and stores operational data locally.
- **CLI + TUI:** both interfaces are first-class and must share use cases.
- **Optional AI:** AI is a planning assistant, never a product dependency.
- **Optional integrations:** calendar, sync, and cloud are later extensions.
- **Evolutionary architecture:** V1 must prepare V2/V3/V4 without rewrites.
- **Terminal-first:** keyboard-driven, dense, legible, and predictable.
- **Open source:** documentation, examples, labels, and commands are English.

## V1 Scope

V1 delivers the offline core:

- tasks, notes, and events;
- Today, Week, Month, Backlog, Notes, and Events collections;
- Forgotten as a derived review list for unresolved tasks from previous months;
- CLI;
- TUI MVP;
- local monthly JSON persistence;
- local JSON search index;
- search;
- basic editing;
- item migration and movement;
- first-run local data directory selection;
- local data path discovery.

V1 does not include:

- AI execution;
- Google Calendar integration;
- machine sync;
- cloud accounts;
- PostgreSQL runtime dependency for local usage.

Future-facing seams are allowed when they keep V1 simple and local-first.

## Item Model

TermBullet supports three item types:

- **Task:** executable pending work.
- **Note:** record, context, or observation.
- **Event:** appointment or internal time-based marker.

Every relevant item has:

- internal global ID;
- public ref;
- type;
- content;
- optional description;
- status;
- collection;
- scheduled date/time when it is an event;
- task priority;
- tag;
- creation and update timestamps;
- version.

Task and Event are distinct concepts. A task must not be automatically converted
into an event.

Priority is operational task metadata. Notes and events do not expose priority
and are stored with `none`.

Tags are local metadata labels. Each item has exactly one tag. If no tag is
selected, the protected `default` tag is used. V1 also keeps a local tag catalog
for creating named tags with optional descriptions before they are attached to
items.

Official task statuses in V1:

- `open`
- `done`
- `cancelled`

`migrate` is an action, not a status. It moves the same open task to another
collection while preserving the internal ID and public ref.

Tasks are planned by collection, not by date. Task creation chooses `today`,
`week`, `month`, or `backlog`. Notes are stored in `notes`. Events are stored in
`events`, and their dates belong only to `scheduled_at`.

Forgotten is the review area for open tasks from previous monthly files that
were not done or cancelled. It is derived from item state and
public-ref/monthly-file history, not a persisted item collection.

Operational views stay focused on the active period: Today, Week, Month, and
Calendar use current-month data for execution. On month rollover, open tasks and
notes outside `default` carry into the current month with their existing
collection so long-running tagged work remains visible. Events do not carry
over. Open `default` tasks from older monthly files appear in Forgotten, while
Search may read across all monthly files as a lookup surface.

Manual migration must always declare the destination:

- `today`;
- `week`;
- `month`;
- `backlog`.

When a task is migrated, the same item changes `collection`, remains `open`,
and keeps the same internal ID and public ref. History records only the
collection change.

## Local Configuration

On first execution, TermBullet asks where local operational data should be
stored. The selected base directory is saved in:

```text
<install-dir>/conf.json
```

The config contains:

```json
{
  "data_root": "/path/to/TermBullet"
}
```

TermBullet stores monthly JSON, tags, and index files under
`<data_root>/data`. Startup validates that the selected directory is readable
and writable. If the install directory cannot write `conf.json`, TermBullet
fails with a clear permission error instead of using a fallback location.

## Public Refs

Public refs are the human-facing identifiers used in CLI and TUI.

Format:

```text
<type>-<MMYY>-<sequence>
```

Prefixes:

- `t` = task
- `n` = note
- `e` = event

Examples:

```text
t-0426-1
n-0426-1
e-0426-1
```

Rules:

- sequence is independent by type and month/year;
- public refs are persisted and never reused inside the same period;
- migrated items preserve their public ref;
- internal ID remains the real identity for persistence and future sync.

## Interfaces

The CLI is documented in [CLI.md](CLI.md).

The TUI is documented in [screens.md](screens.md). The active V2 scope is:

- Main Dashboard;
- Search / Command Palette;
- Item Detail;
- Planning;
- Week View;
- Month View;
- Backlog Triage;
- Daily Review;
- Forgotten Review;
- Notes;
- Calendar;
- Tags;
- Migrate Item;
- Add Item as an auxiliary keyboard-only flow.
- Edit Item as an auxiliary keyboard-only flow.

Deferred TUI screens:

- Review;
- External calendar integration in V3;
- Sync / Cloud in V4.

## Roadmap

### V1 - Offline Core MVP

- CLI and TUI MVP;
- tasks, notes, and events;
- Today, Week, Month, Backlog, Notes, and Events collections;
- Forgotten review;
- Week view;
- monthly JSON files;
- local JSON index;
- readable public refs.

### V2 - AI Planning and Daily Review

V2 is the current published line. It extends the offline core with optional AI
planning and manual daily rollover support while preserving local-first
behavior.

- Planning workspace with guided planning inputs for fresh project plans;
- BYOK AI setup managed through `<data_root>/.aiconf`;
- named AI profiles with one active profile selected by `default=true` or
  `set-ai`;
- CLI `ai chat` as a terminal-first planning interface using the same modes as
  the TUI;
- a canonical planning and Bullet Journal specialist agent loaded for every AI
  planning request;
- conversational planning for CLI flows that can ask clarifying questions before
  a draft is ready;
- New Planning flow for fresh work;
- structured drafts that must be approved before any persistence.
- Daily Review for stale open Today tasks using item history instead of task
  due dates.

New Planning supports:

- guided project planning from a topic, project tag, detail level, and whether
  the first task should start today;
- detail levels: `high` where each task is a single atomic action, and `low`
  where each task represents approximately 1 day or 2 hours of work;
- the AI decides the total number of tasks based on topic complexity and the
  selected detail level;
- collection guardrails: today max 2 (if start today enabled), week 2-10,
  month 10+, backlog unlimited;
- ordered task content where every generated task starts with a growing numeric
  prefix such as `1.`, `2.`, `3.`.

Planning interpretation rules:

- if the user explicitly names a tag, the draft must use that tag;
- if the user explicitly names a tag for an ongoing personal habit, the draft
  should treat it as a lightweight project plan instead of forcing `default`;
- if the user does not name a tag for weekly personal planning, the draft uses
  `default`;
- if the user asks for tasks in `today`, `week`, `month`, or `backlog`, the
  draft must assign those task collections directly;
- if the plan is likely to exceed the current month, future tracking work should
  go to `backlog`;
- ordered user requests are represented by the ordered draft preview and the
  ordered action list, without adding a new persisted ordering field in V2 MVP.

Reviewing existing plans is a future idea, not part of the current Planning
scope. It is intentionally deferred because the current design is optimized for
small prompt windows and narrow context, and broad historical review is not
reliable enough yet.

AI should operate on filtered context from local data. It should not send all
JSON files by default. AI never writes directly; it may respond with normal
conversation while refining the plan. Provider responses use one JSON envelope:
`draft_ready=false` carries chat text, while `draft_ready=true` carries a
structured draft. TermBullet validates that draft, and the user must approve
before Application use cases create or change items.

AI connection settings are not edited inside the TUI in V2 MVP. Users configure
provider, model, optional base URL, and API key source in the user-editable
`<data_root>/.aiconf` file, then validate or switch profiles with CLI commands.
The TUI only shows whether AI is configured and which profile is active.

TermBullet recommends OpenCode Zen with the free `deepseek-v4-flash-free` model
for V2 AI planning. Other hosted or local OpenAI-compatible providers remain
supported through named profiles in `.aiconf`, but no local model is recommended
by default.
Profiles can declare whether a model is a reasoning model and can tune test,
chat, and planning token budgets independently, allowing direct-response models
and hosted reasoning models to coexist in the same configuration file.

CLI `ai chat` uses the active AI profile by default and supports interactive
planning commands such as mode selection, conversational replies, draft preview,
discard, and explicit apply. It is a planning interface, not unrestricted
autonomous execution.
Interactive AI planning keeps recent conversation turns in the active session so
follow-up prompts can refer to prior assistant replies before a draft is ready.
Explicit creation prompts for tasks, plans, roadmaps, or drafts must produce a
structured draft for approval instead of continuing as open-ended chat.
If the model answers a required draft request with normal chat, TermBullet makes
one automatic repair attempt that asks the model to return only the filled draft
JSON template.

The planning agent prompt is installed at
`<install-dir>/agents/planning-bulletjournal-agent.md`. TermBullet must load it
for every AI planning request. If the agent cannot be loaded, AI planning fails
before any provider call.

### V3 - Google Calendar

- optional calendar integration;
- read daily events;
- show schedule context;
- use schedule context for AI planning;
- create events from TermBullet when explicitly requested.

Calendar is auxiliary context, not the product center.

### V4 - Sync + Cloud

- optional sync/cloud;
- authentication;
- push/pull;
- whole-file monthly JSON synchronization;
- conflict handling;
- sync history;
- PostgreSQL backend storing the same JSON file content.

Cloud must never become mandatory, and local JSON files must not become a
disposable cache.

## Acceptance Criteria

V1 is adequate when:

1. users can use TermBullet locally without internet access;
2. TUI MVP navigation works for Main Dashboard, Search, and Add Item;
3. CLI manipulates items without opening the TUI;
4. tasks, notes, and events can be created, listed, edited, and changed;
5. public refs follow the official format;
6. architecture is ready for optional AI, calendar, and sync;
7. documentation is English-first and open-source ready.
