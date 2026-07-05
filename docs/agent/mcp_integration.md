# MCP Integration

This document describes the planned external-agent integration.

The game mod should not directly control Codex or any chat interface.

Instead:

```text
Library of Ruina Mod
  -> Local Decision Bridge
  -> MCP Server
  -> Agent / Codex thread
```

Decision flow back:

```text
Agent / Codex thread
  -> MCP Server
  -> Local Decision Bridge
  -> Library of Ruina Mod
```

## Core Rule

```text
The mod reads and validates game state.
The bridge exposes focused decision data.
The MCP server exposes tools.
The agent acts within the configured control mode.
The mod validates and executes the selected plan.
```

Agent control modes:

```text
Mode 1 Select:
  Agent chooses one evaluated plan.

Mode 2 Request Variant:
  Agent requests a program-generated variant direction.

Mode 3 Propose Plan:
  Agent proposes a concrete plan or partial actions for validation and re-evaluation.
```

## Library of Ruina Mod Responsibilities

The mod should:

```text
Read current battle state.
Produce or request compressed decision data.
Pause decision progress during ApplyLibrarianCardPhase when external decision is enabled.
Send a decision request to the Local Decision Bridge.
Receive a selected plan or timeout/fallback result.
Validate that the selected plan is still legal.
Execute the validated plan in-game.
```

The mod should not:

```text
Open a Codex thread.
Control a Codex UI.
Depend on a specific chat window.
Send full raw game object dumps to the agent by default.
Execute unvalidated agent output.
```

## Local Decision Bridge Responsibilities

The bridge should:

```text
Hold the current decision request.
Hold compressed battle summaries.
Hold feasible outcomes.
Hold candidate objective-plan pairs.
Hold plan evaluations.
Hold the configured agent control mode.
Expose local HTTP or WebSocket endpoints for the mod.
Accept agent plan choices.
Accept supported variant requests.
Accept proposed plans or partial actions when Mode 3 is enabled.
Return selected plans to the mod.
Handle timeouts.
Trigger fallback providers when needed.
```

The bridge is the boundary between the Unity mod and external agent tooling.

## MCP Server Responsibilities

The MCP server should wrap the bridge as tools.

It should not directly read game memory or modify game files.

First-version Mode 1 tools:

```text
get_battle_summary()
list_feasible_outcomes()
list_candidate_plans()
inspect_plan(planId)
submit_plan_choice(planId, reason)
```

Possible Mode 2 tools:

```text
request_plan_variant(direction, constraints, reason)
list_variant_requests()
inspect_variant_result(requestId)
```

Possible Mode 3 tools:

```text
inspect_unit(unitId)
inspect_action(actionId)
compare_plans(planIdA, planIdB)
submit_plan_proposal(proposal, reason)
submit_partial_action_proposal(actions, reason)
```

Mode 2 and Mode 3 outputs are not executable by themselves. The program must
generate, validate, and evaluate the resulting plans before they can be selected.

## Token Control

Default agent context should include:

```text
Battle summary.
Build summaries.
Feasible outcomes.
Top candidate plans.
Plan evaluation summaries.
Known risks.
```

Default agent context should not include:

```text
Raw BattleContext.
Full card XML.
Full passive XML.
Full candidate action list.
Full dice-by-dice detail for every possible action.
Large unfiltered logs.
```

If the agent needs more detail, it should request focused information through MCP tools.

## Timeout And Fallback

External agent decision making must be optional and recoverable.

Fallback cases:

```text
Agent does not respond before timeout.
Bridge is unavailable.
MCP server is unavailable.
Agent returns an unknown planId.
Agent returns an illegal plan choice, variant request, or proposal.
Agent requests an unsupported variant.
Agent proposes illegal or ambiguous actions.
Program validation fails.
```

Fallback order:

```text
RuleAgentDecisionProvider.
Highest PlanEvaluation candidate.
Current local rule-based strategy.
Vanilla auto play as last resort.
```
