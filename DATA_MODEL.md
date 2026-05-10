# TermBullet Data Model

V1 uses monthly local JSON files as the operational data store. PostgreSQL is
reserved for the optional V4 sync/cloud backend and must not be required for
local usage.

## Principles

- Local JSON files are the source of truth in V1.
- V1 assumes one active machine at a time.
- Every item has an internal ID and a human-facing public ref.
- Public refs are persisted and never reused in the same period.
- Timestamps are UTC ISO-8601 text.
- JSON should remain human-readable.
- Writes use temp file plus atomic replacement.
- Each monthly file keeps one backup.
- Corrupted monthly files should recover from backup when possible.
- The model prepares for future whole-file sync.

## File Layout

Data files:

```text
data/<year>/data_<month>_<year>.json
```

Example:

```text
data/2026/data_04_2026.json
```

Backup files:

```text
data/<year>/data_<month>_<year>.backup.json
```

Only one backup per monthly file is kept.

Tag catalog files:

```text
data/tags.json
data/tags.backup.json
```

The tag catalog is global local metadata. Item `tags` arrays remain the source
of item-to-tag assignment, while `data/tags.json` stores optional tag
descriptions and allows tags to exist before any item uses them.

The local index is derived data and can be rebuilt:

```text
data/index.json
```

The index may include ID, public ref, type, status, collection, task priority,
tags, content summary, source file, and timestamps.

## Item Concepts

Types:

- `task`
- `note`
- `event`

Minimum V1 collections:

- `today`
- `week`
- `month`
- `backlog`

Review, Forgotten, and Search are screens/features, not item collections.
Forgotten is a derived review list for open tasks from previous monthly files
that still have no terminal status. Week and Month are task collections, not
dated task schedules. Events use `scheduled_at`.

Current operational queries read the current monthly file. Archive/review
queries may read all monthly files explicitly. Forgotten uses archive review
semantics to find old open tasks, while Today, Week, Month, and Calendar stay
bounded to the current month unless a future period view is explicitly added.

## Identity

Internal ID:

- UUID string;
- generated once;
- immutable;
- real identity for persistence and future sync;
- preserved on the source item after migration.

Public ref:

```text
<type>-<MMYY>-<sequence>
```

Prefixes:

- `t` task
- `n` note
- `e` event

Rules:

- sequence is independent by type and month/year;
- sequence is controlled inside the monthly file;
- public ref is persisted and not reused;
- migration source items preserve the original public ref;
- migration destination items receive a new public ref and record the source ref.

## Item Fields

Required persisted fields:

- `id`
- `public_ref`
- `type`
- `content`
- `description`
- `status`
- `collection`
- `priority`
- `tags`
- `version`
- `created_at`
- `updated_at`

Tasks do not persist a planning date. Task placement is expressed through
`collection`. Dates belong to events through `scheduled_at`.

Optional fields:

- `scheduled_at`
- `completed_at`
- `cancelled_at`
- `migrated_at`
- `migration`
- `migrated_from_id`
- `migrated_from_ref`
- `migrated_to_id`
- `migrated_to_ref`

Status values:

- `open`
- `done`
- `cancelled`
- `migrate`

`migrate` means the task was intentionally moved out of its previous planned
placement. The destination remains executable as an `open` task.

Priority is task metadata. Notes and events store `none` and do not expose
priority in creation flows.

Priority values:

- `none`
- `low`
- `medium`
- `high`

Default task priority is `none`.

## Tag Catalog

Tags are named metadata labels used by tasks, notes, and events.

Rules:

- tag names are normalized to lowercase trimmed text;
- names are unique case-insensitively;
- descriptions are optional;
- creating a catalog tag does not mutate existing items;
- usage counts are derived from item `tags` arrays.

Tag catalog shape:

```json
{
  "tags": [
    {
      "name": "auth",
      "description": "authentication and authorization work",
      "created_at": "2026-05-09T12:00:00Z",
      "updated_at": "2026-05-09T12:00:00Z"
    }
  ]
}
```

## Monthly JSON Shape

```json
{
  "period": "2026-04",
  "file_name": "data_04_2026.json",
  "public_ref_sequences": {
    "task": 3,
    "note": 2,
    "event": 1
  },
  "items": [
    {
      "id": "0f3a9d94-4df0-47f7-95c1-0f967c22f4db",
      "public_ref": "t-0426-1",
      "type": "task",
      "content": "fix jwt authentication",
      "description": null,
      "status": "open",
      "collection": "today",
      "priority": "high",
      "tags": ["jwt", "auth"],
      "scheduled_at": null,
      "version": 1,
      "created_at": "2026-04-22T08:14:00Z",
      "updated_at": "2026-04-22T08:14:00Z",
      "completed_at": null,
      "cancelled_at": null,
      "migrated_at": null,
      "migration": null
    }
  ],
  "history": []
}
```

No per-file schema version is required in V1. Monthly JSON files do not store
user-editable product options.

## History

History is stored in root-level `history`.

Important event types:

- `created`
- `edited`
- `done`
- `cancelled`
- `migrate`
- `forgotten`
- `deleted`

Delete behavior:

- physically remove the item from active `items`;
- append a `deleted` history event;
- include a snapshot of the deleted item.

History cleanup removes history entries, not active items, and must create a
backup before writing.

## Task Collections and Forgotten Review

Tasks are planned by collection.

Rules:

- tasks created from Quick Task use the `today` collection;
- normal task creation must choose one of `today`, `week`, `month`, or `backlog`;
- dates must not be stored on tasks;
- an open task from a previous monthly file with no terminal action appears in
  Forgotten review;
- Forgotten tasks wait for explicit user action.

At startup or at the beginning of a new month, the application should keep old
open tasks in their original monthly files and expose them through Forgotten for
explicit review.

Month rollover is maintenance-only in V1. On the first day of a month it ensures
the current monthly file exists and refreshes the local index. It must not move,
copy, mark, or automatically migrate old tasks. Old open tasks remain in their
original monthly files and are surfaced through Forgotten for explicit human
action.

Recommended forgotten history event:

```json
{
  "type": "forgotten",
  "item_id": "0f3a9d94-4df0-47f7-95c1-0f967c22f4db",
  "from_collection": "today",
  "review": "forgotten",
  "created_at": "2026-04-23T00:05:00Z"
}
```

## Manual Migration

Manual migration is an intentional user action for tasks. It must always declare
the destination collection: `today`, `week`, `month`, or `backlog`.

Rules:

- migrating marks the source task as `migrate` and creates a new `open` task in
  the destination collection;
- the destination task receives a new internal ID and public ref;
- the destination task records `migrated_from_id` and `migrated_from_ref`;
- the source task records `migrated_to_id` and `migrated_to_ref`;
- migration history records the relationship between source and destination;
- migration details are represented in `migration` and/or history.

Recommended migration object:

```json
{
  "from_collection": "today",
  "from_id": "0f3a9d94-4df0-47f7-95c1-0f967c22f4db",
  "from_ref": "t-0426-1",
  "to_collection": "week",
  "to_id": "a0f13256-499f-47bc-a623-6fa8f4df36f8",
  "to_ref": "t-0426-4",
  "migrated_at": "2026-04-22T20:15:00Z",
  "reason": "manual_collection"
}
```

## AI and Sync

Future AI context must be filtered. Do not send all JSON files by default.

V4 sync/cloud synchronizes whole monthly JSON files. Planned simple conflict
rule: latest update wins. PostgreSQL stores the same JSON file content and does
not replace the local operational store.
