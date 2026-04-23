# TermBullet - Architecture Decision Records

This file consolidates TermBullet's initial architecture decisions.

The ADRs below are derived from [product-spec.md](product-spec.md). They guide implementation and record the product, architecture, and technology decisions that shape the project.

## Index

- [ADR-0001 - Local-first product](#adr-0001---local-first-product)
- [ADR-0002 - CLI and TUI as first-class interfaces](#adr-0002---cli-and-tui-as-first-class-interfaces)
- [ADR-0003 - Layered architecture](#adr-0003---layered-architecture)
- [ADR-0004 - Global internal identity and public ref](#adr-0004---global-internal-identity-and-public-ref)
- [ADR-0005 - Initial model with task, note, and event](#adr-0005---initial-model-with-task-note-and-event)
- [ADR-0006 - Local persistence as the operational source](#adr-0006---local-persistence-as-the-operational-source)
- [ADR-0007 - Optional and modular integrations](#adr-0007---optional-and-modular-integrations)
- [ADR-0008 - Future synchronization at entity level](#adr-0008---future-synchronization-at-entity-level)
- [ADR-0009 - TUI based on screens, panels, and keyboard](#adr-0009---tui-based-on-screens-panels-and-keyboard)
- [ADR-0010 - Export and import as basic portability](#adr-0010---export-and-import-as-basic-portability)
- [ADR-0011 - Open source and English-first project](#adr-0011---open-source-and-english-first-project)
- [ADR-0012 - Official technology stack](#adr-0012---official-technology-stack)
- [ADR-0013 - Modular monolith structure](#adr-0013---modular-monolith-structure)

---

## ADR-0001 - Local-first Product

**Status:** Accepted  
**Date:** 2026-04-22

### Context

TermBullet must be a personal productivity tool for the terminal. The product needs to be useful from V1 without depending on internet access, online accounts, external services, AI, external calendars, or cloud infrastructure.

The primary audience includes developers and technical users who value speed, predictability, offline operation, and control over their own data.

### Decision

TermBullet will be local-first.

In V1, every essential operation must work locally:

- create, list, edit, and change items;
- navigate through the TUI;
- use CLI commands;
- query Today, Week, and Backlog;
- search items;
- export and import data;
- manage local configuration.

External services will be treated as optional extensions in future versions.

### Consequences

- The local database is the user's primary operational source.
- The product remains functional without network access.
- The architecture must avoid direct dependency on external providers in the domain.
- Future AI, calendar, and sync features must be attached through ports, adapters, or infrastructure modules.

### Alternatives Considered

- **Cloud-first:** rejected because it would make accounts, network, and servers central to the workflow.
- **Local as cloud cache:** rejected because it weakens the local-first philosophy and complicates V1.

---

## ADR-0002 - CLI and TUI as First-Class Interfaces

**Status:** Accepted  
**Date:** 2026-04-22

### Context

The product must support two natural terminal usage modes:

- continuous visual usage through the TUI;
- fast capture and manipulation through the CLI.

The CLI must not be just a limited shortcut for opening the TUI.

### Decision

CLI and TUI will be first-class interfaces.

Everything essential in the system must be possible through the CLI and also available in the TUI, respecting the experience differences between both interfaces.

Running the executable without a command must open the main TUI:

```bash
termbullet
```

Fast operations must be available through commands such as:

```bash
termbullet add "fix jwt authentication"
termbullet today
termbullet done t-0422-1
termbullet search "jwt"
```

### Consequences

- CLI and TUI cannot duplicate business rules.
- Both interfaces must call the same Application layer use cases.
- Command design must be stable, predictable, and documented.
- The TUI may offer a richer flow, but it must not become the only path for fundamental operations.

### Alternatives Considered

- **TUI as the only interface:** rejected because it hurts fast capture and shell automation.
- **CLI as the only interface:** rejected because it weakens visual planning, review, and triage.

---

## ADR-0003 - Layered Architecture

**Status:** Accepted  
**Date:** 2026-04-22

### Context

TermBullet should start simple in V1, but it must be ready for V2, V3, and V4. Later phases include AI, Google Calendar, synchronization between machines, and a possible optional cloud layer.

If business rules are coupled directly to CLI, TUI, or local storage, future versions will require large refactors.

### Decision

The project will adopt a layered architecture:

- **Core:** entities, business rules, states, identification policies, and migration policies.
- **Application:** use cases, orchestration services, and input/output contracts.
- **Infrastructure:** local persistence, import/export, AI, calendar, and future sync.
- **CLI:** commands, handlers, and text rendering.
- **TUI:** screens, navigation, shortcuts, panels, and visual context.

Central rule:

> CLI and TUI must reuse the same Application layer use cases.

### Consequences

- The domain remains protected from interface and provider details.
- Replacing local persistence or adding future sync should not require rewriting use cases.
- Business-rule tests can be written without depending on a terminal, a real database, or external integrations.
- The project will have more initial structure than a simple script, but better support for evolution.

### Alternatives Considered

- **Procedural monolith:** rejected because it makes evolution and testing harder.
- **Framework-driven architecture:** rejected as an initial decision because the product should not yet be shaped by a specific framework.

---

## ADR-0004 - Global Internal Identity and Public Ref

**Status:** Accepted  
**Date:** 2026-04-22

### Context

Users need to manipulate items quickly in CLI and TUI. Long technical IDs are poor for typing, while simple sequential numbers can become ambiguous and do not prepare the product well for future sync.

The product also needs stable identity for migration, export, import, and future synchronization.

### Decision

Each relevant entity will have two identifiers:

- **Global internal ID:** technical, unique, stable, and immutable.
- **Public Ref:** short, readable, and user-facing.

Official public ref format:

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

- the sequence is independent by type and day;
- the public ref must be persisted;
- the public ref must not be reused;
- the internal ID remains the real entity identity.

### Consequences

- The CLI experience becomes more readable and typeable.
- Future sync can rely on the internal ID, not the human reference.
- Public ref generation must be an explicit domain or application policy, not a loose CLI detail.
- Future import and merge flows must handle public ref collisions carefully.

### Alternatives Considered

- **Only sequential numbers:** rejected because of low robustness and ambiguity risk.
- **Only exposing UUIDs:** rejected because of poor terminal usability.
- **Using title as identifier:** rejected because titles are unstable and naturally collide.

---

## ADR-0005 - Initial Model With Task, Note, and Event

**Status:** Accepted  
**Date:** 2026-04-22

### Context

The product is inspired by Bullet Journal, but it must keep a model simple enough for V1. The main personal workflow items are tasks, notes, and events.

There is also an important product rule: task and event are distinct concepts. A task must not automatically become an event.

### Decision

V1 will adopt three main item types:

- **Task:** executable pending work.
- **Note:** record, context, or observation.
- **Event:** appointment or internal time-based marker.

The required initial collections are:

- **Today**
- **Week**
- **Backlog**

### Consequences

- The initial model covers capture, execution, planning, and review without too many types.
- UI and CLI can keep commands simple and predictable.
- Future calendar integration must respect the difference between local event, external event, and task.
- New item types should be evaluated carefully to avoid fragmenting the workflow.

### Alternatives Considered

- **Generic model with only Item:** rejected because it loses important semantics for commands, screens, and integrations.
- **Many types from V1:** rejected because it increases complexity before validating the core.

---

## ADR-0006 - Local Persistence as the Operational Source

**Status:** Accepted  
**Date:** 2026-04-22

### Context

V1 must work offline and store data locally. Persistence should be simple to start, but prepared for versioning, migrations, and future sync.

Each relevant entity must record enough metadata for basic auditability and evolution:

- internal ID;
- public ref;
- type;
- content;
- status;
- creation date;
- update date;
- current collection;
- priority;
- tags.

### Decision

Local persistence will be the primary operational source in V1.

The concrete storage implementation can be defined later, but it must satisfy these requirements:

- operate offline;
- allow fast queries for CLI and TUI;
- store consistent timestamps;
- support versioning or migrations;
- preserve internal IDs and public refs;
- allow basic export and import;
- avoid coupling the domain to storage details.

### Consequences

- The domain should depend on repository contracts, not on a concrete database implementation.
- The local database choice should consider simplicity, portability, and migration support.
- The persistence format must anticipate future entity-level sync.

### Alternatives Considered

- **Single file without migration structure:** rejected because it limits evolution.
- **Mandatory remote database:** rejected because it breaks the local-first requirement.

---

## ADR-0007 - Optional and Modular Integrations

**Status:** Accepted  
**Date:** 2026-04-22

### Context

The roadmap includes AI in V2, Google Calendar in V3, and sync/cloud in V4. These features increase product value, but they cannot be prerequisites for usage.

AI, calendar, and cloud also involve credentials, network access, external providers, and failures outside the application's control.

### Decision

External integrations will be optional and modular.

In V2, AI will follow the BYOK model:

- user provides provider;
- user provides model;
- user provides API key;
- user may provide base URL;
- the app controls internal profiles such as `plan-day`, `review-day`, `breakdown-task`, and `prioritize-backlog`.

In V3, Google Calendar will be an optional integration for schedule context.

In V4, sync/cloud will be an optional layer over the local database.

### Consequences

- The product must behave correctly when integrations are absent, misconfigured, or offline.
- Credentials and sensitive configuration must stay outside the domain.
- The Application layer should orchestrate optional capabilities through explicit contracts.
- The main experience cannot depend on external calls.

### Alternatives Considered

- **AI embedded as a mandatory dependency:** rejected because it would make the product less predictable and require an account/key for common usage.
- **Mandatory cloud sync from V1:** rejected because it anticipates complexity and contradicts local-first.

---

## ADR-0008 - Future Synchronization at Entity Level

**Status:** Accepted  
**Date:** 2026-04-22

### Context

The roadmap includes usage across multiple machines. A simple approach would be synchronizing the physical local database file directly, but this tends to cause corruption, opaque conflicts, and weak merge control.

The product needs to preserve a complete local database on each machine.

### Decision

Future synchronization will happen at entity level, not by directly synchronizing the physical local database file.

Each machine will keep a complete local database. The sync layer must operate over records, changes, and entity metadata.

### Consequences

- The global internal ID is mandatory for sync integrity.
- The model needs consistent timestamps and metadata from V1.
- Conflicts must be treated as a normal part of operation.
- Sync can be implemented later without turning the local database into a disposable cache.

### Alternatives Considered

- **Synchronizing the local database file:** rejected because of conflict and corruption risk.
- **Using only cloud as the source of truth:** rejected because it breaks local-first.

---

## ADR-0009 - TUI Based on Screens, Panels, and Keyboard

**Status:** Accepted  
**Date:** 2026-04-22

### Context

TermBullet must be a terminal-first tool with the feel of a mature TUI product. The desired experience is close to LazyDocker, LazyGit, K9s, and btop: high density, clear focus, keyboard navigation, and informative panels.

### Decision

The TUI will be based on screens and panels, with keyboard navigation.

V1 must include:

- Main Dashboard;
- Daily Focus;
- Weekly Planning;
- Backlog Triage;
- Review;
- Search / Command Palette;
- Config.

Cross-screen behaviors:

- `Tab` advances focus;
- `Shift+Tab` moves focus back;
- `Enter` opens or expands the active panel;
- `Esc` returns to the previous layout;
- `/` opens contextual filtering;
- `c` opens quick capture;
- `?` opens compact help.

### Consequences

- The TUI needs an explicit model for focus, current screen, and active panel.
- Frequent operations must be accessible without a mouse.
- Visual rendering must preserve density and legibility.
- Future calendar and sync screens must follow the same interaction language.

### Alternatives Considered

- **Sequential prompt interface:** rejected because it does not meet the operational cockpit goal.
- **Mouse-dependent interface:** rejected because it contradicts terminal-first usage.

---

## ADR-0010 - Export and Import as Basic Portability

**Status:** Accepted  
**Date:** 2026-04-22

### Context

Before sync/cloud exists, users still need a simple path for backup, migration, and data recovery.

Export and import also help validate data contracts and prepare the project for future integrations.

### Decision

V1 must provide local export and import in simple formats.

These features must operate over the persisted model while preserving:

- internal IDs;
- public refs;
- types;
- status;
- collections;
- priorities;
- tags;
- relevant timestamps.

### Consequences

- The exported format must be stable enough for real backup.
- Import must handle existing data and possible conflicts.
- The export/import contract can become the basis for future migration tooling.

### Alternatives Considered

- **Postponing export/import until after sync:** rejected because local backup is a basic requirement for a local-first product.
- **Exporting only rendered text:** rejected because it does not preserve enough structure for restoration.

---

## ADR-0011 - Open Source and English-First Project

**Status:** Accepted  
**Date:** 2026-04-22

### Context

TermBullet is intended for a global audience and should be developed as an open source project. Project artifacts should be readable and usable by international contributors and users.

Open source projects also need explicit documentation, contribution paths, and stable architecture decisions so contributors can understand why the system is shaped the way it is.

### Decision

TermBullet documentation, command names, examples, user-facing text, and architecture records will be English-first.

The project will be prepared for open source publication, including:

- README in English;
- product specification in English;
- ADRs in English;
- future `LICENSE` file;
- future contribution guidelines;
- future code of conduct or governance notes if the project becomes community-driven.

### Consequences

- New documentation should be written in English by default.
- Command names and output examples should avoid non-English terms.
- Product decisions should be documented publicly when they affect contributors or long-term architecture.
- A license must be chosen before public release.

### Alternatives Considered

- **Non-English-first documentation:** rejected because it limits global adoption and contribution.
- **No explicit open source posture:** rejected because it leaves licensing, contribution, and governance ambiguous.

---

## ADR-0012 - Official Technology Stack

**Status:** Accepted  
**Date:** 2026-04-23

### Context

TermBullet needs a stack that supports a local-first terminal application with both a rich TUI and a robust CLI. The project must also stay maintainable as an open source codebase and be able to evolve toward AI, calendar integration, and cross-device sync.

The stack must support:

- cross-platform terminal usage;
- a panel/window-based TUI;
- predictable CLI parsing and help output;
- offline local storage in V1;
- a future server-side database for sync/cloud in V4.

### Decision

TermBullet will use the following official technology stack:

- **.NET 8 / C#** as the main platform and implementation language.
- **Terminal.Gui** for the TUI, using a panel/window-based layout.
- **System.CommandLine** for the command-line interface.
- **SQLite** as the local offline database in V1.
- **PostgreSQL** as the future backend database for synchronization/cloud in V4.

Official references:

- [.NET 8 / C#](https://learn.microsoft.com/pt-br/dotnet/core/whats-new/dotnet-8/overview)
- [Terminal.Gui](https://github.com/gui-cs/Terminal.Gui)
- [System.CommandLine](https://learn.microsoft.com/en-us/dotnet/standard/commandline/)
- [SQLite](https://www.sqlite.org/docs.html)
- [PostgreSQL](https://www.postgresql.org/docs/)

### Consequences

- The codebase can use a single language and runtime across Core, Application, Infrastructure, CLI, and TUI.
- CLI and TUI can share the same Application layer without language or process boundaries.
- SQLite supports the V1 local-first/offline requirement.
- PostgreSQL is reserved for the optional V4 server-side sync/cloud layer and must not replace local SQLite as the user's operational store.
- Terminal.Gui shapes the TUI implementation around windows, panels, focus, keyboard navigation, and terminal rendering.
- System.CommandLine shapes CLI implementation around explicit commands, arguments, options, help output, and future JSON-capable command flows.

### Alternatives Considered

- **Go with Bubble Tea/Lip Gloss:** strong for terminal apps, but would move the project away from the .NET/C# ecosystem.
- **Rust with ratatui/clap:** strong performance and terminal tooling, but raises implementation complexity and contributor barrier for this project.
- **Node.js with terminal UI libraries:** viable for CLI tooling, but weaker fit for a long-lived local-first desktop terminal app with rich persistence and future backend sharing.
- **PostgreSQL-only storage:** rejected for V1 because it breaks the lightweight local-first/offline experience.

---

## ADR-0013 - Modular Monolith Structure

**Status:** Accepted  
**Date:** 2026-04-23

### Context

TermBullet is a small terminal-first application. A multi-project architecture with one production project per layer would add ceremony before the product needs it.

The project still needs clear boundaries between domain rules, use cases, persistence, CLI, and TUI, but those boundaries can be enforced through folders, namespaces, contracts, tests, and review discipline.

### Decision

TermBullet V1 will use a modular monolith.

Production code will live in one .NET project:

```text
src/TermBullet/TermBullet.csproj
```

Internal modules will be separated by folders and namespaces:

- `Bootstrap`
- `Core`
- `Application`
- `Infrastructure`
- `Cli`
- `Tui`

Tests will live in a separate test project:

```text
tests/TermBullet.Tests/TermBullet.Tests.csproj
```

### Consequences

- The product remains simple to build, run, and package.
- CLI and TUI can share Application use cases without extra project ceremony.
- Internal boundaries must be maintained through discipline and tests rather than project references.
- Future extraction into separate projects is possible if the codebase grows enough to justify it.

### Alternatives Considered

- **One production project per layer:** rejected for V1 because the project is small and the extra structure would slow early development.
- **Single folder with no module boundaries:** rejected because it would make future AI, calendar, and sync work harder.

---

## Final Notes

These decisions define TermBullet's initial architectural direction, but they do not yet choose:

- final export format;
- default AI provider;
- open source license.

Those choices should be recorded in future ADRs when there is enough technical context for concrete decisions.
