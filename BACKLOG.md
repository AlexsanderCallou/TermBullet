# TermBullet Backlog

This file tracks the current V1 status and the next work candidates. Historical
implementation details belong in release notes and git history.

## Current Status

V1 offline core is complete and released.

Delivered:

- CLI and TUI over shared Application use cases.
- Tasks, notes, and events.
- Today, Week, Month, and Backlog task collections.
- Forgotten review as a derived TUI view.
- Create, list, show, edit, done, cancel, delete, migrate, move, tag, untag,
  priority, search, path, and history clear flows.
- First-run data root selection stored in install-directory `conf.json`.
- Monthly JSON persistence under `<data_root>/data`.
- Safe JSON writes, one backup per operational file, backup recovery, local
  JSON index, and readable JSON formatting.
- Tags catalog, item history, and Item Detail history display.
- Windows x64 and Linux x64 release assets.

## Open V1 Hardening

These are quality and distribution improvements, not blockers for the V1
offline core.

- Decide whether the CLI needs a derived `forgotten` command.
- Add broader regression tests for complete item lifecycle flows.
- Add broader persistence round-trip and backup/recovery tests.
- Run cross-platform smoke testing with published Windows and Linux binaries.
- Improve install, update, and uninstall workflows.

## Post-V1

### V2 - AI Planning

Goal: add optional AI-assisted planning while preserving local-first behavior.
AI must propose structured drafts, the application must validate them, and the
user must approve before anything is persisted.

V2 MVP behavior scenarios:

- Java study roadmap: when the user asks for tag `estudo-java`, one task for
  `today`, four tasks for `week`, remaining near-term tasks for `month`, and
  longer work for `backlog`, the draft must create the requested tag, one scope
  note, and ordered tasks in the requested collections.
- Nutrition chatbot project: when the user asks to build a chatbot project, the
  draft must create a project tag, one scope note, and initial tasks.
- Gym habit tracking: when the user explicitly asks for a tag for an ongoing
  personal habit, the draft must create that non-default tag and weekly
  tracking tasks. Recurring tasks are not part of V2 MVP.
- Local AI setup: Ollama is the recommended local model runtime for users.
  Hosted providers and other services may be used through OpenAI-compatible
  profiles, but the README should present Ollama as the local path.

#### V2.0 - Planning Contracts and ADR

- Keep ADR-0018 aligned with the approved AI workflow.
- Keep `screens.md` aligned with guided New Planning.
- Keep `DATA_MODEL.md` aligned with AI proposal contracts and history events.
- Keep the canonical planning agent prompt aligned with the accepted V2
  behavior scenarios.
- Define acceptance criteria for AI being unavailable, misconfigured, or
  returning invalid drafts.

#### V2.1 - BYOK AI Configuration

Status: in progress.

Implemented foundation:

- `<data_root>/.aiconf` stores named AI profiles in an editable comment-friendly
  line format.
- `termbullet test-ai` creates the `.aiconf` template when missing and validates
  the active profile.
- `termbullet set-ai <name>` sets the default profile when more than one profile
  is configured.
- Documentation recommends Ollama for local model profiles and
  OpenAI-compatible profiles for hosted providers.

Remaining:

- Add TUI status display for the active AI profile and missing or invalid AI
  configuration.
- Keep AI provider editing out of the TUI in V2 MVP.
- Validate that TermBullet still works fully offline when AI is not configured.

#### V2.2 - AI Provider Boundary

Status: MVP implemented.

Implemented foundation:

- The canonical planning agent prompt is shipped as a product asset.
- `PlanningAgentPromptLoader` reads
  `<install-dir>/agents/planning-bulletjournal-agent.md`.
- Missing or unreadable agent prompt fails before model usage can be wired.
- `BuildAiPlanningRequestUseCase` assembles agent prompt, filtered context, and
  user prompt into a model request.
- `IAiPlanningProvider` defines the provider boundary for future hosted or local
  adapters.
- `OpenAiCompatiblePlanningProvider` sends chat-completions requests to hosted
  or local OpenAI-compatible endpoints.
- Provider tests cover request mapping, bearer token handling, missing API key,
  HTTP failures, empty content, and malformed responses.
- Provider tests cover timeout/cancellation reporting.
- `AiPlanningProviderFactory` selects `openai` and `openai-compatible` profiles
  from the active profile.
- `GenerateAiPlanningDraftUseCase` assembles the model request, calls the active
  provider, parses the provider response as a structured draft, and validates it
  before returning it to callers.
- Runtime wiring is connected from Bootstrap to CLI and TUI planning flows.

#### V2.3 - Structured Planning Drafts

Status: in progress.

Implemented foundation:

- Added structured planning draft DTOs with ordered actions.
- Added parser for canonical JSON draft responses.
- Added validation for modes `new_project` and `new_weekly`.
- Added validation for allowed action types: `create_tag`, `create_task`, and
  `create_note`.
- Unsupported actions such as delete, event creation, and direct note body
  editing are rejected before any apply workflow exists.
- Project drafts must use a non-default tag and weekly drafts must use
  `default`.
- Project planning drafts must focus one non-default tag.
- Added scenario fixtures for the Java roadmap, nutrition chatbot project, and
  gym habit tracking examples.

Remaining:

- Preserve ordered user requests through grouped draft previews.
- Validate explicit collection distribution across `today`, `week`, `month`, and
  `backlog`.
- Add broader tests for missing fields, conflicting tags, unsupported
  collections, and unknown public refs.

#### V2.4 - Planning TUI

Status: in progress.

Implemented foundation:

- Implemented guided New Planning as the only current Planning workflow.
- Implemented New Planning guided workspace with topic, project tag, task
  volume, start-today, deterministic distribution, and structured draft
  generation.
- Implemented Draft Actions shell with Apply and Discard actions visible.
- Wired TUI prompt submission to the AI draft generation use case.
- Wired TUI apply and discard actions to the current generated draft.
- Updated Planning contextual help for the real hub/workspace behavior.

Remaining:

- Implement terminal-like chat panels with scroll support.
- Keep draft editing out of V2 MVP; users refine by continuing the conversation
  or discarding the draft.
- Verify keyboard navigation, focus, scroll behavior, and footer shortcuts.

#### V2.5 - Apply Plan Use Case

Status: in progress.

Implemented foundation:

- Added an Application use case that applies approved structured drafts.
- The apply flow validates the draft before persisting any action.
- The apply flow uses existing tag creation, item creation, movement, priority,
  and cancellation use cases.
- Draft actions are applied in order and applied refs are returned in the result
  summary.
- Tests cover project creation actions, review mutations, and invalid drafts
  being rejected before persistence.
- Tests cover unknown public refs for review mutations.
- Apply intentionally uses validation plus ordered execution instead of
  transactional rollback; users can recover through readable JSON/backups if
  manual repair is needed during alpha.

Remaining:

- Append `ai_plan_applied` history plus normal per-item history events where
  needed.
- Add stronger preflight validation for mid-apply failures when new action types
  are added.

#### Future - Plan Review

Status: deferred.

Reviewing existing plans is a future idea, not part of the current Planning
scope. It is intentionally deferred because the current workflow is optimized
for small local models, and those models do not handle broad historical review
reliably enough yet.

#### V2.7 - CLI Support

Status: MVP implemented.

Implemented foundation:

- Added `termbullet ai plan <mode> --prompt ...` to generate a validated draft
  preview through the active AI profile.
- The real CLI wiring loads `<data_root>/.aiconf`, the active AI provider,
  and `<install-dir>/agents/planning-bulletjournal-agent.md`.
- `ai plan` previews the structured draft by default.
- `ai plan --apply --yes` applies the validated draft through the Application
  apply use case.
- `ai plan --apply` prompts for interactive confirmation before applying.
- Added `termbullet ai chat` with line-based prompts, `/mode`, `/apply`,
  `/discard`, and `/exit`.
- `ai chat` generates validated draft previews and applies only after
  interactive confirmation.
- `termbullet` with no command still opens the TUI.
- CLI parsing tests cover the implemented `.aiconf`, `test-ai`, `set-ai`, plan,
  and chat paths.

Remaining:

- Add optional profile switching for CLI chat after the active-profile workflow
  is stable.

#### V2.8 - Release Readiness

- Run `dotnet restore`, `dotnet build`, and `dotnet test`.
- Run manual TUI smoke tests for guided New Planning, draft approval, and AI
  unavailable states.
- Update README, release notes, and deployment assets.
- Publish V2 only after local-first non-AI flows remain unaffected.

### V3 - Google Calendar

- Optional Google Calendar integration.
- Read daily calendar events.
- Show schedule context in the TUI.
- Create events from TermBullet when explicitly requested.

### V4 - Sync + Cloud

- Optional authentication and cloud sync.
- Push/pull synchronization.
- Whole-file monthly JSON synchronization.
- Conflict handling and sync history.
- Optional PostgreSQL backend storing the same JSON file content.

## Distribution

- Homebrew.
- Scoop.
- Winget.
- Chocolatey.
- Release automation.
