# AutoPlay Pipeline

AutoPlay is responsible for choosing a battle page and target for player speed dice.

It does not advance battle phases by itself. Battle phase control belongs to AutoBattle.

## Original Hook

### BattleAllyCardDetail.PlayTurnAutoForPlayer(int idx)

Scope:

```text
A single player-controlled unit's single speed die.
```

Subject:

```text
The player unit that owns the BattleAllyCardDetail instance.
```

Object:

```text
One speed die index, one selected battle page, and one enemy target slot.
```

Effect:

```text
Assigns which card this player speed die will use and which enemy unit or enemy speed die it will target.
```

`idx` means:

```text
The speed dice index on the acting player unit.
```

It is not the character index and not the target index.

## Current Custom Flow

Current flow:

```text
AutoPlayPatch
  -> AutoPlayController
  -> BattleSnapshotReader
  -> TacticalPlanner
       -> LegalActionFinder
            -> LegalActionSearchResult
                 -> ActionCandidate list
                 -> LegalActionSearchReport
       -> LocalActionEvaluator
  -> BattlePlanExecutor
  -> ActionExecutor
```

## Current Verification Status

Latest smoke test:

```text
2026-07-03
```

Verified behavior:

```text
The original PlayTurnAutoForPlayer(int idx) hook is intercepted.
The custom planner creates ActionCandidate entries.
The local evaluator creates score and reason output.
Selected SpeedDiceAction entries are executed.
Selection details are written to tryDicing.log.
```

This verification only confirms that the pipeline runs. It does not claim that the current strategy is tactically good.

### AutoPlayPatch

Scope:

```text
The interception point for original auto-play.
```

Subject:

```text
BattleAllyCardDetail.PlayTurnAutoForPlayer(int idx).
```

Object:

```text
The original vanilla auto-play behavior.
```

Effect:

```text
Runs custom autoplay first. If custom autoplay succeeds, skip vanilla behavior. If it fails, allow vanilla behavior.
```

### AutoPlayController

Scope:

```text
One autoplay request for one speed die.
```

Subject:

```text
The acting BattleAllyCardDetail and speed dice index.
```

Object:

```text
The current battle snapshot and cached scene plan.
```

Effect:

```text
Coordinates reading battle state, obtaining a plan, and executing the action for the requested speed die.
```

### BattleSnapshotReader

Scope:

```text
Current visible battle state needed by the first prototype strategy.
```

Subject:

```text
Original Library of Ruina battle objects.
```

Object:

```text
Player units, enemy units, hand cards, light, HP, stagger, and speed dice data used by the prototype.
```

Effect:

```text
Creates a simplified snapshot that custom strategy code can read without spreading game-object access everywhere.
```

### TacticalPlanner

Scope:

```text
Current prototype whole-scene plan using first-version local action scoring.
```

Subject:

```text
BattleSnapshot.
```

Object:

```text
Candidate player speed dice, ActionCandidates, LocalActionEvaluations, and selected SpeedDiceActions.
```

Effect:

```text
For each usable player speed die, asks LegalActionFinder for candidates, asks LocalActionEvaluator to score them, then puts the highest-scoring action into the BattlePlan.
```

### LegalActionFinder

Scope:

```text
One actor speed die.
```

Subject:

```text
The acting unit, speed die index, currently available hand cards, remaining light, and enemy targets.
```

Object:

```text
Possible card / target / target-slot combinations.
```

Effect:

```text
Produces ActionCandidate entries and a LegalActionSearchReport without scoring candidates.
```

Output:

```text
LegalActionSearchResult
  Candidates
  Report
```

Current first-version legality checks:

```text
Actor is alive and can act.
Speed die exists, is not broken, and has no card assigned.
Card is in the available hand list.
Card cost is within remaining light.
actor.CheckCardAvailableForPlayer(card) passes.
Special ranges such as Instance, FarArea, and FarAreaEach are skipped.
Target is alive and targetable.
Target slot exists and is not broken.
```

Current pipeline report:

```text
LegalActionSearchReport records hand count, remaining light, target count,
card filtering counts, target filtering counts, candidate count,
and Clash / OneSidedAttack counts.
```

### LocalActionEvaluator

Scope:

```text
One ActionCandidate.
```

Subject:

```text
Candidate action and current remaining light.
```

Object:

```text
Card power, target pressure, clash bonus, kill/stagger opportunity, resource penalty, and waste penalty.
```

Effect:

```text
Returns LocalActionEvaluation with a total score and reason string.
```

Current first-version scoring is intentionally rough:

```text
cardPowerScore:
  Card cost and estimated dice power from DiceProbabilityCalculator.

targetScore:
  Prefer already damaged, low-HP, or near-stagger targets.

clashScore:
  Prefer Clash over OneSidedAttack for now.

damageScore:
  Prefer actions with rough expected damage that can kill or nearly kill.

staggerScore:
  Prefer actions with rough expected stagger damage that can stagger or nearly stagger.

resourcePenalty:
  Penalize spending down to low remaining light.

wastePenalty:
  Penalize obvious overkill, especially high-cost cards into very low HP targets.
```

### ActionExecutor

Scope:

```text
Final application of one selected speed-dice action.
```

Subject:

```text
The acting player unit's BattleAllyCardDetail.
```

Object:

```text
Selected card, target unit, target speed die slot, and acting speed die index.
```

Effect:

```text
Writes the card assignment into the original game state.
```

Current important assignment code:

```csharp
self.cardOrder = speedDiceIndex;
targetSlot = self.ChangeTargetSlot(card, target, speedDiceIndex, targetSlot, self.TeamKill());
self.cardSlotDetail.AddCard(card, target, targetSlot, false);
```

Future rule:

```text
ActionExecutor should stay close to this responsibility.
Strategy calculation should move into LegalActionFinder, evaluators, planners, and decision providers.
```
