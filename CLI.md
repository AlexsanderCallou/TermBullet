# TermBullet CLI

The CLI is a first-class interface for capture, lookup, and manipulation without
opening the TUI. It must use System.CommandLine and call Application use cases.

When no command is provided, TermBullet opens the TUI.

```bash
termbullet
```

## Help Shape

```text
TermBullet - Local-First Terminal Planner

Usage:
  termbullet [command] [arguments] [options]

If no command is provided, the main TUI is opened.
```

## Command Tree

```text
termbullet
├── tui
├── add
├── list
├── today
├── week
├── backlog
├── forgotten
├── show
├── edit
├── done
├── cancel
├── migrate
├── move
├── delete
├── tag
├── untag
├── priority
├── search
├── history
│   └── clear
├── export
├── import
└── path
```

## Global Options

- `-h`, `--help`: show help.
- `-v`, `--version`: show version.

## Core Commands

### `add`

Create an item.

```bash
termbullet add "fix jwt authentication"
termbullet add "error happens when audience is empty" --note
termbullet add "review 16:00" --event
```

Type flags are mutually exclusive:

- `--task`
- `--note`
- `--event`

Default type is task.

### `list`

List items, with filters where supported.

```bash
termbullet list
```

### Views and Collection Shortcuts

```bash
termbullet today
termbullet week
termbullet backlog
termbullet forgotten
```

`week` is a planning view derived from task `planned_for` dates. It is not a
persisted collection and items are not moved to `week`.

### `show`

Show one item by public ref.

```bash
termbullet show t-0426-1
```

### `edit`

Edit item content and optional description when supported.

```bash
termbullet edit t-0426-1 "fix auth flow"
```

### State Changes

```bash
termbullet done t-0426-1
termbullet cancel t-0426-1
termbullet migrate t-0426-1 --date 2026-05-12
termbullet migrate t-0426-1 --backlog
```

`migrate` applies to tasks and must receive exactly one destination:

- `--date <yyyy-mm-dd>` migrates the task to a specific planned date.
- `--backlog` migrates the task to Backlog.

Open tasks that were planned for previous days and were not done, cancelled, or
marked migrate are shown in `forgotten` for manual review.

### Movement

```bash
termbullet move t-0426-1 today
termbullet move t-0426-1 backlog
```

### Tags and Priority

```bash
termbullet tag t-0426-1 auth
termbullet untag t-0426-1 auth
termbullet priority t-0426-1 high
```

Priorities:

- `none`
- `low`
- `medium`
- `high`

### `search`

Search items in local data.

```bash
termbullet search "jwt"
```

### `delete`

Remove an active item and append a `deleted` history event with a snapshot.

```bash
termbullet delete t-0426-1
```

### `history clear`

Clear stored history entries, not active items.

```bash
termbullet history clear
```

### Export and Import

```bash
termbullet export
termbullet import <path>
```

Export/import must preserve IDs, public refs, types, status, collections,
planned dates, priorities, tags, timestamps, versions, migration metadata, and
important history.

Import is a restore/migration command for a new installation. It must fail if
the local data directory already contains monthly JSON files. It does not merge,
skip conflicts, or overwrite an active local data set.

### `path`

```bash
termbullet path
```

Show the active local data directory where TermBullet stores JSON files.

V1 only supports data path reporting/selection. There are no user-editable
product keys.

## CLI Rules

- Keep command names and options aligned with this file.
- Keep help and output English-first.
- Keep behavior consistent with equivalent TUI actions.
- Do not implement business rules in command handlers.
- Verify parsing and representative help output when CLI behavior changes.
