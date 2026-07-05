# Plan Evaluation

PlanEvaluation evaluates a CandidateBattlePlan under a SceneObjectiveHypothesis.

It is not only a plan score.

It answers:

```text
Is this plan a coherent and acceptable way to pursue this objective hypothesis,
given the current resources and feasible outcomes?
```

## Resource-Grounded Objective Rule

PlanEvaluation must treat the objective and plan as a pair.

Rule:

```text
A SceneObjectiveHypothesis must be grounded in AvailableResources and FeasibleOutcomes.
PlanEvaluation evaluates whether the CandidateBattlePlan supports the objective,
and whether the objective itself is realistic under current resources.
```

If the plan cannot support the objective, or the objective is unrealistic under current resources, the objective-plan pair should be downgraded or rejected.

## Exchange Rule

PlanEvaluation should treat each plan as a proposed resource exchange.

It should ask:

```text
What does the plan spend or risk?
What does it gain or preserve?
Which gains are irreversible or semi-irreversible?
Which irreversible losses does it prevent?
Does the exchange still look good if the primary payoff fails?
```

This keeps plan evaluation from collapsing into immediate damage, immediate defense,
or local clash score.

## Inputs

PlanEvaluation may use:

```text
BattleContext
AvailableResources
BuildProfiles
FeasibleOutcomes
SceneObjectiveHypothesis
CandidateBattlePlan
LocalActionEvaluations
```

## Output Shape

First-version components:

```text
exchangeQuality
irreversibleGain
objectiveFit
outcomeConfirmation
threatCoverage
allyRiskManagement
resourceFuture
buildIntentCoherence
setupPayoffTiming
redundancyWaste
failureRisk
totalScore
explanation
```
`totalScore` is useful for ranking, but it should not replace the structured report.

## exchangeQuality

Estimates whether the plan creates a favorable resource exchange.

It may include:

```text
resourcesSpent
resourcesRisked
resourcesGained
resourcesPreserved
futureActionAbilityChange
exchangeEfficiency
```

The question is not only whether the plan deals damage or prevents damage. The
question is whether the plan improves the player's path to victory relative to its
cost.

## irreversibleGain

Estimates whether the plan changes the future battle structure in a durable way.

It may include:

```text
enemyDeaths
enemyActionReduction
confirmedStaggerWindows
bossPhaseProgress
bossMechanicSuppression
futureThreatsRemoved
irreversibleLossesPrevented
```

Not every gain is permanent. Semi-irreversible gains, such as stagger or a temporary
burst window, should be valued according to whether the plan or next scene can convert
them into lasting advantage.

## objectiveFit

Estimates whether the plan is actually trying to accomplish its SceneObjectiveHypothesis.

Example:

```text
Objective:
  Recover resources.

High objectiveFit:
  Several actions recover light or draw cards while keeping risk acceptable.

Low objectiveFit:
  Most actions spend resources aggressively and do not improve next-scene stability.
```

## outcomeConfirmation

Estimates whether the plan has enough chance to secure the outcome it claims to pursue.

It may include:

```text
primaryOutcomeChance
secondaryOutcomeChance
confirmedOutcomes
unconfirmedClaims
```

Possible outcomes:

```text
Kill a target.
Stagger a target.
Preserve a key ally.
Prevent an enemy mechanic.
Complete player-side setup.
Safely pass a dangerous scene.
Put an enemy into a next-scene payoff state.
```

## threatCoverage

Estimates whether important enemy threats are handled.

Threats include:

```text
High HP damage.
High stagger damage.
Dangerous statuses.
Enemy buffs.
Enemy resource gain.
Enemy mechanic setup.
Special stage effects.
Boss mechanic value distortion.
```

## allyRiskManagement

Estimates whether allied risks are intentional, compensated, and acceptable.

It should not assume that all allied damage, stagger, or death is always bad.

Key question:

```text
Does the plan expose a unit to expected HP loss or stagger loss that exceeds its safe margin,
without a matching payoff or intentional trigger?
```

It may include:

```text
expectedAllyDeaths
expectedAllyStaggers
intentionalSacrifices
acceptedDamageForTrigger
unplannedCriticalRisks
protectedKeyUnits
```

## resourceFuture

Estimates whether the team remains playable next scene.

It may include:

```text
lightAfterPlan
expectedLightNextScene
handAfterPlan
expectedHandNextScene
resourceStress
keyCardSpent
resourceRecoveryGained
```

Baseline rule:

```text
Without passive, card, or emotion-level effects:
  A character recovers 1 light per scene.
  A character draws 1 card per scene.
```

## buildIntentCoherence

Estimates whether the plan respects player build intent.

It should allow tactical exceptions.

It may include:

```text
charactersUsedAccordingToBuild
justifiedBuildExceptions
unjustifiedBuildViolations
keyCardPolicyViolations
```

## setupPayoffTiming

Estimates whether the plan is using setup and payoff actions at the right time.

It may include:

```text
setupActions
payoffActions
prematurePayoff
missedPayoffWindow
setupProgress
```

## redundancyWaste

Estimates scene-level waste.

It may include:

```text
totalOverkill
totalOverStagger
redundantThreatCoverage
unusedSpeedDice
unnecessaryKeyCardSpend
```

## failureRisk

Estimates what happens if the primary objective is not achieved.

It may include:

```text
failureConsequence
fallbackState
riskIfPrimaryOutcomeFails
isPlanBStillPlayable
```

## totalScore

totalScore is a ranking aid.

Rule:

```text
Decision providers may use totalScore for sorting,
but should also consider explanation, risks, outcome confirmation, and objective feasibility.
```



