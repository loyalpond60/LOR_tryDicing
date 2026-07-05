# Decision Provider

DecisionProvider is the planned boundary for choosing a final battle plan.

It lets the project support local deterministic decisions first, then external MCP-backed agent decisions later.

## Purpose

DecisionProvider should choose among already generated and evaluated options in
the safest mode.

Later modes may let an agent request new variants or propose a plan, but agent
output remains advisory until the program validates and evaluates it.

It should not:

```text
Read raw game objects directly.
Invent illegal raw actions.
Bypass validation.
Execute game-state writes.
```

DecisionProvider should treat plan choice as a constrained tactical judgment about
resource exchange and victory paths.

It may prefer a lower immediate score when that plan creates irreversible gain,
prevents irreversible loss, or avoids a boss-mechanic trap that ordinary scoring
underestimates.

## Input

First-version input should be a compact decision request:

```text
Battle summary.
Build summaries.
Available resource summary.
Feasible outcomes.
Candidate objective-plan pairs.
Plan evaluation summaries.
Resource exchange summaries.
Irreversible gain and irreversible loss summaries.
Known risks.
Timeout configuration.
Fallback preference.
```

## Output

First-version output for Mode 1 Select:

```text
selectedPlanId
selectedObjectiveId
reason
knownRisks
fallbackPreference
```

Later mode-specific output may include:

```text
variantRequest
proposedPlan
partialActionProposal
```

Every output must be validated by the program before execution.

## Agent Control Modes

External agent freedom should be controlled by an explicit mode.

Higher modes are not automatically better. They are useful only when the program
has enough generators, validators, and evaluators to handle the extra freedom.

### Mode 1: Select

Scope:

```text
Selection among already generated and evaluated plans.
```

Allowed:

```text
Select one evaluated objective-plan pair.
Explain the reason.
Report known risks.
Choose a fallback preference when supported.
```

Not allowed:

```text
Change actions.
Invent targets.
Request unlisted card assignments.
Bypass plan evaluation.
```

Program responsibility:

```text
Validate that the selected plan id and objective id exist.
Recheck legality before execution.
Execute only the validated selected plan.
```

Mode 1 is the default and first external-agent integration target.

### Mode 2: Request Variant

Scope:

```text
Agent-guided plan generation without direct action editing.
```

Allowed:

```text
Request that the program generate a variant in a specific direction.
Ask for a safer plan.
Ask for more focus fire.
Ask for a resource recovery line.
Ask for a threat-interception line.
Ask to preserve a key card or conserve light.
Ask to seek a kill, stagger, mechanic stop, or setup line.
```

Not allowed:

```text
Directly change an action.
Directly assign a card.
Directly choose a target slot for an action.
Submit an executable plan.
```

Program responsibility:

```text
Interpret the variant request as constraints or generation hints.
Generate new candidate plans.
Evaluate the new objective-plan pairs.
Return evaluated variants for another decision step.
Execute only a later validated selected plan.
```

Mode 2 is useful when the evaluated options are too narrow but the program can
still own all legal action generation.

### Mode 3: Propose Plan

Scope:

```text
Agent proposal of a concrete plan or partial actions.
```

Allowed:

```text
Propose a full plan.
Propose partial action assignments.
Suggest replacing or reserving specific actions.
Explain the intended exchange and risk.
```

Not allowed:

```text
Directly execute the proposed plan.
Skip legality checks.
Skip local and plan evaluation.
Force the program to accept illegal or unevaluated actions.
```

Program responsibility:

```text
Resolve proposed references to current game objects.
Reject illegal or ambiguous actions.
Fill missing actions only through program-owned generation.
Re-evaluate accepted actions and the resulting plan.
Compare the re-evaluated proposal against other candidates.
Execute only a validated, re-evaluated plan.
```

Mode 3 should wait until validation, proposal parsing, and re-evaluation are
strong enough to keep agent output advisory rather than authoritative.

## MockAgentDecisionProvider

Scope:

```text
Decision-provider interface testing.
```

Subject:

```text
A compact decision request.
```

Object:

```text
One evaluated candidate plan.
```

Effect:

```text
Returns a predictable plan choice so request, response, validation, and execution can be tested.
```

MockAgentDecisionProvider is not meant to be intelligent.

It may choose:

```text
The first plan.
The highest-score plan.
A fixed plan id for test scenarios.
```

## RuleAgentDecisionProvider

Scope:

```text
Local deterministic battle-plan selection.
```

Subject:

```text
Evaluated objective-plan pairs.
```

Object:

```text
The locally preferred objective-plan pair.
```

Effect:

```text
Selects a reasonable plan without using an external agent.
```

RuleAgentDecisionProvider is useful as:

```text
A baseline.
A fallback.
A comparison point for external agent choices.
```

## ExternalAgentDecisionProvider

Scope:

```text
External MCP-backed tactical decision making.
```

Subject:

```text
Compressed battle and plan summaries exposed through a Local Decision Bridge.
```

Object:

```text
An agent-selected evaluated plan, variant request, or plan proposal depending on mode.
```

Effect:

```text
Lets an external agent participate under the configured control mode without
directly controlling the game or receiving full raw state by default.
```

The external agent is not the intelligence itself. The intelligence is the
victory-oriented exchange process. The external agent helps judge high-level tradeoffs
when evaluated plans or requested variants represent genuinely different future
paths.

## Validation Rule

DecisionProvider output is advisory until validated.

For selected plans, the program must check:

```text
The selected plan id exists.
The selected objective id exists.
All actions are still legal.
All cards are still available.
All target units and speed dice are still valid.
No action violates hard safety rules.
```

For Mode 2, the program must also check:

```text
The variant request maps to supported generator hints or constraints.
Generated variants pass normal plan evaluation before being shown or selected.
```

For Mode 3, the program must also check:

```text
Every proposed action maps to current legal game state.
Every accepted proposed action receives local action evaluation.
The resulting plan receives full plan evaluation.
Invalid proposed actions are rejected instead of repaired silently.
```

Only a validated plan can be executed.



