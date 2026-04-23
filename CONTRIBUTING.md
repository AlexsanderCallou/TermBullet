# Contributing to TermBullet

Thank you for considering a contribution to TermBullet.

TermBullet is an English-first open source project for a global audience. This document defines contribution expectations for code, tests, documentation, issues, and pull requests.

## Project Status

TermBullet is currently in early design and V1 planning.

Official repository:

```text
https://github.com/AlexsanderCallou/TermBullet
```

Use `Development` as the base branch for work unless maintainers say otherwise.

Before contributing, read:

- [README.md](README.md)
- [product-spec.md](product-spec.md)
- [ADR.md](ADR.md)
- [AGENTS.md](AGENTS.md)
- [ARCHITECTURE.md](ARCHITECTURE.md)
- [DATA_MODEL.md](DATA_MODEL.md)
- [DEVELOPMENT_PLAN.md](DEVELOPMENT_PLAN.md)

## Language

Use English for:

- issues;
- pull requests;
- documentation;
- code comments;
- CLI help text;
- TUI labels;
- error messages;
- commit messages.

## Development Method

TermBullet follows TDD.

Before writing production code:

1. Write unit tests first.
2. Cover valid data and successful paths.
3. Cover invalid, missing, malformed, or conflicting data.
4. Confirm tests fail for the expected reason when practical.
5. Implement the smallest production change that makes tests pass.
6. Run all relevant tests.

A contribution is not complete until all relevant tests pass.

## Local Setup

Expected setup after the solution is created:

```bash
dotnet restore
dotnet build
dotnet test
```

Run the app locally:

```bash
dotnet run --project src/TermBullet -- [command] [arguments] [options]
```

Example:

```bash
dotnet run --project src/TermBullet -- add "fix jwt authentication"
```

## Architecture Expectations

TermBullet uses a modular monolith.

Production code lives in one .NET project and is separated internally by folders and namespaces:

- `Core`
- `Application`
- `Infrastructure`
- `Cli`
- `Tui`
- `Bootstrap`

Respect dependency direction:

- Core depends on nothing internal.
- Application depends on Core.
- Infrastructure implements Application contracts.
- CLI calls Application use cases.
- TUI calls Application use cases.
- Bootstrap wires modules together.

Do not put business rules in CLI handlers, TUI screens, or JSON file repositories.

## Commit Style

Use Conventional Commits:

```text
<type>(<scope>): <description>
```

Examples:

```text
feat(cli): add item creation command
fix(core): reject empty item content
test(application): cover migrate item failures
docs: add data model draft
refactor(infrastructure): isolate json file writer
```

Common types:

- `feat`
- `fix`
- `test`
- `docs`
- `refactor`
- `chore`
- `build`
- `ci`

## Branch Naming

Use short descriptive branch names:

```text
feat/cli-add-command
fix/public-ref-sequence
docs/data-model
test/application-use-cases
```

Open pull requests against `Development`.

## Pull Request Checklist

Before opening a pull request, verify:

- tests were written before production implementation;
- valid and invalid test cases are covered;
- `dotnet restore` passes;
- `dotnet build` passes;
- `dotnet test` passes;
- CLI help/output was checked when CLI behavior changed;
- TUI navigation/rendering was checked when TUI behavior changed;
- JSON file persistence, backup, and recovery were checked when persistence changed;
- docs were updated when behavior or architecture changed;
- ADR was added or updated for major decisions;
- V1 scope was respected.

## Issues

Good issues include:

- clear problem statement;
- expected behavior;
- actual behavior if applicable;
- reproduction steps if applicable;
- relevant command or screen;
- whether the issue affects Core, Application, Infrastructure, CLI, TUI, or docs.

## Documentation Changes

Update documentation when changing:

- commands;
- options;
- output format;
- TUI behavior;
- architecture;
- data model;
- development process;
- dependencies.

Use:

- `README.md` for high-level project information;
- `product-spec.md` for product behavior;
- `ADR.md` for architecture decisions;
- `ARCHITECTURE.md` for technical structure;
- `DATA_MODEL.md` for persistence structure;
- `DEVELOPMENT_PLAN.md` for V1 implementation order;
- `AGENTS.md` for AI agent development rules.

## Dependency Policy

Before adding a dependency, confirm that it:

- fits the official .NET 8 / C# stack;
- is necessary;
- is suitable for open source usage;
- does not break offline/local-first behavior;
- does not duplicate standard .NET capabilities without a strong reason.

Major dependencies require an ADR.

## Scope Policy

V1 is offline and local-first.

Do not add these unless explicitly requested:

- AI execution;
- Google Calendar integration;
- machine sync;
- cloud accounts;
- PostgreSQL runtime dependency for local usage.

Future-facing interfaces are acceptable when they keep V1 simple.

## License

TermBullet is released under the MIT License. See [LICENSE](LICENSE).
