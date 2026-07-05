# tryDicing Documentation

This directory is split by purpose.

Use this file as the entry point when returning to the project after a break.

For Codex context management and faster architecture work, start with:

```text
codex_entry.md
codex_context_test.md
```

## Current Reading Order

For current project state:

```text
codex_entry.md
project_status.md
workflow.md
```

For original Library of Ruina API notes:

```text
lor_autoplay_original_api.md
```

For implemented mod structure:

```text
architecture/mod_architecture.md
architecture/autoplay_pipeline.md
architecture/auto_battle_flow.md
architecture/raw_battle_data_inventory.md
architecture/static_knowledge_base.md
```

For strategy design:

```text
strategy/strategy_overview.md
strategy/victory_exchange_model.md
strategy/lor_battle_model.md
strategy/key_terms.md
strategy/implementation_versions.md
strategy/build_profile.md
strategy/local_action_evaluation.md
strategy/plan_evaluation.md
```

For future agent / MCP integration:

```text
agent/mcp_integration.md
agent/decision_provider.md
```

For unfinished ideas and long-form brainstorming:

```text
notes/battle_strategy_brainstorm.md
```

## Document Roles

Stable documents should describe decisions we are willing to implement.

Brainstorm documents may contain incomplete ideas, rejected alternatives, and discussion history.

When a brainstorm idea becomes stable, move the distilled version into the appropriate architecture, strategy, or agent document instead of making the brainstorm document the source of truth.

## Document Governance

Before adding a new document, classify it:

```text
README.md:
  Documentation map and document governance.

codex_entry.md:
  Codex task routing, context budget, and project-local workflow entry.

project_status.md:
  Current state, verified behavior, constraints, and next likely step.

architecture/:
  Stable implementation architecture and accepted design boundaries.

strategy/:
  Combat strategy model, evaluator concepts, and implementation roadmap.

agent/:
  Future DecisionProvider, MCP, and external-agent integration boundaries.

notes/:
  Brainstorming, rejected ideas, long discussion history, and unfinished design.

.codex/project-skills/:
  Project-local Codex skills. These are not global personal skills.

tools/:
  Executable scripts, query tools, exporters, and verification helpers.
```

Keep stable documents short enough to be read as working context. If a topic
needs long explanation, put the long form in `notes/` or a referenced resource
and keep the stable document as the distilled source of truth.

When adding or moving a document, update `codex_entry.md` if Codex should know
when to read it.

