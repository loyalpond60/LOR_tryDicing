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
Assign a card and target for a player speed die.
Advance battle flow automatically through StageController phases.
Fall back to vanilla behavior when the custom action selection fails.
Write tryDicing logs to both Unity Player.log and a workspace log file.
Read first-version DeclaredAction summaries from already assigned speed-dice cards.
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

The current strategy is V1 local-action prototype:

```text
For each usable speed die:
  Enumerate candidate card / target / target-slot actions.
  Filter by hand, light, speed die, target, and simple page range rules.
  Mark each candidate as Clash or OneSidedAttack.
  Score each candidate with a first-version local evaluator.
  Select the highest local score.
```

This is still not the final strategy. It is the first explainable scoring pipeline.

## Latest Smoke Test

Date:

```text
2026-07-03
```

Result:

```text
Passed.
```

Verified:

```text
tryDicing.log is created and written.
AutoBattle advances battle phases.
AutoPlayPatch intercepts PlayTurnAutoForPlayer(int idx).
LegalActionFinder produces ActionCandidate entries.
LocalActionEvaluator produces score and reason output.
TacticalPlanner selects LocalAction results.
ActionExecutor executes selected SpeedDiceAction entries.
No ERROR lines were observed in the checked tryDicing.log sample.
```

Observed fallback:

```text
Some speed dice can still fall back to vanilla when no planned action exists.
This is acceptable for the current smoke-test stage.
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

The next stable implementation target is to continue V1 from a working smoke-tested skeleton:

```text
Keep the current runnable pipeline stable.
Improve observability where needed.
Then extend one V1 component at a time.
```

V1 should still be local and deterministic. Agent / MCP integration should wait until the local action and plan data structures are stable enough to expose cleanly.
