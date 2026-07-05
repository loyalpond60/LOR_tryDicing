# Codex Entry

Purpose: keep architecture and workflow discussion fast as the project grows.

This is the first file to read when returning to the project. It tells Codex and
humans where the stable truth lives, what can be ignored at first, and which
documents to open for each kind of task.

## Fast Project Map

```text
src/tryDicing/
  Mod source. This is the main implementation area.

src/tryDicing/AutoPlay/
  Reads battle state, finds legal actions, scores local actions, builds and
  executes selected actions.

src/tryDicing/AutoBattle/
  Advances combat phases and keeps battle flow moving.

mod/tryDicing/
  Packaged mod layout copied into the game by the user.

docs/
  Stable project knowledge, architecture, strategy, and workflow notes.

generated/static_knowledge/
  Exported summaries and JSON lookup data from original game static info.

generated/decompiled/
  Decompiled game source. Treat as reference lookup, not normal reading.

tools/static_info_exporter/
  Python exporters for static game knowledge.

tools/dnSpy-netframework/
  Local decompiler tool. Vendor/tooling area; do not scan by default.
```

## Default Reading Order

For most work:

```text
docs/project_status.md
docs/architecture/mod_architecture.md
docs/architecture/autoplay_pipeline.md
docs/architecture/auto_battle_flow.md
```

For strategy/evaluator work:

```text
docs/strategy/strategy_overview.md
docs/strategy/key_terms.md
docs/strategy/lor_battle_model.md
docs/strategy/local_action_evaluation.md
docs/strategy/plan_evaluation.md
docs/strategy/implementation_versions.md
```

For static knowledge or effect tagging:

```text
docs/architecture/static_knowledge_base.md
generated/static_knowledge/summary.md
generated/static_knowledge/card_effect_summary.md
generated/static_knowledge/ability_effect_summary.md
```

For future agent/MCP work:

```text
docs/agent/decision_provider.md
docs/agent/mcp_integration.md
```

For original API lookup:

```text
docs/lor_autoplay_original_api.md
```

For exploratory history only:

```text
docs/notes/battle_strategy_brainstorm.md
```

Do not read the brainstorm file unless a task asks for historical ideation.

## Task Routing

Use this table to pick the smallest useful context.

```text
Task: Fix or extend action choice
Read: project_status, mod_architecture, autoplay_pipeline, relevant AutoPlay cs files

Task: Fix or extend battle phase automation
Read: project_status, auto_battle_flow, relevant AutoBattle cs files

Task: Change scoring rules
Read: strategy_overview, local_action_evaluation, plan_evaluation,
      LocalActionEvaluator.cs, TacticalPlanner.cs

Task: Add whole-scene planning
Read: strategy_overview, victory_exchange_model, plan_evaluation,
      BattlePlan.cs, TacticalPlanner.cs, BattlePlanExecutor.cs

Task: Export or use static knowledge
Read: static_knowledge_base, static summaries, exporter scripts

Task: Investigate original game behavior
Read: lor_autoplay_original_api, then targeted decompiled files

Task: Prepare agent integration
Read: decision_provider, mcp_integration, implementation_versions
```

## Context Rules

Default to summaries before raw data.

Avoid broad commands over the whole workspace. Prefer:

```powershell
rg "SearchTerm" src docs
rg "SearchTerm" tools/static_info_exporter generated/static_knowledge/*.md
rg "OriginalGameType" generated/decompiled/Assembly-CSharp/Assembly-CSharp
```

Do not open these without a specific reason:

```text
generated/decompiled/
generated/static_knowledge/*.json
tools/dnSpy-netframework/
src/**/bin/
src/**/obj/
logs/
```

## Context Smoke Test

Codex can verify the optimization with:

```powershell
powershell -ExecutionPolicy Bypass -File tools/test_context_budget.ps1
```

This checks that the default searchable file set stays small, heavy paths remain
hidden, and the context inventory still works.

## Architecture Direction

Near-term work should keep the current V1 local deterministic pipeline stable:

```text
BattleSnapshotReader
LegalActionFinder
LocalActionEvaluator
TacticalPlanner
BattlePlanExecutor
ActionExecutor
```

Next likely architectural growth:

```text
CandidateBattlePlanGenerator
PlanEvaluator
DecisionProvider
```

The external agent should receive compressed candidate-plan data, not raw battle
dumps or full static knowledge.

## Maintenance Rule

When a task changes project shape, update one of these before ending the work:

```text
docs/project_status.md
docs/codex_entry.md
docs/architecture/*.md
docs/strategy/*.md
```

When a large reference artifact is added, add it to `.rgignore` or document why
it should remain searchable by default.

When adding a new document, decide whether it is:

```text
Current status:
  docs/project_status.md

Stable architecture:
  docs/architecture/*.md

Strategy model or roadmap:
  docs/strategy/*.md

Agent integration boundary:
  docs/agent/*.md

Exploration or long-form history:
  docs/notes/*.md

Project-local Codex workflow:
  .codex/project-skills/*/SKILL.md

Executable workflow:
  tools/
```

If a new document becomes part of normal Codex routing, add it to the relevant
reading order or task route in this file. If it is only background, keep it out
of the default route and link to it from the stable summary.

Prefer tools over documents when the task is repeated data lookup, export,
validation, or log inspection.
