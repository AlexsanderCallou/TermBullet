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
- [ADR-0008 - Future synchronization at JSON file level](#adr-0008---future-synchronization-at-json-file-level)
- [ADR-0009 - TUI based on screens, panels, and keyboard](#adr-0009---tui-based-on-screens-panels-and-keyboard)
- [ADR-0010 - Export and import as basic portability](#adr-0010---export-and-import-as-basic-portability)
- [ADR-0011 - Open source and English-first project](#adr-0011---open-source-and-english-first-project)
- [ADR-0012 - Official technology stack](#adr-0012---official-technology-stack)
- [ADR-0013 - Modular monolith structure](#adr-0013---modular-monolith-structure)
- [ADR-0014 - Monthly JSON storage for V1](#adr-0014---monthly-json-storage-for-v1)
- [ADR-0015 - Apache-2.0 open source license](#adr-0015---apache-20-open-source-license)

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

- Local JSON files are the user's primary operational source.
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
termbullet done t-0426-1
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
- Business-rule tests can be written without depending on a terminal, real storage, or external integrations.
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
<type>-<MMYY>-<sequence>
```

Prefixes:

- `t` = task
- `n` = note
- `e` = event

Examples:

```text
t-0426-1
n-0426-1
e-0426-1
```

Rules:

- the sequence is independent by type and month/year;
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

V1 must work offline and store data locally. Persistence should be simple to start, human-readable, recoverable, and prepared for future file-level sync.

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
- version.

### Decision

Monthly JSON file persistence will be the primary operational source in V1.

The storage implementation must satisfy these requirements:

- operate offline;
- allow fast queries for CLI and TUI;
- store consistent timestamps;
- support item-level versioning;
- preserve internal IDs and public refs;
- write safely with backup/recovery;
- allow basic export and import;
- avoid coupling the domain to storage details.

### Consequences

- The domain should depend on repository contracts, not on concrete file storage implementation.
- The local storage choice should consider simplicity, portability, and recovery support.
- The persistence format must anticipate future file-level sync.

### Alternatives Considered

- **Single file for all data:** rejected because monthly files reduce file size and sync conflict scope.
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

In V4, sync/cloud will be an optional layer over local JSON files.

### Consequences

- The product must behave correctly when integrations are absent, misconfigured, or offline.
- Credentials and sensitive configuration must stay outside the domain.
- The Application layer should orchestrate optional capabilities through explicit contracts.
- The main experience cannot depend on external calls.

### Alternatives Considered

- **AI embedded as a mandatory dependency:** rejected because it would make the product less predictable and require an account/key for common usage.
- **Mandatory cloud sync from V1:** rejected because it anticipates complexity and contradicts local-first.

---

## ADR-0008 - Future Synchronization at JSON File Level

**Status:** Superseded by ADR-0014  
**Date:** 2026-04-22

### Context

The roadmap includes usage across multiple machines. The original direction was entity-level sync over a local database, but the storage model has since moved to monthly JSON files, and V4 sync/cloud will synchronize whole JSON files.

The product still needs to preserve complete local data on each machine.

### Decision

Future synchronization will happen at JSON file level.

Each machine will keep complete local JSON files. In V4, the sync layer will synchronize monthly files and use latest update wins for simple conflicts.

### Consequences

- The global internal ID remains mandatory for item identity.
- Item `updated_at` and `version` remain useful for future merge and diagnostics.
- Conflicts must be treated as a normal part of operation.
- V1 does not implement multi-machine conflict handling and assumes one active machine at a time.
- Sync can be implemented later without turning local files into a disposable cache.

### Alternatives Considered

- **Entity-level sync:** superseded by the monthly JSON file storage decision.
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

V1 must provide local export and import in JSON format.

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

- The JSON export format must be stable enough for real backup.
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
- Apache-2.0 `LICENSE` file;
- `TRADEMARKS.md` with mark usage guidance;
- explicit independence statement (no affiliation/endorsement/sponsorship by third-party trademark owners);
- historical inspiration statement with trademark attribution for external marks;
- future contribution guidelines;
- trademark usage guidance for project and third-party marks;
- future code of conduct or governance notes if the project becomes community-driven.

### Consequences

- New documentation should be written in English by default.
- Command names and output examples should avoid non-English terms.
- Product decisions should be documented publicly when they affect contributors or long-term architecture.
- The project license is Apache-2.0.

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
- **Monthly JSON files** as the local offline data store in V1.
- **Local JSON index** for faster lookup and search.
- **PostgreSQL** as the future backend database for synchronization/cloud in V4, storing the same JSON files.

Official references:

- [.NET 8 / C#](https://learn.microsoft.com/pt-br/dotnet/core/whats-new/dotnet-8/overview)
- [Terminal.Gui](https://github.com/gui-cs/Terminal.Gui)
- [System.CommandLine](https://learn.microsoft.com/en-us/dotnet/standard/commandline/)
- [PostgreSQL](https://www.postgresql.org/docs/)

### Consequences

- The codebase can use a single language and runtime across Core, Application, Infrastructure, CLI, and TUI.
- CLI and TUI can share the same Application layer without language or process boundaries.
- Monthly JSON files support the V1 local-first/offline requirement while remaining human-readable and easy to back up.
- PostgreSQL is reserved for the optional V4 server-side sync/cloud layer and must not replace local monthly JSON files as the user's operational store.
- Terminal.Gui shapes the TUI implementation around windows, panels, focus, keyboard navigation, and terminal rendering.
- System.CommandLine shapes CLI implementation around explicit commands, arguments, options, help output, and future JSON-capable command flows.

### Alternatives Considered

- **Go with Bubble Tea/Lip Gloss:** strong for terminal apps, but would move the project away from the .NET/C# ecosystem.
- **Rust with ratatui/clap:** strong performance and terminal tooling, but raises implementation complexity and contributor barrier for this project.
- **Node.js with terminal UI libraries:** viable for CLI tooling, but weaker fit for a long-lived local-first desktop terminal app with rich persistence and future backend sharing.
- **SQLite local storage:** rejected for V1 after review because monthly JSON files better fit simple backup, human readability, future AI context, and file-based sync.
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

## ADR-0014 - Monthly JSON Storage for V1

**Status:** Accepted  
**Date:** 2026-04-23

### Context

TermBullet is local-first, personal, and expected to handle hundreds of items per month rather than large multi-user workloads. The storage model should be transparent, easy to back up, easy to inspect manually, and friendly to future AI context assembly.

Monthly JSON files also make simple file sync services viable for single-machine-at-a-time usage in V1 and prepare the V4 cloud model to synchronize whole files.

### Decision

V1 will use monthly JSON files as the local operational store.

File layout:

```text
data/<year>/data_<month>_<year>.json
```

Example:

```text
data/2026/data_04_2026.json
```

Rules:

- only the official file naming pattern is supported;
- the TUI loads the current month by default;
- a local JSON index supports faster lookup and search;
- complex searches may read all monthly files;
- writes use a temporary file followed by atomic replacement;
- one backup is kept per monthly file;
- corrupted monthly files should recover from backup when possible;
- V1 assumes one active machine at a time;
- V4 sync/cloud will synchronize whole JSON files;
- PostgreSQL remains part of V4 but stores the same JSON file content.

Public refs use this format:

```text
<type>-<MMYY>-<sequence>
```

Examples:

```text
t-0426-1
n-0426-1
e-0426-1
```

History is stored as a root-level `history` array in each monthly file. Delete physically removes the item from active `items` and appends a `deleted` history event with an item snapshot.

### Consequences

- Local data is human-readable and easy to back up.
- V1 avoids a database dependency.
- The app must implement safe writes and backup recovery carefully.
- Search needs a rebuildable local index.
- Multi-machine conflict handling is deferred to V4.
- JSON structure must remain stable and documented because users may inspect files directly.

### Alternatives Considered

- **SQLite:** rejected for V1 because monthly JSON files better support transparency, file backup, future AI context, and simple file-level sync.
- **One JSON file for all data:** rejected because monthly files reduce file size and conflict scope.
- **Entity-level cloud storage:** deferred because V4 will synchronize whole JSON files.

---

## ADR-0015 - Apache-2.0 Open Source License

**Status:** Accepted  
**Date:** 2026-04-23

### Context

TermBullet is intended to be an open source project for a global audience. The license must be simple, widely understood, permissive, and friendly to community usage, modification, redistribution, and package manager distribution.

### Decision

TermBullet will use the Apache License 2.0.

The repository must include a root `LICENSE` file using the standard Apache License 2.0 text with:

```text
Copyright (c) 2026 TermBullet contributors
```

### Consequences

- Users may use, modify, and distribute the project under Apache-2.0 terms.
- The license includes an explicit patent grant and contributor patent terms.
- The project is distributed without warranty as described by Apache-2.0.
- Package manager distribution remains straightforward because Apache-2.0 is widely accepted.

### Alternatives Considered

- **MIT:** also permissive and concise, but Apache-2.0 is preferred for its explicit patent grant and clearer protection terms for open source collaboration.
- **GPL-family license:** rejected because a copyleft license would be more restrictive than intended for TermBullet.
- **No license yet:** rejected because public open source usage would remain legally ambiguous.

---

## Final Notes

These decisions define TermBullet's initial architectural direction.

There are no known V1-blocking architecture decisions left open in this document.

Future ADRs may still be needed for release automation details, package manager ownership, V4 conflict handling implementation, and any optional AI provider presets if the BYOK model evolves.
