# Project Status

Last organized: 2026-07-03.

## Goal

Build a Library of Ruina mod that can automatically operate combat.

The long-term goal is not only hard-coded autoplay. The intended direction is:

```text
Read current battle state.
Evaluate legal actions and battle plans.
Let local rules or an external agent choose among evaluated plans.
Execute the validated plan in-game.
```

The player's deckbuilding remains part of the fun. The strategy system should play the build the player created, not replace the player's key page, passive, or deck choices.

## Current Working State

The mod can currently:

```text
Load through the game's mod system.
Install Harmony patches.
Intercept BattleAllyCardDetail.PlayTurnAutoForPlayer(int idx).
Assign a V2-selected card and target for a player speed die.
Advance battle flow automatically through StageController phases.
Randomly select abnormality passive / EGO rewards when emotion-level selection appears.
Prevent invitation books from being consumed after a lost invitation battle.
Intentionally skip speed dice not selected by the V2 plan instead of letting
vanilla auto play fill them.
Write tryDicing logs to both Unity Player.log and a workspace log file.
Read first-version DeclaredAction summaries from already assigned speed-dice cards.
Read first-version PlayerAvailableResources / ActorAvailableResources summaries for the planned side.
Estimate first-version ordinary attack-dice HP and stagger damage with runtime HP/BP resistances.
Assess first-version unanswered enemy threat from declared enemy actions using DamageEstimator min / expected / max estimates.
Classify first-version threat levels with Guaranteed / Expected / Potential risk and HP/stagger pressure ratios.
Build first-version ThreatResponseAssessment and ThreatResponseMatrix relationship data from BattleSnapshot for V2 plan search.
Collect first-version team-wide ActionCandidate lists from PlayerAvailableResources for V2 plan search.
Evaluate first-version selected action sets with PlanEvaluator / PlanEvaluation.
Search first-version selected action sets with PlanSearch / PlanSearchResult.
Execute first-version V2 PlanSearch results through TacticalPlanner.
Block invalid planned actions with a final BattlePlanExecutor legality check before writing to game state.
Enumerate first-version local action candidates.
Evaluate first-version local action scores.
Estimate first-version card dice power through a dedicated DiceProbabilityCalculator.
Score first-version kill and stagger opportunities in LocalActionEvaluator.
Log selected local action score and reason.
Export first-version static game knowledge from BaseMod/StaticInfo into workspace JSON files.
Export first-version card effect tag profiles for future evaluator use.
Export first-version decompiled card ability and passive effect summaries for future BuildProfile and evaluator use.
Export first-version EGO profiles and EmotionCard / abnormality-card profiles.
Mark complex passive/effect hooks such as dice power, targeting, resistance, damage, survival, and cost modifiers.
Extract first-version passive method summaries for common conditions, formulas, numeric hints, card-id checks, and keyword-buff applications.
```

The current strategy is V2 scene-plan prototype:

```text
For the planned side:
  Read the current BattleSnapshot.
  Collect independently legal ActionCandidate entries.
  Build ThreatAssessment and ThreatResponseMatrix data.
  Run first-version PlanSearch over resource-feasible action sets.
  Convert selected ActionCandidates into SpeedDiceActions.
  Leave speed dice unassigned when V2 does not select an action for them.
```

This is still not the final strategy. It is the first executable scene-plan
pipeline.

## Latest Build Verification

Date:

```text
2026-07-07
```

Result:

```text
Build passed.
```

Verified:

```text
dotnet build src\tryDicing\tryDicing.csproj completed with 0 warnings and 0 errors.
TacticalPlanner compiles with V2 PlanSearch as the executed plan source.
AutoPlayController compiles with intentional V2 skip behavior.
BattlePlanExecutor compiles with final action legality checks.
```

Runtime smoke status:

```text
Needs an in-game smoke test after copying the rebuilt mod assembly into the
game mod folder.
```

## Important Constraints

Workspace files may be read and edited freely:

```text
C:\Users\User\Documents\library_of_ruina_mod開發
```

Game installation files outside the workspace are read-only for Codex:

```text
F:\SteamLibrary\steamapps\common\Library Of Ruina
```

Only the user should copy files into the game installation or modify files there.

## Current Mod Areas

```text
src/tryDicing/Initializer.cs
src/tryDicing/TryDicingLogger.cs
src/tryDicing/AutoPlay
src/tryDicing/AutoBattle
```

Current independent log file path:

```text
C:\Users\User\Documents\library_of_ruina_mod開發\logs\tryDicing.log
```

Packaged mod files are kept under:

```text
mod/tryDicing
```

## Current Documentation Map

Development workflow:

```text
docs/workflow.md
```

Original game API notes:

```text
docs/lor_autoplay_original_api.md
```

Code architecture:

```text
docs/architecture/mod_architecture.md
docs/architecture/autoplay_pipeline.md
docs/architecture/auto_battle_flow.md
docs/architecture/static_knowledge_base.md
```

Strategy design:

```text
docs/strategy
```

Agent / MCP design:

```text
docs/agent
```

Raw brainstorm archive:

```text
docs/notes/battle_strategy_brainstorm.md
```

## Next Likely Implementation Step

The next stable implementation target is to smoke-test V2 execution in game:

```text
Confirm PlanSearch-selected actions are legal at execution time.
Confirm unselected speed dice remain empty and are not filled by vanilla auto play.
Inspect V2Plan and AutoPlay skip/block log lines.
```

Agent / MCP integration should wait until the local action and plan data structures are stable enough to expose cleanly.
