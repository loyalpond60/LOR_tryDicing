# Plan Evaluation

PlanEvaluation evaluates a selected List<ActionCandidate> under the current BattleSnapshot and ThreatResponseMatrix.

It is not only a plan score.

It answers:

```text
Is this selected action set a coherent and acceptable scene plan,
given the current resources, threats, and expected exchange?
```

## Evaluator Boundary

PlanEvaluation is not a combat simulator.

It should not try to fully reproduce Library of Ruina resolution, passive
scripts, card scripts, boss mechanics, or stage effects.

Boundary rule:

```text
Use runtime-known values when the game exposes them.
Use heuristic estimates when exact values are not available.
Represent complex or unknown mechanics as uncertainty before modeling them.
Only model a special rule locally when it is common, high-impact, and has a
clear stable API or extracted rule.
```

The evaluator should answer:

```text
Which selected action set looks like a better exchange?
Which threats appear answered, partially answered, accepted, or unknown?
Which score dimensions are based on rough estimates?
What risk remains if the estimate is wrong?
```

It should not claim:

```text
The exact post-resolution HP / stagger state.
The exact result of every clash die.
The exact effect of every passive, buff, card script, or boss mechanic.
```

Unknown mechanics policy:

```text
Unknown mechanics do not block evaluation.
They lower confidence, add explanation notes, and may trigger external-agent
review later.
```

## V2 Matrix-Based Plan Search Sketch

This is the current core-algorithm sketch for moving from local action choice to
full-scene planning.

Architecture position:

```text
BattleSnapshot
  -> ThreatAssessor
      -> List<ThreatAssessment>

BattleSnapshot
  -> ActionCandidateCollector
      -> List<ActionCandidate>

ThreatResponseMatrix
  = List<ThreatAssessment> x List<ActionCandidate>
  -> List<ThreatResponseAssessment>

PlanSearch
  consumes ActionCandidate data, ThreatResponseMatrix, and resource constraints
  -> selected List<ActionCandidate>

PlanEvaluator
  scores and explains the selected action set
```

Core rule:

```text
ThreatResponseMatrix is complete relationship data.
It keeps None responses.
It does not choose actions.
It does not aggregate threats.
It does not connect to V1 local scoring by itself.
```

Meaning:

```text
R[t, c] means how ActionCandidate c responds to ThreatAssessment t.
It is threat-response value, not the complete value of c.
```

An ActionCandidate can also have:

```text
proactive damage value
stagger value
resource value
setup value
build fit
waste
```

The decision algorithm should select the best action set, not the best single
action.

Formal sketch:

```text
Given:
  C = all independently legal ActionCandidate entries
  T = all ThreatAssessment entries
  R[t, c] = ThreatResponseAssessment for threat t and candidate c

Choose:
  P subset of C

Subject to:
  One player speed die can choose at most one candidate.
  One actor cannot reuse the same hand card.
  One actor cannot spend more light than its remainingLight.

Optimize:
  PlanEvaluation quality
  under legality and resource constraints
```

This resembles a dynamic-programming or combinatorial-optimization problem, but
it is not a plain knapsack. It is closer to:

```text
multiple-choice knapsack
+ weighted set coverage
+ resource-constrained assignment
```

Recommended search shape:

```text
1. Threat-guided candidate pool
   Use ThreatAssessment and ThreatResponseMatrix before search.
   Keep strong responses to Critical / Major threats.
   Keep kill / stagger / high local-value candidates even when they are not
   direct threat responses.
   Keep a small set of low-cost or resource-friendly candidates.

2. Resource-feasible plan search
   Search selected action sets from the threat-guided pool.
   Actor-owned resources still constrain the result:
     speed dice cannot be reused
     hand cards cannot be reused
     light cannot be overspent

3. Plan evaluation
   Score the full plan with a structured PlanEvaluation instead of only a total
   number.
```

Possible DP / search state:

```text
processed actor index
selected action candidates
covered threat mask or coverage summary
resource usage summary
plan score breakdown
```

Open design point:

```text
Threat coverage has two levels:

Single-response coverage:
  One ThreatResponseAssessment may directly or partially answer one threat.

Plan-level coverage:
  Multiple selected actions can combine their damage or stagger damage against
  a threat owner. This can upgrade several weak/partial owner attacks into full
  coverage if their combined expected damage or stagger can kill/stagger the
  owner before that threat resolves.

Timing rule:
  A selected action contributes to preventing a threat through owner damage only
  if it targets the threat owner and its acting speed is higher than the threat
  speed. Equal-speed ordering is currently treated as uncertain and does not
  count as confirmed prevention.

Current first-version rule:
  ClashThreatDie is direct coverage.
  Expected or guaranteed owner kill/stagger is full coverage.
  Potential owner kill/stagger is partial coverage.
  Pressure alone does not count as coverage.
```

Boundary:

```text
ThreatResponseMatrix remains data.
PlanSearch performs search.
PlanEvaluator judges the selected List<ActionCandidate>.
V1 TacticalPlanner may later read a small adapter from this data, but V1 should
not define the V2 model.
```

Design warning:

```text
Do not optimize for maximum threat coverage by itself.
The goal is the best victory-oriented exchange set.
```

A good plan may:

```text
fully answer a threat
partially answer a threat
ignore a threat
accept a threat to gain a larger victory-progress exchange
```

## Resource-Grounded Objective Rule

Objective hypotheses are delayed until V4 objective / feasible-outcome variants.

V2 PlanEvaluation should not require SceneObjectiveHypothesis, FeasibleOutcome, or CandidateBattlePlan. It should evaluate whether a selected action set creates a good scene exchange under current resources and threats.

When objective variants are introduced later, the objective context must be grounded in available resources and feasible outcomes.


## Exchange Rule

PlanEvaluation should treat each plan as a proposed resource exchange.

It should ask:

```text
What does the plan spend or risk?
What does it gain or preserve?
How does it change terminal progress?
How does it change future action economy?
How does it change resource flow?
How does it change unresolved risk?
Does it create useful setup future value?
Does the exchange still look good if the primary payoff fails?
```

This keeps plan evaluation from collapsing into immediate damage, immediate defense,
or local clash score.

## Inputs

V2 PlanEvaluation may use:

```text
BattleSnapshot
PlayerAvailableResources
List<ActionCandidate>
ThreatResponseMatrix
LocalActionEvaluations when available
Optional BuildProfile / UserBuildIntent context in later versions
```


## Output Shape

First-version components:

```text
terminalProgress
actionEconomyChange
resourceFlowChange
riskChange
setupFutureValue
cost
waste
totalScore
explanation
```
`totalScore` is useful for ranking, but it should not replace the structured report.

## Projection Rule

PlanEvaluation dimensions are not mutually exclusive event buckets.

Rule:

```text
Events can be multi-impact.
Dimensions must be single-reason.
```

A combat event may project into multiple scoring dimensions when it changes
multiple strategic properties. This is intentional.

Double counting happens only when two dimensions reward the same property for
the same reason.

Example:

```text
EnemyKilled:
  terminalProgress += enemy is removed from the win condition.
  actionEconomyChange += enemy future actions are removed.
  riskChange += enemy future threats are removed.
  resourceFlowChange += future defensive resource pressure is reduced.
  waste -= overkill or overcommitment.
```

The event receives several impacts, but each impact answers a different
question.

## terminalProgress

Estimates how much the plan moves the battle toward an ending condition or phase
goal.

It may include:

```text
enemyDeaths
enemyHpDamageTowardKillRange
bossPhaseProgress
waveClearProgress
mechanicObjectiveProgress
```

Question:

```text
How much closer is the battle to being won or advanced?
```

## actionEconomyChange

Estimates how the plan changes future usable action ability for both sides.

It may include:

```text
alliedDeathsPrevented
alliedStaggersPrevented
enemyDeaths
enemyStaggers
enemyActionSuppression
keyEnemyDiceCanceled
preservedAlliedSpeedDice
```

Question:

```text
How does this plan change who can act effectively in future scenes?
```

## resourceFlowChange

Estimates how the plan changes future light, hand, key-card timing, and resource
pressure.

It may include:

```text
lightAfterPlan
expectedLightNextScene
handAfterPlan
expectedHandNextScene
resourceRecoveryGained
keyCardTimingPreserved
futureDefensiveResourcePressureReduced
```

Baseline rule:

```text
Without passive, card, or emotion-level effects:
  A character recovers 1 light per scene.
  A character draws 1 card per scene.
```

## riskChange

Estimates unresolved or newly created risk after the plan.

It may include:

```text
unansweredThreats
partiallyAnsweredThreats
expectedAllyDeaths
expectedAllyStaggers
bossMechanicFailureRisk
failureConsequence
fallbackState
```

Unresolved risk is not automatically wrong. It is wrong when the remaining risk
is not justified by the exchange.

## setupFutureValue

Estimates whether setup actions create future options that were not previously
available or affordable.

It may include:

```text
setupActions
payoffReadinessChange
futurePayoffWindow
futureKeyCardAffordability
setupProgress
timingRisk
handClogRisk
```

Rule:

```text
Setup is high value when it makes a future payoff move from infeasible to feasible.
Setup is medium value when it makes an already feasible payoff cheaper or safer.
Setup is low value when no payoff window exists or current survival is endangered.
```

Example:

```text
A build has a high-cost key card whose cost decreases when more copies of that
card are in hand.

A copy-card action may have low immediate damage, but high setupFutureValue if it
adds a key-card copy and makes the future payoff affordable.
```

Sketch:

```text
setupFutureValue =
  futurePayoffValue(after setup)
- futurePayoffValue(before setup)
- setupCost
- timingRisk
- handClogRisk
```

## cost

Estimates what the plan spends or risks to get its impact.

It may include:

```text
lightSpent
cardsSpent
speedDiceUsed
keyCardsSpent
acceptedHpRisk
acceptedStaggerRisk
futureTimingCost
```

## waste

Estimates repeated, excessive, or low-purpose resource use.

It may include:

```text
totalOverkill
totalOverStagger
redundantThreatCoverage
unusedSpeedDice
unnecessaryKeyCardSpend
resourceRecoveryOverflow
```

## Supporting Checks

The following checks support PlanEvaluation.

They should not become extra top-level score buckets unless they answer a new
single-reason question.

## objectiveFit

Estimates whether the selected action set has a coherent purpose. This becomes objectiveFit when V4 objective variants are introduced.

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

## threatResponseSummary

Summarizes whether important enemy threats are handled, partially handled,
ignored, or intentionally accepted.

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

This summary feeds `riskChange`, `actionEconomyChange`, `resourceFlowChange`,
and `waste`.

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

This feeds `riskChange` and `actionEconomyChange`.

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

This feeds `resourceFlowChange`.

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

This feeds `setupFutureValue`.

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

This feeds `waste`.

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



