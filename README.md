# TermBullet

TermBullet is a personal productivity application for the terminal, inspired by the Bullet Journal philosophy and designed especially for developers and users who live in the shell.

The project combines two first-class interfaces over the same functional core:

- **TUI**: the main visual interface, with screens and panels inspired by tools such as LazyDocker and LazyGit.
- **CLI**: a fast interface for capturing, querying, and manipulating items without opening the TUI.

> Status: this repository currently contains the product specification and architecture decisions. The commands and screens below represent the target product interface.

## Open Source

TermBullet is intended to be an open source project for a global audience.

Documentation, command names, examples, and user-facing text should be English-first. License, contribution guidelines, and governance details should be added before the first public release.

## Objective

Allow users to organize tasks, notes, events, and personal reviews directly from the terminal with a fast, predictable, local-first workflow.

V1 must work fully offline, without depending on internet access, online accounts, AI, Google Calendar, or cloud services. Future integrations must remain optional and must not break the local-first foundation.

## Principles

- **Local-first**: the local database is the user's primary operational source.
- **Terminal-first**: optimized for keyboard, shell, and frequent terminal usage.
- **CLI + TUI**: both interfaces must reuse the same use cases.
- **Optional AI**: planning assistance, never a product dependency.
- **Optional integrations**: calendar, sync, and cloud are extensions.
- **Evolutionary architecture**: V1 must prepare the ground for V2, V3, and V4.
- **Open source by design**: documentation and structure should support external contributors.

## V1 Scope

The first version of TermBullet should deliver the offline core:

- creation and manipulation of tasks, notes, and events;
- Today, Week, and Backlog collections;
- main TUI with keyboard navigation;
- CLI for capture and fast operations;
- search;
- basic editing;
- item migration and movement;
- local configuration;
- basic export and import.

V1 does not include AI, Google Calendar, machine sync, or cloud accounts.

## Item Types

The system must support three main item types:

- **Task**: an executable item or pending work.
- **Note**: a record, context, or observation.
- **Event**: an appointment or internal time-based marker.

Each item must record at least:

- internal global ID;
- public ref;
- type;
- content;
- status;
- creation and update dates;
- current collection;
- priority;
- tags.

## Public Refs

TermBullet must not expose only simple numeric IDs as the main user-facing reference.

Each entity has:

- **Internal global ID**: technical, stable, unique, and prepared for future sync.
- **Public Ref**: short, readable, and used in the CLI/TUI.

Official format:

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
t-0422-2
n-0422-1
e-0422-1
```

Rules:

- the sequence is independent by type and day;
- the public ref must be persisted;
- the public ref must not be reused;
- the internal ID remains the real entity identity.

## CLI Interface

When no command is provided, TermBullet should open the main TUI:

```bash
termbullet
```

Planned main commands:

```text
termbullet
├── tui
├── add
├── list
├── today
├── week
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
├── export
├── import
└── config
    ├── list
    ├── get
    ├── set
    └── path
```

Target usage examples:

```bash
termbullet add "fix jwt authentication"
termbullet add --note "error happens when audience is empty"
termbullet today
termbullet done t-0422-1
termbullet show t-0422-1
termbullet search "jwt"
```

Planned global options:

- `-h`, `--help`: show help.
- `-v`, `--version`: show version.
- `--json`: JSON output when supported.
- `--no-color`: disable colors.
- `--db <path>`: use an alternative local database.
- `--profile <name>`: use a specific configuration profile.

## TUI Interface

The TUI should work as a personal cockpit for planning and execution:

- terminal-first and keyboard-first;
- inspired by LazyDocker/LazyGit;
- dense, clear, and operational;
- based on screens, panels, visual focus, and consistent shortcuts.

Required V1 screens:

- Main Dashboard;
- Daily Focus;
- Weekly Planning;
- Backlog Triage;
- Review;
- Search / Command Palette;
- Config.

Future screens:

- Calendar View in V3;
- Sync / Cloud in V4.

Expected cross-screen behaviors:

- `Tab` and `Shift+Tab` to move focus between panels;
- `Enter` to expand or open the active panel;
- `Esc` to go back;
- `/` for contextual filtering;
- `c` for quick capture;
- `?` for compact contextual help.

## Architecture

The project must keep clear separation between domain, use cases, and interfaces.

Recommended layers:

- **Core**: entities, business rules, states, identification policies, and migration policies.
- **Application**: use cases, orchestration services, and input/output contracts.
- **Infrastructure**: local persistence, import/export, AI, calendar, and future sync.
- **CLI**: commands, handlers, and text rendering.
- **TUI**: screens, navigation, shortcuts, panels, and visual context.

Essential rule:

> CLI and TUI must reuse the same Application layer use cases.

## Roadmap

### V1 - Offline Core

- TUI;
- CLI;
- tasks, notes, and events;
- today, week, and backlog;
- local persistence;
- export/import;
- human-readable identification.

### V2 - AI Planning

- AI configuration module;
- BYOK model;
- daily planning;
- daily review;
- task breakdown;
- backlog prioritization;
- preview before persisting suggestions.

### V3 - Google Calendar

- reading daily events;
- dashboard display;
- schedule context for AI;
- event creation from TermBullet.

### V4 - Sync + Cloud

- synchronization between machines;
- optional cloud;
- conflict resolution;
- sync history;
- preservation of the local-first philosophy.

## V1 Acceptance Criteria

V1 is considered adequate when:

1. users can use TermBullet locally without internet access;
2. the TUI offers consistent screen and panel navigation;
3. the CLI can manipulate the main items without opening the TUI;
4. tasks, notes, and events can be created, listed, edited, and changed;
5. the system uses readable public refs in the official format;
6. basic export and import are available;
7. the architecture is ready for future AI, calendar, and sync;
8. documentation is English-first and ready for open source publication.

## Documentation

- [Product specification](product-spec.md)
- [Architecture Decision Records](ADR.md)
