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
