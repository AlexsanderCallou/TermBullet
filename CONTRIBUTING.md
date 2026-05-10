# Contributing to TermBullet

TermBullet is an English-first open source project for a global audience.

Study context: TermBullet is part of the author's study on using AI to support
software coding and project delivery. It is recommended for personal use,
experimentation, and learning only.

Use `Development` as the base branch unless maintainers say otherwise.

## Read First

- [README.md](README.md)
- [PRODUCT.md](PRODUCT.md)
- [CLI.md](CLI.md) when changing commands
- [screens.md](screens.md) when changing the TUI
- [ARCHITECTURE.md](ARCHITECTURE.md)
- [DATA_MODEL.md](DATA_MODEL.md)
- [ADR.md](ADR.md)
- [BACKLOG.md](BACKLOG.md)

## Language

Use English for issues, pull requests, documentation, comments, CLI help, TUI
labels, error messages, examples, and commit messages.

## Development Method

TermBullet follows TDD.

Before production code:

1. Write unit tests first where practical.
2. Cover valid data and successful paths.
3. Cover invalid, missing, malformed, or conflicting data.
4. Confirm tests fail for the expected reason when practical.
5. Implement the smallest production change.
6. Run relevant tests.

## Local Setup

```bash
dotnet restore
dotnet build
dotnet test
dotnet run --project src/TermBullet -- [command] [arguments] [options]
```

Run the relevant subset before opening a pull request, and run all three
verification commands for code-affecting changes when practical.

## Architecture Expectations

TermBullet uses one production project with clear folders:

- `Domain`
- `Application`
- `Repositories`
- `Services`
- `Cli`
- `Tui`
- `Bootstrap`

Respect dependency direction:

- Domain depends on no internal outer folder.
- Application depends on Domain and repository/service interfaces.
- Repositories implement persistence contracts.
- Services implement technical service contracts.
- CLI and TUI call Application use cases.
- Bootstrap wires everything together.

Do not put business rules in CLI handlers, TUI screens, or JSON repositories.

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
docs: update data model
refactor(infrastructure): isolate json file writer
```

Common types: `feat`, `fix`, `test`, `docs`, `refactor`, `chore`, `build`,
`ci`.

## Pull Request Checklist

- tests were written first where practical;
- valid and invalid cases are covered;
- `dotnet restore`, `dotnet build`, and `dotnet test` pass;
- CLI help/output was checked when CLI behavior changed;
- TUI navigation/rendering was checked when TUI behavior changed;
- persistence backup/recovery was checked when persistence changed;
- docs were updated when behavior or architecture changed;
- ADR was added or updated for major decisions;
- V1 scope was respected.

## Issues

Good issues include:

- clear problem statement;
- expected behavior;
- actual behavior if applicable;
- reproduction steps;
- relevant command or screen;
- affected area: Domain, Application, Repositories, Services, CLI, TUI, docs.

## Dependency Policy

Before adding a dependency, confirm it:

- fits the official .NET 8 / C# stack;
- is necessary;
- is suitable for open source usage;
- preserves offline/local-first behavior;
- does not duplicate standard .NET capabilities without a strong reason.

Major dependencies require an ADR.

## Scope Policy

V1 is offline and local-first. Do not add AI execution, Google Calendar, machine
sync, cloud accounts, or PostgreSQL runtime dependency unless explicitly
requested.

Future-facing interfaces are acceptable when they keep V1 simple.

## Legal

Legal policy and trademark usage are in [TRADEMARKS.md](TRADEMARKS.md). The
project license is Apache License 2.0 in [LICENSE](LICENSE).
