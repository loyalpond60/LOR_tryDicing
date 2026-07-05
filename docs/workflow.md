# Library of Ruina Mod Workflow

This document records the current working flow for the `tryDicing` mod project.

## Current Goal

Build a Library of Ruina mod that can automatically operate combat by combining:

```text
AutoBattle:
  Advances the battle phase flow.

AutoPlay:
  Chooses battle pages and targets when the game requests player auto-assignment.
```

## Feature Description Format

When documenting a function or workflow, use this shape:

```text
Function or feature name

Scope:
  What size of action it controls.

Subject:
  The unit/object being operated on.

Object:
  The target unit/object/slot/card being selected or affected.

Effect:
  What the subject does to the object.
```

The first milestone was a smoke test:

1. Load the mod DLL.
2. Install a Harmony patch.
3. Intercept `BattleAllyCardDetail.PlayTurnAutoForPlayer(int idx)`.
4. Write a log line.
5. Let the original game logic continue.

This has been verified in `Player.log`.

The current milestone is a V1 local-action auto player:

1. Intercept `BattleAllyCardDetail.PlayTurnAutoForPlayer(int idx)`.
2. Read the current battle snapshot.
3. Enumerate legal `ActionCandidate` entries for usable player speed dice.
4. Produce `LegalActionSearchReport` diagnostics.
5. Score candidates with `LocalActionEvaluator`.
6. Build a first-version `BattlePlan`.
7. Execute selected actions through `ActionExecutor`.
8. Return `false` so vanilla auto play does not run after successful custom selection.

Scope:

```text
One call controls one acting unit's one speed die.
```

Subject:

```text
The acting BattleUnitModel recovered from BattleAllyCardDetail._self.
The speed die is identified by PlayTurnAutoForPlayer(int idx).
```

Object:

```text
One selected BattleDiceCardModel from the acting unit's hand.
One target BattleUnitModel selected from living enemies.
One target speed die slot selected on that enemy.
```

Effect:

```text
The acting unit places the selected card on its idx speed die,
then assigns that card to the selected enemy's selected speed die.
```

## Workspace Layout

```text
C:\Users\User\Documents\library_of_ruina_mod開發
  docs\
    workflow.md

  src\
    tryDicing\
      tryDicing.csproj
      Initializer.cs
      AutoBattle\
        AutoBattleActionInvoker.cs
        AutoBattleController.cs
        AutoBattleProbePatch.cs
        AutoBattleState.cs
        AutoBattleStateProbe.cs
        AutoBattleStateReader.cs
      AutoPlay\
        ActionCandidate.cs
        ActionExecutor.cs
        AutoPlayCache.cs
        AutoPlayController.cs
        AutoPlayLog.cs
        AutoPlayPatch.cs
        BattlePlan.cs
        BattlePlanExecutor.cs
        BattleSnapshot.cs
        BattleSnapshotReader.cs
        InteractionType.cs
        LegalActionFinder.cs
        LegalActionSearchReport.cs
        LegalActionSearchResult.cs
        LocalActionEvaluation.cs
        LocalActionEvaluator.cs
        SpeedDiceAction.cs
        TacticalPlanner.cs

  mod\
    tryDicing\
      StageModInfo.xml
      Assemblies\
        tryDicing.dll
        0Harmony.dll
      Data\
      Resource\

  tools\
    dnSpy-netframework\
```

### `src\tryDicing`

This is the C# source project.

It compiles into:

```text
src\tryDicing\bin\Release\tryDicing.dll
```

### `mod\tryDicing`

This is the mod folder that should be copied into the game manually.

The game-side target path is:

```text
F:\SteamLibrary\steamapps\common\Library Of Ruina\LibraryOfRuina_Data\Mods\tryDicing
```

Only the user should copy files into this game directory.

### `tools\dnSpy-netframework`

This is used to inspect Library of Ruina's `Assembly-CSharp.dll`.

It is not part of the mod output.

## Important Files

### `StageModInfo.xml`

Path:

```text
mod\tryDicing\StageModInfo.xml
```

This acts as the mod manifest / entry file.

For the current DLL-only smoke test, all invitation data is disabled:

```xml
<Stage Exist="false" />
<EnemyUnit Exist="false" />
<CardInfo Exist="false" />
<Passive Exist="false" />
```

The purpose is only to let the game detect the mod and load DLLs from:

```text
mod\tryDicing\Assemblies
```

### `Initializer.cs`

Path:

```text
src\tryDicing\Initializer.cs
```

This is called when the mod loads:

```csharp
public class Initializer : ModInitializer
{
    public override void OnInitializeMod()
    {
        new Harmony("tryDicing.autoplay.smoke").PatchAll();
        TryDicingLogger.Info("Harmony smoke patch installed. fileLog=" + TryDicingLogger.LogFilePath);
    }
}
```

Its job is to install all Harmony patches in the DLL and initialize the tryDicing log path.

### `AutoPlay\AutoPlayPatch.cs`

Path:

```text
src\tryDicing\AutoPlay\AutoPlayPatch.cs
```

This intercepts the game's auto battle-page method and delegates to the AutoPlay controller:

Scope:

```text
This patch intercepts one vanilla auto-selection call for one speed die.
```

Subject:

```text
The BattleAllyCardDetail instance passed by the original method.
```

Object:

```text
The speed die index idx and AutoPlayController.
```

Effect:

```text
It gives AutoPlayController a chance to execute the planned action for this speed die.
If selection succeeds, it skips vanilla auto play.
If selection fails, it lets vanilla auto play continue.
```

```csharp
[HarmonyPatch(typeof(BattleAllyCardDetail), "PlayTurnAutoForPlayer")]
public static class AutoPlayPatch
{
    public static bool Prefix(BattleAllyCardDetail __instance, int idx)
    {
        TryDicingLogger.Info("PlayTurnAutoForPlayer intercepted. idx=" + idx);
        if (AutoPlayController.TryPlay(__instance, idx))
        {
            return false;
        }

        return true;
    }
}
```

`return false` means custom selection succeeded and the original game method is skipped.

`return true` means custom selection could not handle this speed die, so vanilla auto play is allowed to continue as fallback.

### `AutoPlay\AutoPlayController.cs`

Path:

```text
src\tryDicing\AutoPlay\AutoPlayController.cs
```

Scope:

```text
This controller handles one intercepted vanilla auto-selection call.
```

Subject:

```text
The acting BattleUnitModel recovered from BattleAllyCardDetail._self and the speed die identified by idx.
```

Object:

```text
The cached BattlePlan and the SpeedDiceAction for this actor + idx.
```

Effect:

```text
It reads or creates a scene-level BattlePlan, finds the action for this actor + idx,
and sends that action to BattlePlanExecutor.
```

### `AutoPlay\TacticalPlanner.cs`

Current strategy shape:

Scope:

```text
This planner creates a BattlePlan for all available speed dice in the current actor faction.
```

Subject:

```text
All living actor units in the snapshot.
```

Object:

```text
Their usable hand cards, all targetable enemies, and target speed dice.
```

Effect:

```text
It creates SpeedDiceAction entries by enumerating legal candidates, scoring each candidate
with LocalActionEvaluator, and selecting the highest local score for each usable speed die.
```

```text
For each available speed die:
  1. Ask LegalActionFinder for legal card / target / target-slot candidates.
  2. Log the LegalActionSearchReport.
  3. Score each candidate with LocalActionEvaluator.
  4. Choose the highest local score.
  5. Reserve that card and light in the planner's simple planning state.
  6. Add the selected SpeedDiceAction to the BattlePlan.
```

Expected V1 logs:

```text
[tryDicing] LocalAction search actorId=..., idx=..., hand=..., light=..., candidates=..., clash=..., oneSided=...
[tryDicing] LocalAction selected actorId=..., idx=..., card=..., targetId=..., targetSlot=..., score=..., reason=...
```

### `AutoPlay\ActionExecutor.cs`

Scope:

```text
This executor applies exactly one planned speed-dice action to the original game state.
```

Subject:

```text
action.Actor and action.ActorSpeedDiceIndex.
```

Object:

```text
action.Card, action.Target, and action.TargetSpeedDiceIndex.
```

Effect:

```text
It is the only AutoPlay component that directly assigns a card into the game:

self.cardOrder = speedDiceIndex;
targetSlot = self.ChangeTargetSlot(card, target, speedDiceIndex, targetSlot, self.TeamKill());
self.cardSlotDetail.AddCard(card, target, targetSlot, false);
```

## Verified Hook Point

The current verified hook point is:

```csharp
BattleAllyCardDetail.PlayTurnAutoForPlayer(int idx)
```

Observed log:

```text
[tryDicing] PlayTurnAutoForPlayer intercepted. idx=0
```

`idx` is the speed dice index.

Examples:

```text
idx=0  first speed die
idx=1  second speed die
idx=2  third speed die
```

This confirms:

1. The mod is detected.
2. The DLL is loaded.
3. Harmony patching works.
4. `PlayTurnAutoForPlayer` is involved in the auto battle-page button.

## AutoBattle Probe

### `AutoBattle\AutoBattleProbePatch.cs`

Scope:

```text
This patch observes and advances the whole battle flow once per StageController.OnUpdate call.
```

Subject:

```text
The current StageController instance.
```

Object:

```text
StageController.State, StageController.Phase, StageController.battleState,
player speed dice assignment counts, and AutoBattleController.
```

Effect:

```text
It calls AutoBattleStateProbe.Sample(...) to log battle state changes,
then calls AutoBattleController.Update(...) to decide whether the mod should advance the battle.
```

### `AutoBattle\AutoBattleController.cs`

Scope:

```text
This controller handles one whole battle round at a coarse phase level.
It is intentionally above AutoPlay, which only chooses cards and targets for speed dice.
```

Subject:

```text
The current StageController and the current round number.
```

Object:

```text
The current battle phase:
RoundStartPhase_UI, RoundStartPhase_System, ApplyLibrarianCardPhase,
and the original StageController methods that correspond to Space / P / Space.
```

Effect:

```text
Per round, it advances the battle in this order:
1. Skip the round-start UI.
2. Stop speed dice rolling.
3. Ask the original auto-card button flow to assign player cards.
4. Complete the librarian card phase and start action resolution.
```

Current safeguards:

```text
Each action is attempted at most once per round.
Actions wait briefly after entering a phase before firing.
The controller resets its per-round flags when the round number changes or battle ends.
```

Expected controller logs:

```text
[tryDicing] AutoBattle round begin. round=...
[tryDicing] AutoBattle action: SkipRoundStartUi round=...
[tryDicing] AutoBattle action: StopSpeedDiceRoll round=...
[tryDicing] AutoBattle action: SetAutoCardForPlayer round=...
[tryDicing] AutoBattle action: CompleteApplyingLibrarianCardPhase round=...
```

### `AutoBattle\AutoBattleActionInvoker.cs`

Scope:

```text
This class is the narrow bridge from our mod code into original StageController methods.
```

Subject:

```text
The StageController instance passed from the OnUpdate patch.
```

Object:

```text
Original methods:
StageController.CheckInput(true)
StageController.StopSpeedDiceRoll()
StageController.SetAutoCardForPlayer()
StageController.CompleteApplyingLibrarianCardPhase(true)
```

Effect:

```text
It invokes the original methods by reflection and logs failures instead of crashing the whole update loop.
```

Why `CompleteApplyingLibrarianCardPhase(true)`:

```text
The original Space-key path calls CompleteApplyingLibrarianCardPhase(false).
The bool parameter is named auto in the decompiled code.
Using true lets the mod advance the phase even if the previous automated speed-dice step set the original input guard.
```

### `AutoBattle\AutoBattleStateReader.cs`

Scope:

```text
This reader classifies the current battle flow into a coarse AutoBattleState.
```

Subject:

```text
The current StageController.
```

Object:

```text
The current raw stage phase and player speed dice/card-slot state.
```

Effect:

```text
It maps raw LoR state into:
Unknown, NotInBattle, WaitingRoll, WaitingCards, ReadyToStartBattle, ResolvingBattle, BattleEnded.
```

Expected probe log:

```text
[tryDicing] AutoBattleState=WaitingRoll | rawState=Battle, phase=..., battleState=..., round=..., playerDice=..., assigned=..., broken=...
```

Current original methods found while investigating battle automation:

```text
StageController.StopSpeedDiceRoll()
StageController.SetAutoCardForPlayer()
StageController.ApplyPlayerCardAuto(BattleUnitModel, int)
StageController.CompleteApplyingLibrarianCardPhase(bool)
StageController.CheckInput(bool)
BattleUIInputController.CheckInputStopSpeedDice()
BattleUIInputController.CheckInputBattleStart()
BattleUIInputController.CheckInputRencounter()
```

## Build Command

From the workspace:

```powershell
dotnet build "C:\Users\User\Documents\library_of_ruina_mod開發\src\tryDicing\tryDicing.csproj" -c Release
```

Expected output:

```text
Build succeeded.
0 warnings
0 errors
```

After building, copy the compiled DLL inside the workspace:

```powershell
Copy-Item `
  -LiteralPath "C:\Users\User\Documents\library_of_ruina_mod開發\src\tryDicing\bin\Release\tryDicing.dll" `
  -Destination "C:\Users\User\Documents\library_of_ruina_mod開發\mod\tryDicing\Assemblies\tryDicing.dll" `
  -Force
```

Codex may update files inside the workspace, but should not modify the game installation directory.

## Manual Install

The user manually copies:

```text
C:\Users\User\Documents\library_of_ruina_mod開發\mod\tryDicing
```

to:

```text
F:\SteamLibrary\steamapps\common\Library Of Ruina\LibraryOfRuina_Data\Mods\tryDicing
```

Do not rely on Codex to write outside the workspace.

## Log Location

Library of Ruina is a Unity game. The useful log file is usually:

```text
C:\Users\User\AppData\LocalLow\Project Moon\LibraryOfRuina\Player.log
```

Equivalent path:

```text
%USERPROFILE%\AppData\LocalLow\Project Moon\LibraryOfRuina\Player.log
```

Search for:

```text
[tryDicing]
```

Useful expected lines:

```text
[tryDicing] Harmony smoke patch installed. fileLog=...
[tryDicing] PlayTurnAutoForPlayer intercepted. idx=0
```

If the mod does not load, search the log for:

```text
Exception
Could not load
Assembly
StageModInfo
Harmony
tryDicing
```

## Current References Used

### Local Game Files

Sample mod:

```text
F:\SteamLibrary\steamapps\common\Library Of Ruina\LibraryOfRuina_Data\Mods\ModSample
```

Important observed sample files:

```text
ModSample\StageModInfo.xml
ModSample\Assemblies\LOR_Sample_dll.dll
ModSample\SampleCode\SampleCode.cs
```

Game assembly inspected:

```text
F:\SteamLibrary\steamapps\common\Library Of Ruina\LibraryOfRuina_Data\Managed\Assembly-CSharp.dll
```

Relevant decompiled classes:

```text
BattleAllyCardDetail
BattlePlayingCardSlotDetail
BattleObjectManager
BattleUnitModel
BattleDiceCardModel
```

### Harmony Docs

Harmony introduction:

```text
https://harmony.pardeike.net/articles/intro.html
```

Harmony prefix patching:

```text
https://harmony.pardeike.net/articles/patching-prefix.html
```

Harmony annotations:

```text
https://harmony.pardeike.net/articles/annotations.html
```

## Next Milestone

Continue improving the V1 local-action strategy while keeping the tested control pipeline stable.

Current scoring can grow toward:

```text
+ can kill
+ can stagger
+ can intercept dangerous enemy attack
+ good resistance matchup
- light/resource pressure
- overkill
```

The next major strategy milestone after V1 is scene-level plan evaluation, where the
system evaluates the whole set of speed-dice assignments instead of only selecting
the best local action for each speed die.

## Safety Rule

Codex may edit only inside:

```text
C:\Users\User\Documents\library_of_ruina_mod開發
```

For paths outside the workspace, especially:

```text
F:\SteamLibrary\steamapps\common\Library Of Ruina
```

Codex should read only. The user manually copies or installs files.
