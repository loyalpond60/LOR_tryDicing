# Codex Project Entry

This repository is a Library of Ruina mod workspace. The main job of Codex in
this project is architecture management, implementation support, and workflow
acceleration.

## Start Here

Read these first for most tasks:

```text
docs/codex_entry.md
docs/project_status.md
docs/README.md
```

Then read only the task-specific files listed in `docs/codex_entry.md`.

## Context Budget Rules

Do not scan the whole workspace by default. The repository contains large
generated and vendor areas that should be treated as lookup data.

Avoid broad reads of:

```text
generated/decompiled/
generated/static_knowledge/*.json
tools/dnSpy-netframework/
tools/dnSpy-netframework.zip
src/**/bin/
src/**/obj/
logs/
```

Use summaries and indexes first:

```text
generated/static_knowledge/summary.md
generated/static_knowledge/card_effect_summary.md
generated/static_knowledge/ability_effect_summary.md
docs/architecture/static_knowledge_base.md
```

Only open raw JSON, decompiled source, dnSpy files, or logs when a task
explicitly needs them.

## Preferred Search Pattern

Use targeted `rg` searches. Examples:

```powershell
rg "TacticalPlanner" src docs
rg "BattleUnitModel" generated/decompiled/Assembly-CSharp/Assembly-CSharp
rg "ERROR|WARN|tryDicing" logs/tryDicing.log
```

For a quick project inventory:

```powershell
powershell -ExecutionPolicy Bypass -File tools/context_inventory.ps1
```

For a repeatable context optimization test:

```powershell
powershell -ExecutionPolicy Bypass -File tools/test_context_budget.ps1
```

## Current Architecture Boundary

Keep these boundaries stable unless the task is explicitly architectural:

```text
AutoPlay reads battle state, enumerates legal actions, evaluates candidates,
chooses a plan, and applies validated card/target assignments.

AutoBattle advances battle phases and should not own strategy decisions.

ActionExecutor is the write boundary for final card assignment.

Agent / MCP integration should stay behind a DecisionProvider-style boundary
until local deterministic plan data is stable.
```

## Editing Expectations

Keep generated or packaged output separate from source changes. Source lives in:

```text
src/tryDicing/
```

Packaged mod output lives in:

```text
mod/tryDicing/
```

Large extracted reference data lives in:

```text
generated/
```

When a large reference artifact is added, add it to `.rgignore` or document why
it should remain searchable by default.

Project-specific Codex skills and agent-facing workflow helpers should stay in
the project unless the user explicitly asks to promote them globally.

Preferred project-local location:

```text
.codex/project-skills/
```

Do not create or update global skills under `C:\Users\User\.codex\skills` for
Library of Ruina-specific workflows unless the user explicitly approves that.
