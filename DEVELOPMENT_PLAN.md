# TermBullet - V1 Development Plan

This document defines the incremental implementation plan for TermBullet V1.

TermBullet follows TDD. Each milestone starts with tests, uses mocked good and bad data where appropriate, and is complete only when all relevant tests pass.

## V1 Goal

Deliver a local-first, offline terminal productivity tool with:

- CLI;
- TUI;
- tasks, notes, and events;
- Today, Week, and Backlog;
- SQLite persistence;
- search;
- basic editing;
- migration and movement;
- local configuration;
- basic export and import.

## Standard Verification Commands

Expected commands once the solution exists:

```bash
dotnet restore
dotnet build
dotnet test
```

Expected local run command:

```bash
dotnet run --project src/TermBullet -- [command] [arguments] [options]
```

Examples:

```bash
dotnet run --project src/TermBullet -- add "fix jwt authentication"
dotnet run --project src/TermBullet -- today
dotnet run --project src/TermBullet -- done t-0422-1
```

## Milestone 0 - Repository Scaffold

### Goal

Create the .NET solution structure for the modular monolith.

### Deliverables

- `TermBullet.sln`
- `src/TermBullet/TermBullet.csproj`
- `tests/TermBullet.Tests/TermBullet.Tests.csproj`
- initial test framework setup
- basic build and test pipeline

### Tests First

- test project must contain at least one intentional smoke test;
- test command must run successfully.

### Done Criteria

- `dotnet restore` passes;
- `dotnet build` passes;
- `dotnet test` passes;
- no production feature behavior is implemented yet.

## Milestone 1 - Core Domain

### Goal

Implement the minimum domain model.

### Deliverables

- item entity;
- item type enum/value object;
- item status rules;
- priority rules;
- collection rules;
- public ref value object;
- public ref generation policy contract or domain service.

### Tests First

Tests must cover:

- valid task/note/event creation;
- invalid empty content;
- invalid item type;
- valid status transitions;
- invalid status transitions;
- valid public ref parsing;
- invalid public ref parsing;
- public ref prefix by item type.

### Done Criteria

- all Core tests pass;
- Core has no dependency on CLI, TUI, Infrastructure, or database libraries.

## Milestone 2 - Application Use Cases

### Goal

Implement V1 use cases using mocked repositories.

### Deliverables

- create item;
- list items;
- show item;
- edit item;
- mark done;
- cancel item;
- migrate item;
- move item;
- tag/untag item;
- set priority;
- search items;
- today/week/backlog queries.

### Tests First

Use mocked repositories and mocked clocks/ID generators.

Tests must cover:

- successful use cases with valid data;
- not found public refs;
- duplicate public refs;
- invalid status changes;
- invalid collections;
- invalid priorities;
- invalid tag names;
- search with empty result;
- search with multiple results.

### Done Criteria

- all Application tests pass;
- use cases are independent from System.CommandLine, Terminal.Gui, and SQLite.

## Milestone 3 - SQLite Infrastructure

### Goal

Persist V1 data locally with SQLite.

### Deliverables

- schema migrations;
- SQLite connection management;
- item repository;
- tag repository or tag handling inside item repository;
- public ref sequence storage;
- local settings storage.

### Tests First

Use temporary SQLite databases.

Tests must cover:

- migration on empty database;
- create/read item;
- update item;
- list by collection;
- list by status;
- tag add/remove persistence;
- public ref sequence persistence;
- soft delete or delete behavior;
- malformed/invalid persistence inputs where applicable.

### Done Criteria

- infrastructure tests pass;
- schema matches `DATA_MODEL.md`;
- no PostgreSQL dependency is required for V1.

## Milestone 4 - CLI MVP

### Goal

Implement the official command tree for core V1 workflows.

### Deliverables

- `termbullet add`;
- `termbullet list`;
- `termbullet today`;
- `termbullet week`;
- `termbullet backlog`;
- `termbullet show`;
- `termbullet edit`;
- `termbullet done`;
- `termbullet cancel`;
- `termbullet migrate`;
- `termbullet move`;
- `termbullet tag`;
- `termbullet untag`;
- `termbullet priority`;
- `termbullet search`;
- global options where practical.

### Tests First

Tests must cover:

- command parsing with valid arguments;
- command parsing with missing required arguments;
- invalid options;
- successful handler execution with mocked use cases;
- readable success output;
- readable error output;
- help output for representative commands.

### Done Criteria

- all CLI tests pass;
- commands call Application use cases;
- no business rules are implemented in CLI handlers.

## Milestone 5 - Export, Import, and Config

### Goal

Add backup/migration support and local configuration.

### Deliverables

- `termbullet export`;
- `termbullet import`;
- `termbullet config list`;
- `termbullet config get`;
- `termbullet config set`;
- `termbullet config path`;
- JSON export format.

### Tests First

Tests must cover:

- export empty database;
- export populated database;
- import valid data;
- import malformed data;
- import duplicate public refs;
- config get/set;
- missing config key.

### Done Criteria

- tests pass;
- exported data preserves IDs, public refs, tags, status, collections, and timestamps.

## Milestone 6 - TUI MVP

### Goal

Implement the first usable TUI experience.

### Deliverables

- TUI startup when no command is provided;
- Main Dashboard;
- Daily Focus;
- Backlog Triage;
- Search;
- Config;
- keyboard focus model;
- footer shortcuts;
- action dispatch to Application use cases.

### Tests First

Tests should focus on non-rendering logic first:

- screen state initialization;
- focus movement;
- selected item changes;
- action dispatch;
- search query state;
- view model mapping from use case results.

Manual verification may be required for terminal rendering.

### Done Criteria

- TUI tests pass where practical;
- manual smoke test confirms navigation and rendering;
- TUI does not implement business rules directly.

## Milestone 7 - V1 Release Candidate

### Goal

Stabilize the offline V1.

### Deliverables

- documentation review;
- command help review;
- import/export validation;
- database migration validation;
- cross-platform smoke testing where practical;
- release notes draft.

### Tests First

Regression tests must cover:

- item lifecycle from create to done/cancel/migrate;
- public ref sequence behavior;
- CLI and Application consistency;
- persistence round trip;
- export/import round trip.

### Done Criteria

- `dotnet restore` passes;
- `dotnet build` passes;
- `dotnet test` passes;
- CLI smoke tests pass;
- TUI smoke test passes;
- README and docs reflect actual behavior.

## Definition of Done

A task is done only when:

- tests were written first;
- valid and invalid mocked data were covered where applicable;
- implementation is complete;
- all relevant tests pass;
- documentation is updated when behavior changes;
- no V2/V3/V4 feature was accidentally introduced into V1 scope.
