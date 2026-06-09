# TermBullet Backlog

This file tracks the current execution focus. Historical implementation details
belong in release notes and git history.

## Current Status

The backlog is intentionally empty after the V2 publication.

TermBullet currently has:

- the offline local-first core;
- CLI and TUI over shared Application use cases;
- monthly JSON persistence, tag catalog, local index, and item history;
- task, note, and event workflows;
- Today, Week, Month, Backlog, Forgotten, Daily Review, Notes, Calendar, Tags,
  Item Detail, Search, Add Item, Edit Item, and Migrate Item surfaces;
- optional AI planning through `<data_root>/.aiconf`;
- structured AI drafts that require explicit approval before persistence;
- the canonical planning agent installed beside the executable.

## Next Planning Area

The next cycle is the first real refactoring cycle. Before adding new product
features, choose one refactoring target and describe it here with:

- the concrete pain;
- the module boundaries involved;
- the expected user-visible preservation;
- the tests that must protect the behavior.

## Future Ideas

Future ideas stay outside the active backlog until they are selected for a
cycle:

- broader AI planning over existing open work;
- Google Calendar integration;
- sync and cloud;
- package manager distribution;
- release automation.
