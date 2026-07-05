# Raw Battle Data Inventory

This document lists original Library of Ruina data that the current mod can read or already uses.

Purpose:

```text
Know what raw data is available.
Decide what should be compressed into pipeline reports.
Avoid sending raw game objects or huge dumps to logs or future agents.
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
  -> LocalActionEvaluation
  -> SpeedDiceAction
  -> BattlePlan
  -> BattlePlanExecutor
  -> ActionExecutor
```

## StageController

Available raw data:

```text
stage.State
stage.Phase
stage.RoundTurn
```

Current use:

```text
AutoBattleStateReader reads battle phase and round.
AutoBattleController decides when to skip UI, stop speed dice, assign cards, and start battle.
```

Suitable for pipeline report:

```text
round
phase
rawState
```

Not suitable for routine log:

```text
Full StageController object dump.
Private fields not needed for current decision flow.
```

## BattleObjectManager

Available raw data:

```text
BattleObjectManager.instance.GetAliveList(faction)
```

Current use:

```text
BattleSnapshotReader reads alive actors and targets.
```

Suitable for pipeline report:

```text
actorCount
targetCount
alive actor ids
alive target ids
```

Not suitable for routine log:

```text
Full unit object dumps.
All fields from every BattleUnitModel.
```

## BattleUnitModel

Available raw data:

```text
id
faction
hp
MaxHp
breakDetail.breakGauge
turnState
speedDiceResult
allyCardDetail
cardSlotDetail
IsBreakLifeZero()
IsKnockout()
IsTargetable(actor)
IsTargetable_theLast()
GetSpeed(speedDiceIndex)
CheckCardAvailableForPlayer(card)
TeamKill()
ChangeTargetSlot(card, target, speedDiceIndex, targetSlot, teamKill)
```

Current use:

```text
BattleSnapshotReader stores actor and target lists.
LegalActionFinder checks actor usability, target legality, speed dice state, and card availability.
LocalActionEvaluator reads HP and break gauge for rough target scoring.
ActionExecutor writes final assignment through original methods.
```

Suitable for pipeline report:

```text
actorId
targetId
hp / MaxHp when relevant
breakGauge when relevant
speed value
speed die broken state
is targetable summary
```

Not suitable for routine log:

```text
Full BattleUnitModel dump.
All buffs/debuffs before we have a compressed status schema.
All passive script objects before we model passives.
```

## BattleAllyCardDetail

Available raw data:

```text
GetHand()
GetDeck()
GetDiscarded()
PlayTurnAutoForPlayer(int idx)
private _self field
```

Current use:

```text
AutoPlayPatch intercepts PlayTurnAutoForPlayer(int idx).
BattleSnapshotReader reads _self through reflection.
TacticalPlanner reads GetHand() for available cards.
```

Suitable for pipeline report:

```text
hand count
card names / ids for selected or blocked cards when useful
```

Not suitable for routine log:

```text
Full hand/deck/discard dumps every speed die by default.
```

## BattlePlayingCardSlotDetail

Available raw data:

```text
PlayPoint
ReservedPlayPoint
cardAry
AddCard(card, target, targetSlot, ...)
```

Current use:

```text
TacticalPlanner computes remainingLight = PlayPoint - ReservedPlayPoint.
LegalActionFinder checks whether actor and target slots already have cards.
ActionExecutor calls AddCard().
```

Suitable for pipeline report:

```text
remainingLight
assigned slot count
whether a target slot already has a card
```

Not suitable for routine log:

```text
Full card slot object dump.
All internal slot state before a schema is needed.
```

## BattleDiceCardModel

Available raw data:

```text
GetName()
GetID()
GetSpec()
GetCost()
GetOriginCost()
GetPriority(speed)
GetBehaviourList()
IsExhaustOnUse()
IsValidTarget(actor, card, target)
```

Current use:

```text
LegalActionFinder checks card cost, card availability, range, and priority.
LocalActionEvaluator estimates rough card power from cost and dice behaviours.
AutoPlayLog prints selected card name and cost.
```

Suitable for pipeline report:

```text
selected card name
selected card cost
card id later
blockedByLight count
blockedByRange count
blockedByAvailability count
blockedByPriority count
```

Not suitable for routine log:

```text
Full card XML.
Full card script objects.
Full behaviour list for every candidate by default.
```

## DiceCardSpec

Available raw data:

```text
card.GetSpec().Ranged
card.GetSpec().affection
```

Current use:

```text
LegalActionFinder skips first-version unsupported ranges:
  CardRange.Instance
  CardRange.FarArea
  CardRange.FarAreaEach
```

Suitable for pipeline report:

```text
blockedByRange count
range for selected card when debugging special cases
```

Not suitable for routine log:

```text
Full card spec dump every candidate.
```

## DiceBehaviour

Available raw data:

```text
Min
Dice
Type
Detail
Script
Desc
```

Current use:

```text
LocalActionEvaluator estimates dice power:
  average = (Min + Dice) / 2
  attack dice count fully
  non-attack dice count partially
```

Suitable for pipeline report:

```text
estimatedCardPower
selected card dice summary later if needed
```

Not suitable for routine log:

```text
All dice behaviours for every candidate by default.
Dice scripts before script effects are modeled.
```

## Targeting / Legality Methods

Available raw data / methods:

```text
actor.CheckCardAvailableForPlayer(card)
target.IsTargetable(actor)
target.IsTargetable_theLast()
actor.ChangeTargetSlot(card, target, speedDiceIndex, targetSlot, actor.TeamKill())
card.IsValidTarget(actor, card, target)
```

Current use:

```text
LegalActionFinder uses CheckCardAvailableForPlayer and targetability checks.
ActionExecutor uses ChangeTargetSlot before AddCard.
```

Suitable for pipeline report:

```text
blockedByAvailability
blockedByTarget
blockedByTargetSlot
final target slot after ChangeTargetSlot later if needed
```

Not suitable for routine log:

```text
Repeated full legality method trace for every candidate unless debugging a specific bug.
```

## Current Pipeline Report Shape

The first implemented report is:

```text
LegalActionSearchReport
```

It belongs between:

```text
LegalActionFinder
  -> TacticalPlanner
```

It reports:

```text
actorId
actorSpeedDiceIndex
speedValue
handCount
remainingLight
targetCount
speedDieUsable
cardsChecked
cardsUsable
blockedByLight
blockedByAvailability
blockedByRange
blockedByPriority
targetsChecked
targetSlotsChecked
blockedByTarget
blockedByTargetSlot
candidateCount
clashCount
oneSidedAttackCount
```

It does not report:

```text
Full card list.
Full unit state.
Full dice behaviour list.
Passive or script internals.
```

Reason:

```text
The report should explain pipeline state without becoming a raw data dump.
```

## Future Agent / Model Boundary

Future agent or neural-model input should be based on compressed structures:

```text
LegalActionSearchReport
LocalActionEvaluation summary
PlanSummary
BattleSummary
```

It should not receive raw Unity / Library of Ruina objects by default.

