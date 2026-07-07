# Key Terms

This document fixes the core relationship between strategy terms.

It exists to keep `objective`, `plan`, `resource`, and `score` from becoming vague.

## High-Level Chain

```text
BattleSnapshot
  -> PlayerAvailableResources
  -> ThreatAssessment
  -> ActionCandidate
  -> ThreatResponseMatrix
  -> PlanSearch
  -> PlanEvaluation
  -> BattlePlan
  -> DecisionProvider
```

This is a working strategy spine, not a promise that every design term becomes a class.


## BuildProfile

BuildProfile describes what each player-built character is intended to do. It is a delayed optional concept until V3 Build Intent / User Intent.

It is based on:

```text
Key page.
Passives.
Deck.
Player-provided build intent.
Current combat resources when relevant.
```

It affects how actions and plans are interpreted.

It does not directly choose actions.

BuildProfile may later use keyword-based inference, but keyword extraction should start as helper logic or fields on BuildProfile rather than independent architecture layers.

Examples:

```text
copy + cost reduction:
  Copy actions may create setup future value.

draw + light:
  Resource recovery may have higher build fit.

burn / bleed / smoke:
  Status application may be proactive value.
```

## BattleContext

BattleContext describes the current objective battle state.

It includes:

```text
Current scene.
All allied units.
All enemy units.
Speed dice.
Cards in hand.
Already assigned cards.
HP and stagger state.
Light and hand state.
Buffs, debuffs, and visible enemy actions.
```

BattleContext is raw situation data read from the game.

## AvailableResources

AvailableResources describes what the player side can actually use in the current scene.

It is derived from BattleContext.

It includes:

```text
Usable speed dice.
Speed values.
Current light.
Cards in hand.
Playable cards.
Available clash opportunities.
Available one-sided attack opportunities.
Available intercept opportunities.
Available resource recovery actions.
Available draw actions.
Available setup actions.
Key pages currently available or unavailable.
```

AvailableResources prevents the strategy from proposing objectives that the current hand, light, speed dice, or targeting state cannot support.

## ThreatAssessment

ThreatAssessment describes a visible enemy proposal for future loss.

It may describe:

```text
HP damage threat.
Stagger threat.
Death threat.
Status threat.
Enemy buff or resource gain.
Boss mechanic progress.
Future tempo loss.
```

Important rule:

```text
A threat is not a command to defend.
It is an exchange the enemy is trying to make.
```

The strategy may fully answer, partially answer, ignore, or accept a threat when
another exchange is better for victory.

## ActionCandidate

ActionCandidate represents one possible action for one player speed die.

It describes:

```text
Actor.
Actor speed die.
Selected battle page.
Target unit.
Target speed die when applicable.
Interaction type.
Intent tags.
```

ActionCandidate may still need legality checks.

## LocalActionEvaluation

LocalActionEvaluation is the evaluation report for one ActionCandidate.

It estimates:

```text
Legality.
Clash value.
Damage value.
Stagger value.
Defense value.
Resource value.
Setup value.
Enemy effect suppression value.
Waste.
Build fit.
```

It does not decide whether the full scene plan is good.

## ThreatResponseMatrix

ThreatResponseMatrix describes how each ActionCandidate responds to each
ThreatAssessment.

Formal shape:

```text
R[T, AC]
```

Meaning:

```text
R[t, c] = response value of action candidate c against threat t.
```

This is not the complete value of the action candidate.

An action may also have:

```text
damage value.
stagger value.
resource value.
setup value.
build fit.
waste.
```

Important rule:

```text
ThreatResponseMatrix is input data for plan search and evaluation.
It is not the final strategy.
```

## FeasibleOutcome

FeasibleOutcome is a delayed concept for variant generation.

It may become useful when the system needs to derive possible scene directions before generating objective-oriented plan variants. Until then, feasibility can remain inside PlanSearch or PlanEvaluation calculations.


## SceneObjectiveHypothesis

SceneObjectiveHypothesis is a delayed concept for V4 objective / feasible-outcome variants.

V2 should first search for and evaluate good selected action sets. Objective hypotheses should appear only when the system needs to generate and compare distinct objective-oriented variants.


## CandidateBattlePlan

CandidateBattlePlan is not currently required as an independent object.

For V2, a selected plan can be represented as:

```text
List<ActionCandidate>
```

Only introduce CandidateBattlePlan later if selected actions need plan ids, objective ids, validation state, serialization, evaluation cache, or other cross-step metadata.


## ResourceExchangeEstimate

ResourceExchangeEstimate should not be an independent architecture layer for now.

The exchange idea is important, but spend / risk / gain / preserve values should first live as PlanEvaluation fields or calculation details.


## PlanImpact

PlanImpact is an optional future extraction from PlanEvaluation.

PlanImpact describes the multi-dimensional strategic effect of a plan.

First-version dimensions:

```text
terminalProgress
actionEconomyChange
resourceFlowChange
riskChange
setupFutureValue
cost
waste
```

Rule:

```text
Events can be multi-impact.
Dimensions must be single-reason.
```

The same combat event may project into several dimensions when it changes
different strategic properties.

Example:

```text
Enemy death:
  terminalProgress because the enemy is removed from the win condition.
  actionEconomyChange because enemy future actions are removed.
  riskChange because enemy future threats are removed.
  resourceFlowChange because future defensive resource pressure is reduced.
```

This is not double counting if each dimension answers a different question.

## IrreversibleGainEstimate

IrreversibleGainEstimate describes whether a plan creates durable victory progress or
prevents durable loss.

It may describe:

```text
Enemy death.
Enemy action reduction.
Stagger or burst windows that can be converted.
Boss phase or mechanic progress.
Preserved allied action ability.
Prevented deaths, staggers, or mechanic failures.
```

It distinguishes true victory-path progress from temporary numerical advantage.

Current role:

```text
IrreversibleGainEstimate is a durability tag or explanation inside PlanImpact.
It should not become a separate top-level bucket that double-counts the same value.
```

## PlanEvaluation

PlanEvaluation evaluates a selected List<ActionCandidate> under the current BattleSnapshot and ThreatResponseMatrix.

It answers:

```text
Does this plan actually support the objective it was built for?
Are the costs and risks acceptable?
What happens if the plan fails?
```

When V4 objective variants exist, PlanEvaluation may also evaluate objective context. V2 should not require a separate CandidateBattlePlan or ObjectivePlanPair.

## AgentDecision

AgentDecision chooses the final evaluated objective-plan pair.

It should include:

```text
Selected objective hypothesis.
Selected battle plan.
Reason for selection.
Known risks.
Important rejected alternatives when useful.
```

The program must validate the selected plan before execution.

## Delayed Objective Relationship

Objective and plan pairing is a delayed V4 concern.

V2 should keep the core loop simple:

```text
PlanSearch proposes selected ActionCandidate sets.
PlanEvaluation judges those selected action sets.
BattlePlan converts the chosen set into executable SpeedDiceAction entries.
```

When objective variants are introduced later, objective context can be passed into PlanSearch and PlanEvaluation without immediately adding ObjectivePlanPair or objective-aware generator/evaluator classes.
