# Local Action Evaluation

LocalActionEvaluation evaluates one possible action for one player speed die.

It answers:

```text
If only this speed die is considered, is this action worth taking?
```

It does not decide whether the whole scene plan is good.

## Action Shape

An action candidate should describe:

```text
actor
actorSpeedDiceIndex
card
target
targetSpeedDiceIndex
interactionType
intentTags
```

## interactionType

interactionType should only describe the rules-resolution shape:

```text
Clash
OneSidedAttack
Invalid
```

It should not contain tactical purpose.

For example, an action can be:

```text
interactionType: Clash
intentTags: InterceptThreat, ProtectAlly, PreventEnemyStatus
```

## intentTags

intentTags describe why the action is being considered.

First-version tags:

```text
DealDamage
StaggerTarget
InterceptThreat
ProtectAlly
RecoverResource
SetupMechanic
PreventEnemyDamage
PreventEnemyStaggerDamage
PreventEnemyStatus
PreventEnemyBuff
PreventEnemyResourceGain
PreventEnemyMechanicSetup
```

Important principle:

```text
Clashing is not the goal.
Damage is not the only goal.
The action's tactical purpose must be represented explicitly.
```

## Evaluation Components

First-version LocalActionEvaluation can include:

```text
LegalityEvaluation
ClashEvaluation
DamageEvaluation
StaggerEvaluation
DefenseEvaluation
ResourceEvaluation
SetupEvaluation
EnemyEffectSuppressionEvaluation
WasteEvaluation
BuildFitEvaluation
localScore
explanation
```

## LegalityEvaluation

Checks whether the action can actually be used.

Examples:

```text
The actor speed die exists.
The card is in hand.
The card is playable with current light.
The target is valid.
The target speed die is valid when required.
The card is not already assigned elsewhere.
```

Illegal actions should be rejected before scoring.

## ClashEvaluation

Estimates the value and risk of a clash.

May include:

```text
winChance
loseChance
expectedClashDamage
expectedDamageTaken
importantEnemyDieStopped
```

## DamageEvaluation

Estimates HP damage value.

May include:

```text
expectedHpDamage
killChance
overkillAmount
targetPriority
```

## StaggerEvaluation

Estimates stagger damage value.

May include:

```text
expectedStaggerDamage
staggerChance
overStaggerAmount
staggerPayoff
```

## DefenseEvaluation

Estimates defensive value.

May include:

```text
expectedHpPrevented
expectedStaggerPrevented
allyProtected
selfRisk
```

## ResourceEvaluation

Estimates light and hand impact.

May include:

```text
lightSpent
lightRecovered
cardsSpent
cardsDrawn
expectedNextSceneLight
expectedNextSceneHand
```

## SetupEvaluation

Estimates whether the action advances the actor's own setup.

This should stay extensible.

May include:

```text
setupTagsAdvanced
setupProgress
payoffReadinessChange
```

## EnemyEffectSuppressionEvaluation

Estimates whether the action prevents dangerous enemy effects.

Examples:

```text
Prevent enemy status application.
Prevent enemy buff.
Prevent enemy resource gain.
Prevent enemy mechanic setup.
```

This matters because clash win rate or raw damage may not be the most important reason to intercept an enemy die.

## WasteEvaluation

Estimates local waste.

Examples:

```text
Severe overkill.
Severe over-stagger.
Using a key card for low impact.
Spending too much light for a low-risk target.
```

Team-wide waste belongs to PlanEvaluation.

## BuildFitEvaluation

Estimates whether the action fits the actor's BuildProfile.

Examples:

```text
A clash-control build intercepting a dangerous die:
  High fit.

A burst-payoff build spending a key card during a real payoff window:
  High fit.

A payoff build spending a key card on a low-value target:
  Low fit.
```

## Usage Rule

LocalActionEvaluation ranks one action in isolation.

It should feed into plan generation, but it should not replace full-scene PlanEvaluation.

