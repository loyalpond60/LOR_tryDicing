# AutoBattle Flow

AutoBattle is responsible for advancing battle phases without requiring the user to press space or P.

It is separate from AutoPlay:

```text
AutoBattle:
  Advances the game through battle phases.

AutoPlay:
  Chooses cards and targets when the game asks for auto card assignment.
```

## Hook

### StageController.OnUpdate

Scope:

```text
The battle scene update loop.
```

Subject:

```text
The current StageController.
```

Object:

```text
The current battle phase.
```

Effect:

```text
Lets the mod observe the current phase and trigger phase-appropriate original methods.
```

The patch avoids declaring the original `deltatTime` parameter because the original method name is typo-sensitive for Harmony patch binding.

## Simplified Phase Flow

Current intended automation:

```text
RoundStartPhase_UI
  -> skip round start UI

RoundStartPhase_System
  -> stop speed dice rolling

ApplyLibrarianCardPhase
  -> trigger original auto card assignment
  -> complete card application

Battle resolving
  -> wait

Next scene
  -> repeat
```

## Original Battle Phase Semantics

Source reference:

```text
generated/decompiled/Assembly-CSharp/Assembly-CSharp/StageController.cs
```

The original battle loop is a phase-driven state machine owned by
`StageController`. `OnFixedUpdateLate(float deltaTime)` dispatches behavior based
on the current `StagePhase`.

The normal scene flow can be summarized as:

```text
RoundStartPhase_UI
  -> RoundStartPhase_System
  -> SortUnitPhase
  -> DrawCardPhase
  -> ApplyEnemyCardPhase
  -> ApplyLibrarianCardPhase
  -> ArrangeEquippedCards
  -> ChangeMapPhase, when a card requests a map change
  -> ActivateStartBattleEffect
  -> WaitStartBattleEffect
  -> SetCurrentDiceAction
  -> CheckFarAreaPlay
  -> ExecuteFarAreaPlay / EndFarAreaPlay, when a mass attack exists
  -> MoveUnits
  -> WaitUnitsArrive
  -> ExecuteParrying / EndParrying, when two selected cards clash
  -> ExecuteOneSideAction / EndOneSideAction, when no clash exists
  -> ProcessViewAction
  -> SetCurrentDiceAction, until no actions remain
  -> RoundEndPhase
  -> RoundStartPhase_UI
```

Important details:

```text
SortUnitPhase:
  Rolls speed dice for alive units and then advances to DrawCardPhase.

DrawCardPhase:
  Draws scene cards for player units and advances to ApplyEnemyCardPhase.

ApplyEnemyCardPhase:
  Automatically assigns enemy cards for each usable enemy speed die.
  Non-controllable player units are also assigned here.
  Then the phase advances to ApplyLibrarianCardPhase.

ApplyLibrarianCardPhase:
  Waits for player card assignment.
  Manual input, vanilla auto-card, or this mod can fill player card slots.

ArrangeEquippedCards:
  Collects all assigned BattlePlayingCardDataInUnitModel entries.
  Sorts them by range and speed value.

SetCurrentDiceAction:
  Picks each unit's current actionable card from the arranged list.
  If no actions remain, the scene moves toward RoundEndPhase.

CheckFarAreaPlay:
  Gives mass attacks priority before ordinary movement and clash resolution.

MoveUnits / WaitUnitsArrive:
  Moves acting units and chooses the next arrived highest-speed action.
  If the target slot has a matching opposing action, a clash starts.
  Otherwise, a one-sided action starts.

ProcessViewAction:
  Waits for the action visualization to finish, processes achievements, and
  returns to SetCurrentDiceAction for the next action.

RoundEndPhase:
  Runs round-end hooks, emotion checks, no-card checks, unit reset, card ability
  cleanup, delayed effects, and then returns to RoundStartPhase_UI.
```

## Card Assignment Versus Resolution

Card assignment and combat resolution are separate engine stages.

During assignment:

```text
BattleAllyCardDetail.PlayTurnAutoForPlayer(int idx)
  -> chooses a card and target for one player speed die in vanilla auto mode
  -> calls BattlePlayingCardSlotDetail.AddCard(...)
```

`AddCard(...)` creates or updates a `BattlePlayingCardDataInUnitModel` for the
chosen speed die. It records:

```text
owner
card
target
slotOrder
targetSlotOrder
speedDiceResultValue
earlyTarget
earlyTargetOrder
card ability script
dice behavior queue
```

It may also reserve or spend card resources and trigger card apply hooks.

During resolution:

```text
StageController
  -> arranges all assigned cards
  -> creates current dice actions
  -> executes mass attacks, clashes, or one-sided actions
  -> removes used card data
  -> repeats until no actions remain
```

This distinction matters for AutoPlay:

```text
AutoPlay should decide and validate assignments.
AutoBattle should advance phases.
The original engine should still resolve dice, card scripts, passives, damage,
stagger, death, emotion, and round-end effects.
```

## Original Methods Invoked

### CheckInput(bool forcelyInput)

Scope:

```text
Original battle input handling.
```

Subject:

```text
StageController.
```

Object:

```text
The current battle phase.
```

Effect:

```text
In round-start UI phase, can skip the UI when called with force.
```

### StopSpeedDiceRoll()

Scope:

```text
The speed dice rolling phase.
```

Subject:

```text
StageController.
```

Object:

```text
Current speed dice roll animation / state.
```

Effect:

```text
Stops speed dice rolling and advances toward card application.
```

### SetAutoCardForPlayer()

Scope:

```text
Player card assignment phase.
```

Subject:

```text
StageController and all actionable player units.
```

Object:

```text
Each player unit's available speed dice.
```

Effect:

```text
Calls the original auto-card path, which eventually reaches BattleAllyCardDetail.PlayTurnAutoForPlayer(int idx).
```

Because AutoPlayPatch intercepts `PlayTurnAutoForPlayer`, this method becomes the bridge from battle flow automation into custom strategy.

### CompleteApplyingLibrarianCardPhase(bool auto)

Scope:

```text
The end of player card assignment phase.
```

Subject:

```text
StageController.
```

Object:

```text
The player-side assigned cards.
```

Effect:

```text
Moves the battle toward card arrangement and combat resolution.
```

Current implementation calls it with:

```text
auto = true
```

This bypasses the original manual-input guard.

## Fallback Principle

AutoBattle should keep the game playable.

If a custom card assignment fails, AutoPlay can fall back to vanilla auto-play.

If future agent integration fails, AutoBattle should still be able to continue with a local rule provider or current prototype strategy.

## Logging

AutoBattle writes through:

```text
TryDicingLogger
```

This means AutoBattle messages appear in:

```text
Unity Player.log
C:\Users\User\Documents\library_of_ruina_mod開發\logs\tryDicing.log
```
