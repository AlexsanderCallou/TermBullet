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
- Today, Week, and Backlog collections;
- Forgotten as a derived review list for unresolved past planned tasks;
- CLI;
- TUI MVP;
- local monthly JSON persistence;
- local JSON search index;
- search;
- basic editing;
- item migration and movement;
- local data path discovery;
- basic export and import.

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
- planned date when it is a dated task;
- scheduled date/time when it is an event;
- task priority;
- tags;
- creation and update timestamps;
- version.

Task and Event are distinct concepts. A task must not be automatically converted
into an event.

Priority is operational task metadata. Notes and events do not expose priority
and are stored with `none`.

Tags are local metadata labels. Items store tag names, and V1 also keeps a local
tag catalog for creating named tags with optional descriptions before they are
attached to items.

Official task statuses in V1:

- `open`
- `done`
- `cancelled`
- `migrate`

`migrate` means the task was intentionally moved out of its previous planned
placement. The destination remains executable as an open task.

Tasks created for today are planned for today by default. A task should only get
a future planned date when the user intentionally creates or moves it to that
date. Backlog tasks may keep `planned_for` as `null`.

Forgotten is the review area for open tasks that were planned for a previous
day and were not done, cancelled, or marked migrate. It is derived from item
state and dates, not a persisted item collection.

Manual migration must always declare the destination:

- migrate to a specific date;
- migrate to Backlog.

When a task is moved by migration, the original item stays in the JSON with status
`migrate`. A new open task is created at the destination and records which item
it came from.

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
- migration source items preserve their public ref;
- new migration destination items get their own public ref and record the source
  ref;
- internal ID remains the real identity for persistence, import/export, and
  future sync.

## Interfaces

The CLI is documented in [CLI.md](CLI.md).

The TUI is documented in [screens.md](screens.md). The active MVP scope is:

- Main Dashboard;
- Search / Command Palette;
- Item Detail;
- Planning placeholder;
- Week View;
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
- Today, Week, and Backlog collections;
- Forgotten review;
- Week view;
- monthly JSON files;
- local JSON index;
- export/import;
- readable public refs.

Import is for restoring or moving existing TermBullet JSON files into a new
installation. It is not a merge feature and must not import over an active local
data set.

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
6. basic export and import work;
7. architecture is ready for optional AI, calendar, and sync;
8. documentation is English-first and open-source ready.
