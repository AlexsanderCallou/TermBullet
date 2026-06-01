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
├── add
├── list
├── today
├── week
├── month
├── backlog
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
└── path
```

## Global Options

- `-h`, `--help`: show help.
- `-v`, `--version`: show version.

## Core Commands

### TUI

Open the TUI by running TermBullet without a command:

```bash
termbullet
```

There is no separate `tui` command in V1. The shortest command opens the main
terminal interface.

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
termbullet month
termbullet backlog
```

`week` and `month` show tasks in their respective collections. They are not
date-grouped task schedules.

Forgotten is currently exposed in the TUI as a derived review list. There is no
active `forgotten` CLI command in the current command tree.

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
termbullet migrate t-0426-1 --collection week
termbullet migrate t-0426-1 --collection backlog
```

`migrate` applies to tasks and must receive a destination collection:

- `--collection today`
- `--collection week`
- `--collection month`
- `--collection backlog`

Open tasks from previous monthly files that were not done or cancelled are
shown in the TUI Forgotten review for manual action.

### Movement

```bash
termbullet move t-0426-1 today
termbullet move t-0426-1 backlog
```

### Tags and Task Priority

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

Priority is task metadata. Notes and events are stored with `none`.

### `search`

Search items in local data.

```bash
termbullet search "jwt"
```

Search may read across all monthly JSON files. It is a lookup surface and does
not change item state.

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

### `path`

```bash
termbullet path
```

Show the active local config and data paths.

Example output:

```text
config: C:\Users\Alexsander\AppData\Local\TermBullet\bin\conf.json
data_root: C:\Users\Alexsander\Documents\TermBullet
data: C:\Users\Alexsander\Documents\TermBullet\data
```

On first execution, TermBullet asks for the base data directory, validates
read/write permissions, and saves the selection in `<install-dir>/conf.json`.
There are no user-editable product keys.

## CLI Rules

- Keep command names and options aligned with this file.
- Keep help and output English-first.
- Keep behavior consistent with equivalent TUI actions.
- Do not implement business rules in command handlers.
- Verify parsing and representative help output when CLI behavior changes.
