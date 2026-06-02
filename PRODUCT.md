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

The TUI is documented in [screens.md](screens.md). The active MVP scope is:

- Main Dashboard;
- Search / Command Palette;
- Item Detail;
- Planning placeholder;
- Week View;
- Month View;
- Backlog Triage;
- Forgotten Review;
- Notes;
- Calendar;
- Tags;
- Migrate Item;
- Add Item as an auxiliary keyboard-only flow.

Deferred TUI screens:

- AI Planning;
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

### V2 - AI Planning

- Planning workspace with AI-assisted goal-to-task support;
- BYOK AI setup;
- provider/model/key/base URL setup;
- daily planning and review;
- task breakdown;
- backlog prioritization;
- brain dump transformation;
- preview before persisting suggestions.

AI should operate on filtered context from local data. It should not send all
JSON files by default.

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
