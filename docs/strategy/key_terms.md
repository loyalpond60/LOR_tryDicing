# Key Terms

This document fixes the core relationship between strategy terms.

It exists to keep `objective`, `plan`, `resource`, and `score` from becoming vague.

## High-Level Chain

```text
BuildProfile
  -> BattleContext
  -> AvailableResources
  -> ActionCandidate
  -> LocalActionEvaluation
  -> FeasibleOutcome
  -> SceneObjectiveHypothesis
  -> CandidateBattlePlan
  -> ResourceExchangeEstimate
  -> IrreversibleGainEstimate
  -> PlanEvaluation
  -> AgentDecision
```

## BuildProfile

BuildProfile describes what each player-built character is intended to do.

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

## FeasibleOutcome

FeasibleOutcome describes what the current resources and evaluated actions make realistically possible this scene.

It is evidence, not a command.

Examples:

```text
Enemy A can probably be staggered.
Enemy B can probably be killed.
A dangerous enemy die can be intercepted.
The team can recover resources safely.
A build-specific mechanic can be advanced.
An enemy buff or status die can be prevented.
No safe kill is available this scene.
No safe stagger is available this scene.
```

Important rule:

```text
Objectives must be derived from feasible outcomes, not from abstract wishes.
```

## SceneObjectiveHypothesis

SceneObjectiveHypothesis is a hypothesis about what the current scene may try to accomplish.

It should be derived from FeasibleOutcomes.

It may propose:

```text
Survive a dangerous scene.
Stagger a specific enemy.
Kill a specific enemy.
Recover light or cards.
Set up a build-specific mechanic.
Spend resources for a payoff window.
Prevent an enemy mechanic from advancing.
```

If later plan evaluation shows the objective is unsafe, unrealistic, or too expensive, the objective should be revised, downgraded, or rejected.

## CandidateBattlePlan

CandidateBattlePlan is a full set of selected player actions generated under a SceneObjectiveHypothesis. CandidateBattlePlan is also the scene-level exchange proposal for the current scene.

It answers:

```text
If this is what the scene is trying to accomplish,
which actions should each available speed die take?
```

The same BattleContext may produce different plans for different objective hypotheses.

## ResourceExchangeEstimate

ResourceExchangeEstimate describes what a plan spends, risks, gains, and preserves.

It may describe:

```text
HP and stagger risk.
Light spent or recovered.
Cards spent or drawn.
Speed dice and action opportunities used.
Enemy HP damage.
Enemy stagger pressure.
Enemy action suppression.
Future hand and light stability.
```

It exists so plan evaluation can reason about exchange quality instead of only local
score or immediate damage.

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

## PlanEvaluation

PlanEvaluation evaluates a CandidateBattlePlan under its SceneObjectiveHypothesis.

It answers:

```text
Does this plan actually support the objective it was built for?
Are the costs and risks acceptable?
What happens if the plan fails?
```

PlanEvaluation evaluates the objective-plan pair, not the plan alone.

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

## Objective And Plan Relationship

Objective and plan should be treated as a pair.

```text
SceneObjectiveHypothesis proposes what the scene may try to do.
CandidateBattlePlan proposes how to do it.
PlanEvaluation checks whether that objective-plan pair is actually coherent.
AgentDecision chooses one evaluated objective-plan pair.
```

Important rule:

```text
The objective helps generate the plan.
The plan evaluation validates or rejects the objective.
The objective itself must be grounded in feasible outcomes.
```



