# TermBullet Release Notes

## v1.1.1 - Readable JSON Storage

TermBullet V1.1.1 keeps the first-run data directory setup and improves local
file readability.

### Added

- README uninstall instructions for safely removing the executable while
  preserving local data.

### Changed

- Monthly JSON files are now written with indentation.
- The local `index.json` file is now written with indentation.

### Release Assets

Expected assets:

```text
termbullet_1.1.1_windows_x64.zip
termbullet_1.1.1_linux_x64.tar.gz
termbullet_1.1.1_checksums.txt
```

Publishing `v1.1.1` as the latest GitHub release makes the documented install
commands resolve to these assets.

## v1.1.0 - First-Run Data Directory Setup

TermBullet V1.1 keeps the offline core and adds first-run data directory setup.

### Added

- First-run prompt for choosing the local data root.
- Install-directory `conf.json` with the selected `data_root`.
- Startup validation for read/write permissions in the selected data directory.
- Clear startup error when the install directory cannot write `conf.json`.
- `termbullet path` output for config, data root, and operational data paths.

### Changed

- Operational data no longer depends on the current working directory.
- Monthly JSON, tags, and index files now live under `<data_root>/data`.
- Windows release publishing can be run with `publish.ps1`.

### Release Assets

Expected assets:

```text
termbullet_1.1.0_windows_x64.zip
termbullet_1.1.0_linux_x64.tar.gz
termbullet_1.1.0_checksums.txt
```

Publishing `v1.1.0` as the latest GitHub release makes the documented install
commands resolve to these assets.

## v1.0.0 - V1 Offline Core

TermBullet V1 is the first offline core release. It provides a local-first
terminal planner for tasks, notes, events, and review workflows.

### Included

- Keyboard-first TUI with Main Dashboard, Search, Item Detail, Add Item, Migrate
  Item, Planning placeholder, Week, Month, Backlog, Forgotten, Notes, Calendar,
  and Tags screens.
- CLI for capture, listing, item detail, search, status changes, editing,
  priority, tags, movement, migration, data path discovery, and history clear.
- Task, note, and event item model with public refs such as `t-0526-1`,
  `n-0526-1`, and `e-0526-1`.
- Task collections: Today, Week, Month, and Backlog.
- Forgotten review as a derived view for unresolved tasks from previous monthly
  files.
- Events with `scheduled_at`.
- Local monthly JSON persistence, safe writes, one backup per monthly file,
  backup recovery, and local JSON index.
- Windows x64 and Linux x64 install scripts that resolve the latest GitHub
  release and verify SHA256 checksums.

### Not Included

- AI execution.
- Google Calendar integration.
- Machine sync or cloud accounts.
- PostgreSQL runtime dependency.
- Export/import commands.

### Release Assets

Expected assets:

```text
termbullet_1.0.0_windows_x64.zip
termbullet_1.0.0_linux_x64.tar.gz
termbullet_1.0.0_checksums.txt
```

The install scripts read the latest GitHub release by default, so publishing
`v1.0.0` as the latest release makes the documented install commands resolve to
these assets.
