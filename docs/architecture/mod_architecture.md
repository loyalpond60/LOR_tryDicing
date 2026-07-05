# Mod Architecture

This document describes how the implemented mod is organized.

It focuses on code responsibilities, not strategy theory.

## Load Entry

### Initializer.OnInitializeMod

Scope:

```text
The whole tryDicing mod load process.
```

Subject:

```text
The mod assembly.
```

Object:

```text
Harmony patches declared by the mod.
```

Effect:

```text
Installs runtime patches so the mod can intercept selected Library of Ruina methods.
```

## Logging Layer

### TryDicingLogger

Scope:

```text
All tryDicing diagnostic output.
```

Subject:

```text
Messages produced by AutoPlay, AutoBattle, and future strategy evaluators.
```

Object:

```text
Unity Player.log and the workspace log file.
```

Effect:

```text
Writes each message to Player.log through Unity logging and also appends it to the workspace log file.
```

Current file log path:

```text
C:\Users\User\Documents\library_of_ruina_mod開發\logs\tryDicing.log
```

Purpose:

```text
Keep Unity-level errors visible in Player.log while giving strategy debugging a cleaner tryDicing-only log.
```

## AutoPlay Layer

AutoPlay handles card and target assignment for player speed dice.

Current files:

```text
src/tryDicing/AutoPlay/AutoPlayPatch.cs
src/tryDicing/AutoPlay/AutoPlayController.cs
src/tryDicing/AutoPlay/BattleSnapshotReader.cs
src/tryDicing/AutoPlay/ActionCandidate.cs
src/tryDicing/AutoPlay/InteractionType.cs
src/tryDicing/AutoPlay/LegalActionFinder.cs
src/tryDicing/AutoPlay/LocalActionEvaluation.cs
src/tryDicing/AutoPlay/LocalActionEvaluator.cs
src/tryDicing/AutoPlay/DiceProbabilityCalculator.cs
src/tryDicing/AutoPlay/DiceProbabilityProfile.cs
src/tryDicing/AutoPlay/TacticalPlanner.cs
src/tryDicing/AutoPlay/BattlePlanExecutor.cs
src/tryDicing/AutoPlay/ActionExecutor.cs
```

Current responsibility split:

```text
AutoPlayPatch:
  Intercepts the original auto-play method.

AutoPlayController:
  Coordinates one auto-play decision request for a speed die.

BattleSnapshotReader:
  Reads current battle state from original game objects.

LegalActionFinder:
  Lists first-version legal candidate actions for one actor speed die.

LocalActionEvaluator:
  Scores one ActionCandidate and returns an explainable LocalActionEvaluation.

DiceProbabilityCalculator:
  Estimates first-version dice power from a battle page's dice list.

TacticalPlanner:
  Coordinates candidate enumeration, local evaluation, and final action selection.

BattlePlanExecutor:
  Finds the planned action for the requested speed die.

ActionExecutor:
  Applies the selected card and target to the original game object.
```

Important boundary:

```text
ActionExecutor should be the only AutoPlay class that directly writes the final card assignment into game state.
```

## AutoBattle Layer

AutoBattle handles battle phase advancement so the user does not need to repeatedly press space and P.

Current files:

```text
src/tryDicing/AutoBattle/AutoBattleProbePatch.cs
src/tryDicing/AutoBattle/AutoBattleStateProbe.cs
src/tryDicing/AutoBattle/AutoBattleStateReader.cs
src/tryDicing/AutoBattle/AutoBattleController.cs
src/tryDicing/AutoBattle/AutoBattleActionInvoker.cs
```

Current responsibility split:

```text
AutoBattleProbePatch:
  Hooks StageController.OnUpdate.

AutoBattleStateProbe:
  Samples current battle phase.

AutoBattleStateReader:
  Converts original StageController state into a simplified AutoBattleState.

AutoBattleController:
  Decides when to advance the battle phase.

AutoBattleActionInvoker:
  Calls original StageController methods through reflection.
```

## Strategy Boundary

The current mod already has a working control pipeline.

Future strategy improvements should avoid putting all logic into one large class. The intended direction is:

```text
BattleContextReader
AvailableResourceReader
LegalActionFinder
LocalActionEvaluator
CandidateBattlePlanGenerator
PlanEvaluator
DecisionProvider
ActionExecutor
```

The final executor should remain small. It should receive a validated action or plan and apply it to the game.
