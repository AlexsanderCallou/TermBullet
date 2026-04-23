# TermBullet - Data Model

This document defines the initial V1 data model for TermBullet.

V1 uses local monthly JSON files as the operational data store. PostgreSQL is reserved for the future V4 sync/cloud backend and must not be required for local usage.

## Data Principles

- Local JSON files are the operational source of truth in V1.
- The app is designed for one active machine at a time in V1.
- Every item has a stable internal ID and a human-facing public ref.
- Public refs are persisted and never reused.
- Timestamps are stored in UTC.
- JSON files should be optimized for human readability.
- Writes must be safe: write to a temporary file and then replace atomically.
- Each monthly file keeps one backup file.
- Corrupted monthly JSON files should be recovered from the latest backup when possible.
- The model must prepare for future file-level sync without implementing multi-machine sync in V1.

## File Layout

Data files are separated by year.

Required path pattern:

```text
data/<year>/data_<month>_<year>.json
```

Example:

```text
data/2026/data_04_2026.json
data/2026/data_05_2026.json
```

Only this file naming pattern is supported.

Monthly backup file pattern:

```text
data/<year>/data_<month>_<year>.backup.json
```

Example:

```text
data/2026/data_04_2026.backup.json
```

Only one backup per monthly file is kept.

## Local Index

The app should maintain a local JSON index for faster lookup.

Recommended path:

```text
data/index.json
```

The index is derived data and can be rebuilt from monthly files.

The index may include:

- item ID;
- public ref;
- type;
- status;
- collection;
- priority;
- tags;
- content summary;
- source file;
- created/updated timestamps.

Simple views should use the current monthly file. More complex searches may read all monthly files when the index is insufficient.

## Main Concepts

TermBullet starts with three item types:

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

Review, Search, and Config are screens or features, not item collections.

## Internal ID

Each persisted entity must have an internal global ID.

Recommended V1 format:

```text
UUID string
```

Rules:

- generated once;
- immutable;
- used as the real identity;
- used for import/export and future sync;
- preserved when an item is migrated between monthly files.

## Public Ref

Public ref format:

```text
<type>-<MMYY>-<sequence>
```

Prefixes:

- `t` = task
- `n` = note
- `e` = event

Examples for April 2026:

```text
t-0426-1
n-0426-1
e-0426-1
```

Rules:

- sequence is independent by type and month/year;
- sequence is controlled inside the monthly file;
- public ref must be persisted;
- public ref must never be reused inside the same month/year;
- public ref is not the real identity;
- migrated items preserve their original public ref.

The `MMYY` segment avoids collisions across years while keeping refs short.

## Item Status

Initial status values:

```text
open
in_progress
done
cancelled
migrated
```

Rules:

- new tasks and notes usually start as `open`;
- events may start as `open`;
- `done` records completion;
- `cancelled` records intentional cancellation;
- `migrated` records movement to another monthly file or planning period.

## Priority

Initial priority values:

```text
none
low
medium
high
```

Default:

```text
none
```

## Timestamps and Version

Store timestamps as UTC ISO-8601 text:

```text
2026-04-22T12:34:56Z
```

Required per item:

- `created_at`
- `updated_at`
- `version`

Optional per item:

- `due_at`
- `scheduled_at`
- `completed_at`
- `cancelled_at`
- `migrated_at`

The `version` field is incremented on item changes and prepares V4 merge behavior.

## Monthly JSON Structure

Recommended structure:

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
      "due_at": null,
      "scheduled_at": null,
      "estimate_minutes": null,
      "version": 1,
      "created_at": "2026-04-22T08:14:00Z",
      "updated_at": "2026-04-22T08:14:00Z",
      "completed_at": null,
      "cancelled_at": null,
      "migrated_at": null,
      "migration": null
    }
  ],
  "history": [
    {
      "id": "7d5b9856-045f-43ef-a646-4ee9c86fe2d8",
      "item_id": "0f3a9d94-4df0-47f7-95c1-0f967c22f4db",
      "public_ref": "t-0426-1",
      "event_type": "created",
      "occurred_at": "2026-04-22T08:14:00Z",
      "data": {
        "content": "fix jwt authentication"
      }
    }
  ],
  "settings": {}
}
```

No per-file schema version is required in V1. The project assumes a single schema.

## History

History is stored in a root-level `history` array inside the monthly JSON file.

Only important events are stored:

- `created`
- `edited`
- `done`
- `cancelled`
- `migrated`
- `deleted`

Delete behavior:

- remove the item physically from the `items` array;
- append a `deleted` event to `history`;
- include a snapshot of the deleted item in the history event data.

History cleanup:

- users may clear history through a command;
- cleanup removes history entries, not active items;
- cleanup must create a backup before writing.

## Migration Between Months

Open tasks from the previous month should be migrated automatically on the first day of the next month.

Rules:

- the active item moves to the destination monthly file;
- the original public ref is preserved;
- the original internal ID is preserved;
- the source monthly file keeps a history event indicating migration;
- the destination monthly file stores the active item;
- the source item record should not remain active in `items`;
- migration details are represented in the `migration` field and/or root-level history.

Recommended migration object on the active item:

```json
{
  "from_period": "2026-04",
  "to_period": "2026-05",
  "migrated_at": "2026-05-01T00:05:00Z",
  "reason": "automatic_month_rollover"
}
```

## AI Context Preparation

Future AI features must not send all JSON files by default.

The app should assemble a filtered context including only relevant data, such as:

- current month;
- selected item;
- related tags;
- recent history;
- relevant backlog items.

If possible, sensitive or private fields should be excluded from AI context unless explicitly included by the user.

## Import and Export Contract

Monthly JSON files are already portable, but export/import commands still exist for controlled backup and migration flows.

Exported data must preserve:

- internal IDs;
- public refs;
- item type;
- content and description;
- status;
- collection;
- priority;
- tags;
- timestamps;
- version;
- migration metadata;
- important history.

Import must handle:

- valid monthly JSON files;
- malformed JSON;
- duplicate public refs inside a period;
- duplicate internal IDs;
- missing required fields;
- corrupted file with available backup.

## Sync Preparation

V1 does not implement multi-machine sync.

V1 rule:

```text
Use one active machine at a time.
```

Future V4 sync/cloud will synchronize whole JSON files.

Conflict rule planned for V4:

```text
Latest update wins.
```

PostgreSQL remains part of V4 as the cloud/backend database, but the server stores the same JSON file content rather than transforming it into a different entity model.
