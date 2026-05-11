# TermBullet Release Notes

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
