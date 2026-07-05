# Library of Ruina Battle Model

This document defines the project-specific abstraction for Library of Ruina combat.

It is not an API reference, an external AI paper summary, or a scoring formula catalog.
It is the shared model that strategy code, future agent integration, and later learning
systems should speak.

## Purpose

The strategy system should model Library of Ruina on its own terms:

```text
Speed dice.
Battle pages.
Clashes.
One-sided attacks.
Light.
Hand and deck resources.
Stagger pressure.
Visible enemy actions.
Passives, status effects, and page scripts.
```

It should not start from generic RPG roles or directly copy another card-game model.

The purpose of this document is to fix the core shape:

```text
State -> resources -> legal actions -> exchange-aware plans -> evaluation -> validated execution
```

## Core View

Library of Ruina combat can be treated as a scene-based tactical planning problem.

Each scene, the player side assigns battle pages to speed dice under card, light,
target, speed, and clash constraints.

The strategy problem is not only:

```text
Which card should this one speed die use?
```

It is:

```text
Given the current state and available resources,
how should all available player speed dice be assigned this scene?
```

At the strategic level, combat is a resource-exchange problem.

The player side spends or risks HP, stagger margin, light, cards, speed dice, status
windows, and future action quality to reduce enemy HP, stagger, action quality,
mechanic progress, and future threat.

The model should care most about exchanges that change future winning chances, not
only exchanges that look numerically good in the current scene.

## State: S_t

`S_t` means the visible battle state at the beginning of scene `t`, before player
card assignment is finalized.

`S_t` may include:

```text
Scene state:
  Current scene number.
  Current battle phase.
  Which side is being planned for.

Allied unit state:
  HP.
  Stagger gauge.
  Light.
  Hand.
  Deck / discard later.
  Speed dice and speed values.
  Already assigned cards.
  Buffs, debuffs, passives, and build profile later.

Enemy unit state:
  HP.
  Stagger gauge.
  Speed dice and speed values.
  Visible selected cards.
  Visible targets.
  Buffs, debuffs, passives, and special mechanics later.

Resource state:
  Usable speed dice.
  Cards in hand.
  Remaining light.
  Already reserved light.
  Key pages or resources that should be preserved later.
```

Current code mapping:

```text
BattleSnapshotReader -> BattleSnapshot
```

Current implementation reads only a subset of this state. The model is broader than
the current code, but code should grow toward this model incrementally.

## Action: a

A single speed-dice action is:

```text
a = (actor, actorSpeedDie, card, target, targetSpeedDie)
```

Current code mapping:

```text
ActionCandidate
```

ActionCandidate

Scope:

```text
One possible assignment for one player speed die.
```

Subject:

```text
One acting player unit and one of its speed dice.
```

Object:

```text
One battle page, one target unit, and one target speed die or target slot.
```

Effect:

```text
If selected and executed, the acting speed die uses the card against the target slot.
```

## Legal Actions: A_d(S_t)

`A_d(S_t)` means all legal actions for one player speed die `d` in state `S_t`.

Legal action generation should answer:

```text
What can this speed die actually do right now?
```

It should not answer:

```text
Is this action strategically good?
```

Current code mapping:

```text
LegalActionFinder.Find(...)
  -> LegalActionSearchResult
       -> Candidates
       -> Report
```

This boundary matters:

```text
LegalActionFinder:
  Checks whether actions can exist.

LocalActionEvaluator:
  Scores whether actions are useful.
```

## Plan: P_t

A scene plan is a set of selected actions for the player side:

```text
P_t = { a_1, a_2, ..., a_n }
```

More precisely, it is a mapping:

```text
P_t:
  player speed die -> selected legal action
```

A valid plan must satisfy cross-action constraints:

```text
One speed die can receive at most one card.
One hand card instance cannot be used twice.
Total card cost for each actor must fit available light.
Targets and target slots must remain valid.
Special page types must follow their own rules.
```

A plan is also an exchange proposal.

It answers:

```text
What resources does the player side spend or risk this scene?
What enemy resources, actions, thresholds, or future threats does it remove?
What future action ability does it preserve?
Which gains are irreversible, semi-irreversible, or merely temporary?
```

Current code mapping:

```text
BattlePlan
TacticalPlanner
```

Current implementation status:

```text
The current TacticalPlanner builds a scene-level plan by choosing the best local
action for each usable speed die.

It does not yet perform full combination search across all possible scene plans.
```

In other words:

```text
Current version = local greedy planning.
Future version = scene-level plan optimization.
```

## Resources Before Objectives

Objectives should not be invented before checking what the current scene can support.

The order should be:

```text
Battle state
  -> available resources
  -> legal actions
  -> feasible plans or feasible outcomes
  -> objective hypothesis
  -> plan selection
```

This prevents the system from saying:

```text
Kill the boss now.
Win every clash.
Spend the strongest card.
```

when the current hand, light, speed, or target state cannot support that objective.

## Objective Hypothesis

`SceneObjectiveHypothesis` is not a hard command.

It is a temporary explanation of what a feasible plan may be trying to accomplish.

Examples:

```text
Confirm a kill.
Confirm a stagger.
Reduce incoming damage.
Intercept a dangerous enemy die.
Recover light or cards.
Advance a build-specific setup.
Accept damage to preserve resources.
Avoid waste while waiting for a better scene.
```

Important rule:

```text
Objective hypotheses must be grounded in feasible outcomes and available resources.
```

Objective and plan form a pair:

```text
SceneObjectiveHypothesis:
  What this scene might try to accomplish.

CandidateBattlePlan:
  How this scene attempts to accomplish it.

PlanEvaluation:
  Whether that objective-plan pair is coherent, affordable, and acceptable.
```

If evaluation shows that an objective is unsafe, unrealistic, or too expensive, the
objective should be revised, downgraded, or rejected.

## Evaluation

Local action value:

```text
v(a | S_t)
```

Question:

```text
How useful is this one action in the current state?
```

Current code mapping:

```text
LocalActionEvaluator -> LocalActionEvaluation
```

Plan value:

```text
V(P_t | S_t)
```

Question:

```text
How good is this whole scene plan in the current state?
Does this plan make a favorable exchange, create irreversible gain, or prevent irreversible loss?
```

Planned code mapping:

```text
PlanEvaluator -> PlanEvaluation
```

Important warning:

```text
The best local action for each speed die does not guarantee the best scene plan.
The best scene plan does not guarantee the battle will be won.
```

This is why the project separates local evaluation, plan evaluation, and future
battle-trajectory evaluation.

## Resolution And Uncertainty

Library of Ruina combat includes dice rolls and many conditional effects.

Conceptually:

```text
R(S_t, P_t) -> possible S_(t+1) outcomes
```

The same plan may produce different next states because clashes and dice values are
uncertain.

Current version:

```text
No full forward simulator.
No exact passive simulation.
No exact card-script simulation.
No exact clash probability model.
```

Future versions may estimate:

```text
Clash win probability.
Expected HP damage.
Expected stagger damage.
Expected incoming damage prevented.
Expected light / hand state after the scene.
Probability that a kill or stagger is confirmed.
Estimating irreversible gain and irreversible loss prevention.
Detecting when boss mechanics distort ordinary exchange value.
```

## Existing Code Mapping

Core model mapping:

```text
S_t:
  BattleSnapshot

a:
  ActionCandidate

A_d(S_t):
  LegalActionFinder.Find(...)

Search diagnostics:
  LegalActionSearchReport

v(a | S_t):
  LocalActionEvaluation

P_t:
  BattlePlan

Plan construction:
  TacticalPlanner

V(P_t | S_t):
  Planned PlanEvaluation layer

Final choice:
  Planned DecisionProvider / AgentDecision layer

Execution:
  ActionExecutor
```

Current pipeline:

```text
BattleSnapshotReader
  -> BattleSnapshot
  -> TacticalPlanner
       -> LegalActionFinder
            -> LegalActionSearchResult
                 -> Candidates
                 -> Report
       -> LocalActionEvaluator
  -> BattlePlan
  -> BattlePlanExecutor
  -> ActionExecutor
```

## Responsibility Boundary

The program should own:

```text
Reading raw game objects.
Compressing state into model structures.
Generating legal actions.
Validating legality.
Applying selected actions to the game.
Logging pipeline reports.
```

Local rules, future agents, or future neural models may help with:

```text
Choosing among evaluated plans.
Recognizing strategic exceptions.
Balancing short-term safety and long-term resource flow.
Explaining why one feasible plan is preferred over another.
```

They should not directly write game state or invent unvalidated actions.




