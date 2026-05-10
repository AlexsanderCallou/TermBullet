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
- `backlog`

Additional collections may exist for product flow:

- `monthly`
- `archived`

Review, Forgotten, and Search are screens/features, not item collections.
Forgotten is a derived review list for open tasks whose `planned_for` date is
before today. Week is the V1 dated week collection; the TUI Week View groups
week items by task `planned_for` and event `scheduled_at`.

## Identity

Internal ID:

- UUID string;
- generated once;
- immutable;
- real identity for persistence, import/export, and future sync;
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
- `planned_for`
- `priority`
- `tags`
- `version`
- `created_at`
- `updated_at`

`planned_for` is required for active tasks in Today or Week. Backlog tasks may
store it as `null`. Notes and events may store it as `null`.

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
      "planned_for": "2026-04-22",
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

## Task Planning and Forgotten Review

Tasks have a `planned_for` date.

Rules:

- tasks created from Today use today's date as `planned_for`;
- future dates are only set when the user intentionally plans a task for the
  future;
- Backlog tasks keep `planned_for` as `null`;
- an open task with `planned_for` before today and no terminal action appears
  in Forgotten review;
- Forgotten tasks wait for explicit user action.

At startup or at the beginning of the day, the application should check open
tasks planned before today. If a task was not done, cancelled, or marked
migrate on its planned day, it appears in Forgotten review.

Recommended forgotten history event:

```json
{
  "type": "forgotten",
  "item_id": "0f3a9d94-4df0-47f7-95c1-0f967c22f4db",
  "from_collection": "today",
  "review": "forgotten",
  "planned_for": "2026-04-22",
  "created_at": "2026-04-23T00:05:00Z"
}
```

## Manual Migration

Manual migration is an intentional user action for tasks. It must always declare
the destination:

- specific date;
- Backlog.

Rules:

- migrating to a date marks the source task as `migrate` and creates a new
  `open` task with the destination planned date;
- migrating to Backlog marks the source task as `migrate` and creates a new
  `open` task in Backlog without an active date;
- the destination task receives a new internal ID and public ref;
- the destination task records `migrated_from_id` and `migrated_from_ref`;
- the source task records `migrated_to_id` and `migrated_to_ref`;
- migration history records the relationship between source and destination;
- migration details are represented in `migration` and/or history.

Recommended migration object:

```json
{
  "from_collection": "today",
  "from_planned_for": "2026-04-22",
  "from_id": "0f3a9d94-4df0-47f7-95c1-0f967c22f4db",
  "from_ref": "t-0426-1",
  "to_collection": "today",
  "to_planned_for": "2026-04-23",
  "to_id": "a0f13256-499f-47bc-a623-6fa8f4df36f8",
  "to_ref": "t-0426-4",
  "migrated_at": "2026-04-22T20:15:00Z",
  "reason": "manual_date"
}
```

## Export, Import, AI, and Sync

Export/import must preserve IDs, refs, type, content, description, status,
collection, planned dates, task priority, tags, timestamps, version, migration
metadata, tag catalog entries, and important history.

Import is intended for restoring or moving TermBullet JSON files into a new
installation. It must only run when the local data directory has no existing
monthly JSON files. If local monthly JSON files already exist, import must fail
before writing anything.

Import behavior:

- validate the provided data first;
- reject malformed JSON;
- reject duplicate public refs inside a period;
- reject duplicate internal IDs;
- reject missing required fields;
- reject import into a non-empty local data set;
- write the imported monthly files as the new local data set.

Import does not merge, skip conflicting records, or overwrite an existing active
local data set. Users who want to replace an installation must clear or move the
existing local data directory first.

Future AI context must be filtered. Do not send all JSON files by default.

V4 sync/cloud synchronizes whole monthly JSON files. Planned simple conflict
rule: latest update wins. PostgreSQL stores the same JSON file content and does
not replace the local operational store.
