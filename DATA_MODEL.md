# TermBullet - Data Model

This document defines the initial V1 data model for TermBullet.

V1 uses SQLite as the local offline database. PostgreSQL is reserved for the future V4 sync/cloud backend and must not be required for local usage.

## Data Principles

- Local data is the operational source of truth in V1.
- Every item has a stable internal ID and a human-facing public ref.
- Public refs are persisted and never reused.
- Timestamps are stored in UTC.
- The schema must be migration-friendly.
- The model must prepare for future entity-level sync without implementing sync in V1.

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
- used for persistence, import/export, and future sync.

## Public Ref

Public ref format:

```text
<type>-<MMDD>-<sequence>
```

Prefixes:

- `t` = task
- `n` = note
- `e` = event

Examples:

```text
t-0422-1
n-0422-1
e-0422-1
```

Rules:

- sequence is independent by type and MMDD key;
- sequence must be persisted;
- public ref must never be reused;
- public ref is not the real identity.

Because the public ref does not include a year, the sequence for a given type/MMDD must continue across years instead of restarting if that would create a duplicate.

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
- `migrated` records movement to another collection or planning period;
- deletion should prefer `deleted_at` for recoverability unless a hard delete is explicitly required.

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

## Timestamps

Store timestamps as UTC ISO-8601 text:

```text
2026-04-22T12:34:56Z
```

Required timestamps:

- `created_at`
- `updated_at`

Optional timestamps:

- `due_at`
- `scheduled_at`
- `completed_at`
- `cancelled_at`
- `migrated_at`
- `deleted_at`

## SQLite Schema - Initial Draft

### `schema_migrations`

Tracks applied database migrations.

```sql
CREATE TABLE schema_migrations (
    version INTEGER PRIMARY KEY,
    name TEXT NOT NULL,
    applied_at TEXT NOT NULL
);
```

### `public_ref_sequences`

Stores the next sequence for each item type and MMDD key.

```sql
CREATE TABLE public_ref_sequences (
    item_type TEXT NOT NULL,
    mmdd TEXT NOT NULL,
    next_sequence INTEGER NOT NULL,
    updated_at TEXT NOT NULL,
    PRIMARY KEY (item_type, mmdd)
);
```

### `items`

Stores tasks, notes, and events.

```sql
CREATE TABLE items (
    id TEXT PRIMARY KEY,
    public_ref TEXT NOT NULL UNIQUE,
    type TEXT NOT NULL CHECK (type IN ('task', 'note', 'event')),
    content TEXT NOT NULL,
    description TEXT NULL,
    status TEXT NOT NULL CHECK (status IN ('open', 'in_progress', 'done', 'cancelled', 'migrated')),
    collection TEXT NOT NULL CHECK (collection IN ('today', 'week', 'backlog', 'monthly', 'archived')),
    priority TEXT NOT NULL CHECK (priority IN ('none', 'low', 'medium', 'high')),
    due_at TEXT NULL,
    scheduled_at TEXT NULL,
    estimate_minutes INTEGER NULL,
    version INTEGER NOT NULL DEFAULT 1,
    created_at TEXT NOT NULL,
    updated_at TEXT NOT NULL,
    completed_at TEXT NULL,
    cancelled_at TEXT NULL,
    migrated_at TEXT NULL,
    deleted_at TEXT NULL
);
```

Recommended indexes:

```sql
CREATE INDEX idx_items_type ON items(type);
CREATE INDEX idx_items_status ON items(status);
CREATE INDEX idx_items_collection ON items(collection);
CREATE INDEX idx_items_priority ON items(priority);
CREATE INDEX idx_items_updated_at ON items(updated_at);
CREATE INDEX idx_items_deleted_at ON items(deleted_at);
```

### `tags`

Stores unique tag names.

```sql
CREATE TABLE tags (
    id TEXT PRIMARY KEY,
    name TEXT NOT NULL UNIQUE,
    created_at TEXT NOT NULL
);
```

### `item_tags`

Maps items to tags.

```sql
CREATE TABLE item_tags (
    item_id TEXT NOT NULL,
    tag_id TEXT NOT NULL,
    created_at TEXT NOT NULL,
    PRIMARY KEY (item_id, tag_id),
    FOREIGN KEY (item_id) REFERENCES items(id),
    FOREIGN KEY (tag_id) REFERENCES tags(id)
);
```

Recommended indexes:

```sql
CREATE INDEX idx_item_tags_tag_id ON item_tags(tag_id);
```

### `item_history`

Records relevant state changes for preview, review, and future sync reasoning.

```sql
CREATE TABLE item_history (
    id TEXT PRIMARY KEY,
    item_id TEXT NOT NULL,
    event_type TEXT NOT NULL,
    event_data TEXT NULL,
    created_at TEXT NOT NULL,
    FOREIGN KEY (item_id) REFERENCES items(id)
);
```

Examples of `event_type`:

```text
created
updated
status_changed
collection_changed
priority_changed
tag_added
tag_removed
migrated
deleted
```

### `app_settings`

Stores local application settings.

```sql
CREATE TABLE app_settings (
    key TEXT PRIMARY KEY,
    value TEXT NOT NULL,
    updated_at TEXT NOT NULL
);
```

## Import and Export Contract

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
- item history when supported.

Import must handle:

- empty database import;
- existing database import;
- duplicate public refs;
- duplicate internal IDs;
- malformed files;
- unsupported schema versions.

## Sync Preparation

V1 does not implement sync, but the schema prepares for it through:

- stable internal IDs;
- `version` on items;
- UTC timestamps;
- `deleted_at` for tombstone-like behavior;
- item history for change reasoning.

Future V4 sync may add:

- device identity;
- operation log;
- remote revision;
- conflict records;
- sync state.

These should be added later through migrations, not required in V1.
