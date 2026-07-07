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

Current files by responsibility:

```text
Hook / orchestration:
  AutoPlayPatch.cs
  AutoPlayController.cs

Observed scene data:
  BattleSnapshot.cs
  BattleSnapshotReader.cs
  DeclaredAction.cs
  PlayerAvailableResources.cs
  ActorAvailableResources.cs

Candidates and local scoring:
  ActionCandidate.cs
  ActionCandidateCollector.cs
  InteractionType.cs
  LegalActionFinder.cs
  LegalActionSearchReport.cs
  LegalActionSearchResult.cs
  LocalActionEvaluation.cs
  LocalActionEvaluator.cs
  PlanEvaluation.cs
  PlanEvaluator.cs
  PlanSearch.cs
  PlanSearchResult.cs

Assessment helpers:
  DamageEstimate.cs
  DamageEstimator.cs
  DiceProbabilityCalculator.cs
  DiceProbabilityProfile.cs
  ThreatAssessment.cs
  ThreatAssessor.cs
  ThreatLevel.cs
  ThreatResponseAssessment.cs
  ThreatResponseAssessor.cs
  ThreatResponseMatrix.cs
  ResponseMechanism.cs
  OwnerDamageOutcome.cs

Planning / execution:
  TacticalPlanner.cs
  BattlePlan.cs
  SpeedDiceAction.cs
  BattlePlanExecutor.cs
  ActionExecutor.cs
```

Current responsibility split:

```text
AutoPlayPatch:
  Intercepts the original auto-play method.

AutoPlayController:
  Coordinates one auto-play decision request for a speed die.

BattleSnapshotReader:
  Reads current battle state from original game objects.

PlayerAvailableResources:
  Summarizes player-side team resources for the current assignment scene.

ActorAvailableResources:
  Summarizes one actor's hand, light, and usable speed dice while preserving resource ownership.

DeclaredAction:
  Summarizes an already assigned battle page on a unit speed die as observed scene data.

LegalActionFinder:
  Lists first-version legal candidate actions for one actor speed die.

ActionCandidateCollector:
  Uses PlayerAvailableResources to collect independently legal candidates across
  all player-side usable speed dice for current V2 plan search.

LocalActionEvaluator:
  Scores one ActionCandidate and returns an explainable LocalActionEvaluation.
  It remains V1 support code and is not the current TacticalPlanner execution
  path.

PlanEvaluator:
  Scores a selected List<ActionCandidate> as a full-scene exchange using
  BattleSnapshot and ThreatResponseMatrix. It is not a searcher; PlanSearch uses
  it to rank selected action sets.

PlanSearch:
  Performs first-version threat-guided beam search over candidate action sets.
  It uses PlanEvaluator for scoring and returns PlanSearchResult. TacticalPlanner
  converts its selected actions into the executable BattlePlan.

DamageEstimator:
  Estimates first-version HP and stagger damage using runtime HP/BP resistances.

DiceProbabilityCalculator:
  Estimates first-version dice power from a battle page's dice list.

ThreatAssessment:
  Derived danger data for one opposing DeclaredAction if unanswered.

ThreatResponseAssessment:
  Derived relationship data for one ThreatAssessment and one ActionCandidate.
  Its mechanism and owner-damage outcome are fields, not separate strategy layers.

ThreatAssessor:
  Estimates first-version unanswered enemy threat from declared enemy actions and DamageEstimator output.

ThreatResponseAssessor:
  Builds one ThreatResponseAssessment.

ThreatResponseMatrix:
  Groups all ThreatAssessment x ActionCandidate relationship records for V2
  full-scene plan evaluation. It can build the full relationship table from a
  BattleSnapshot, but it does not choose actions by itself.

TacticalPlanner:
  Runs PlanSearch and converts legal selected ActionCandidates into
  SpeedDiceActions. It does not fill unselected speed dice with V1 greedy
  actions.

BattlePlanExecutor:
  Validates the planned action again at execution time before sending it to
  ActionExecutor.

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

## AutoEmotion Layer

AutoEmotion handles automatic emotion-level rewards such as abnormality passive
cards and EGO cards. It is separate from AutoPlay because it responds to a
different game decision point.

Current files:

```text
src/tryDicing/AutoEmotion/AutoEmotionChoicePatch.cs
```

Current responsibility split:

```text
AutoEmotionChoicePatch:
  Hooks LevelUpUI.Init and LevelUpUI.InitEgo after the original game has already
  generated candidate abnormality / EGO choices. For the current first version,
  it randomly selects one candidate, applies it through the original
  StageLibraryFloorModel.OnPickPassiveCard / OnPickEgoCard methods, then hides
  the selection UI. SelectOne abnormality passives currently choose a random
  living player unit. Future strategy should replace only the random choice
  step, not move this flow into AutoPlay.
```

## Progression / QoL Layer

Progression patches handle narrow campaign or reception-flow behavior that is
outside battle strategy.

Current files:

```text
src/tryDicing/Progression/InvitationBookLossPatch.cs
```

Current responsibility split:

```text
InvitationBookLossPatch:
  Hooks StageController.GameOver. When an invitation battle is lost, it clears
  StageController.UsedBooks before the original loss cleanup removes those books
  from DropBookInventoryModel. This prevents the reception invitation books from
  being consumed by test losses, without changing normal book burning or other
  DropBookInventoryModel.RemoveBook callers.
```

## Strategy Boundary

The current mod already has a working control pipeline.

Future strategy improvements should avoid putting all logic into one large class. The intended direction is:

```text
BattleContextReader
AvailableResourceReader
LegalActionFinder
LocalActionEvaluator
PlanSearch
PlanEvaluator
DecisionProvider
ActionExecutor
```

The final executor should remain small. It should receive a validated action or plan and apply it to the game.
