# LoR Auto Play Relevant Original APIs

This document records original Library of Ruina classes, fields, and methods that are relevant to the planned auto battle-page strategy mod.

Source inspected:

```text
F:\SteamLibrary\steamapps\common\Library Of Ruina\LibraryOfRuina_Data\Managed\Assembly-CSharp.dll
```

Inspection tool:

```text
C:\Users\User\Documents\library_of_ruina_mod開發\tools\dnSpy-netframework
```

## Target Feature

The target feature is:

```text
For each player speed die:
  1. Choose one usable battle page.
  2. Choose a target unit.
  3. Choose a target speed die / target slot.
  4. Apply the choice in-game.
```

## Description Format

When describing a function related to auto battle-page selection, use this structure:

```text
FunctionName(...)

Scope:
  What size of action this function controls.

Subject:
  The unit/object being operated on.

Object:
  The target unit/object/slot/card being selected or affected.

Effect:
  What the subject does to the object.
```

## Main Hook Point

### `BattleAllyCardDetail.PlayTurnAutoForPlayer(int idx)`

This is the currently verified original method used by the player auto battle-page button.

Scope:

```text
This function handles one speed die of one acting unit.
It decides which battle page this speed die uses and which enemy speed die it targets.
```

Subject:

```text
The acting BattleUnitModel stored inside BattleAllyCardDetail._self.
idx identifies which speed die of this acting unit is being handled.
```

Object:

```text
A selected BattleDiceCardModel from the acting unit's hand.
A target BattleUnitModel, usually an enemy.
A target speed die index on that target unit.
```

Effect:

```text
The acting unit places the selected battle page on its idx speed die.
That page is assigned to attack or clash with the selected target unit's selected speed die.
```

Observed by Harmony smoke test:

```text
[tryDicing] PlayTurnAutoForPlayer intercepted. idx=0
```

Meaning:

```text
idx = speed die index
idx=0 means the first speed die
idx=1 means the second speed die
```

Original behavior, simplified:

```text
1. Read cards in hand.
2. Remove Instance-type pages.
3. Keep cards that CheckCardAvailableForPlayer returns true for.
4. Score cards using card.GetPriority(0).
5. Randomly select among the highest-priority cards.
6. Pick a target with BattleObjectManager.GetTargetByCardForPlayer.
7. Randomly select a target speed die.
8. Call self.cardSlotDetail.AddCard(card, target, targetSlot, false).
```

Why it matters:

```text
This is the method we can patch to replace vanilla auto play with our own rule-based strategy.
```

Patch plan:

```csharp
[HarmonyPatch(typeof(BattleAllyCardDetail), "PlayTurnAutoForPlayer")]
public static class AutoPlayPatch
{
    public static bool Prefix(BattleAllyCardDetail __instance, int idx)
    {
        // Run our strategy.
        // return false to skip vanilla auto play.
        // return true to let vanilla auto play continue.
    }
}
```

## Unit Model

### `BattleUnitModel`

Represents a battle unit, including librarians and enemies.

Scope:

```text
This object represents one battle participant.
```

Subject:

```text
When used as self, this is the acting unit whose speed die is being assigned.
When used as target, this is the object being attacked, intercepted, or evaluated.
```

Object:

```text
Its speed dice, hand/deck state, selected card slots, HP, stagger gauge, resistances, buffs, and passives.
```

Effect:

```text
The strategy reads this object to decide whether the unit can act, what cards it can use,
what targets it can affect, and whether it can redirect an enemy speed die.
```

Important fields:

```csharp
public int id;
public Faction faction;
public List<SpeedDice> speedDiceResult;
public BattlePlayingCardSlotDetail cardSlotDetail;
public BattleAllyCardDetail allyCardDetail;
public BattleUnitBreakDetail breakDetail;
public BattleUnitPassiveDetail passiveDetail;
public BattleUnitTurnState turnState;
```

Important properties:

```csharp
public float hp { get; }
public int MaxHp { get; }
public int PlayPoint { get; }
public int MaxPlayPoint { get; }
public int speedDiceCount { get; }
```

Important methods:

```csharp
public int GetSpeed(int idx);
public SpeedDice GetSpeedDiceResult(int index);
public bool CheckCardAvailableForPlayer(BattleDiceCardModel card);
public bool CanChangeAttackTarget(BattleUnitModel target, int myIndex = 0, int targetIndex = 0);
public int ChangeTargetSlot(BattleDiceCardModel card, BattleUnitModel target, int currentSlot, int targetSlot, bool teamkill);
public List<BattleUnitModel> GetFixedTargets();
public bool IsTargetable(BattleUnitModel attacker);
public bool IsTargetable_theLast();
public bool IsBreakLifeZero();
public bool IsKnockout();
public AtkResist GetResistHP(BehaviourDetail detail);
public AtkResist GetResistBP(BehaviourDetail detail);
public bool TeamKill();
```

How we use it:

```text
BattleUnitModel is the core object for evaluating both allies and enemies.

For our strategy:
  - self is the acting librarian.
  - self.speedDiceResult[idx] is the current speed die.
  - self.allyCardDetail gives hand/deck state.
  - self.cardSlotDetail applies selected cards.
  - target.hp, target.breakDetail.breakGauge, and target resists are useful scoring inputs.
```

## Hand / Deck / Card Detail

### `BattleAllyCardDetail`

Represents a unit's battle-page state: hand, deck, discard, cards in use, and reserved cards.

Scope:

```text
This object manages one unit's battle pages.
```

Subject:

```text
The unit that owns this BattleAllyCardDetail.
```

Object:

```text
Cards in hand, deck, discard pile, use pile, and reserved pile.
```

Effect:

```text
The strategy reads GetHand() to find candidate cards.
Vanilla auto play uses PlayTurnAutoForPlayer(int idx) to select and assign one card for one speed die.
```

Important methods:

```csharp
public List<BattleDiceCardModel> GetHand();
public List<BattleDiceCardModel> GetDeck();
public List<BattleDiceCardModel> GetDiscarded();
public List<BattleDiceCardModel> GetUse();
public List<BattleDiceCardModel> GetAllDeck();
public void UseCard(BattleDiceCardModel card);
public void ReturnCardToHand(BattleDiceCardModel appliedCard);
public void DrawCards(int count);
public BattleDiceCardModel GetRandomCardInHand(Predicate<BattleDiceCardModel> condition = null);
public void PlayTurnAutoForPlayer(int idx);
```

How we use it:

```text
GetHand() gives candidate battle pages.
PlayTurnAutoForPlayer(int idx) is the method being patched.
UseCard() is usually called internally by AddCard(), so our strategy should normally call AddCard() instead of UseCard() directly.
```

Important note:

```text
BattleAllyCardDetail has a private field named _self.
If patching PlayTurnAutoForPlayer, we receive BattleAllyCardDetail __instance.
To get the acting BattleUnitModel, we may need reflection to read _self.
```

## Battle Page Model

### `BattleDiceCardModel`

Represents a battle page in hand/deck/use.

Scope:

```text
This object represents one usable battle page instance.
```

Subject:

```text
The acting unit uses this card from its hand.
```

Object:

```text
The target unit and target speed die selected for this card.
```

Effect:

```text
The card defines cost, range, dice, scripts, priority, and targeting constraints.
Those values decide whether the acting unit can use it and how valuable it is for a target.
```

Important methods:

```csharp
public string GetName();
public LorId GetID();
public DiceCardSpec GetSpec();
public int GetPriority(int speed);
public int GetCost();
public int GetOriginCost();
public List<DiceBehaviour> GetBehaviourList();
public bool IsExhaustOnUse();
public DiceCardSelfAbilityBase CreateDiceCardSelfAbilityScript();
public List<BattleDiceBehavior> CreateDiceCardBehaviorList();
public bool IsValidTarget(BattleUnitModel actor, BattleDiceCardModel card, BattleUnitModel target);
```

Important spec data:

```csharp
card.GetSpec().Ranged;
card.GetSpec().affection;
```

Common range values seen in vanilla logic:

```text
CardRange.Instance
CardRange.FarArea
CardRange.FarAreaEach
```

How we use it:

```text
GetCost() is used for resource checks.
GetPriority(speed) can be used as a baseline vanilla value.
GetBehaviourList() is used to estimate attack/defense dice strength.
GetSpec().Ranged helps avoid special pages in early versions.
```

## Dice Behaviour

### `LOR_DiceSystem.DiceBehaviour`

Represents one die on a battle page.

Important fields:

```csharp
public int Min;
public int Dice;
public BehaviourType Type;
public BehaviourDetail Detail;
public string Script;
public string Desc;
```

Examples:

```text
Min/Dice       dice range, such as 4-8
Type           attack / defense type
Detail         Slash / Penetrate / Hit / Guard / Evasion
Script         dice ability script id
```

How we use it:

```text
For simple scoring:
  average roll = (Min + Dice) / 2

For damage estimate:
  only attack dice should count as HP damage.

For stagger estimate:
  attack dice and some defense dice can contribute to break/stagger value.
```

## Speed Dice Slots / Applying Cards

### `BattlePlayingCardSlotDetail`

Represents the chosen battle pages assigned to a unit's speed dice this scene.

Scope:

```text
This object controls the battle pages assigned to one unit's speed dice for the current scene.
```

Subject:

```text
The acting BattleUnitModel that owns this cardSlotDetail.
```

Object:

```text
The acting unit's speed die slot, the selected card, the target unit, and the target speed die slot.
```

Effect:

```text
AddCard(card, target, targetSlot, false) places the selected card onto the acting unit's current cardOrder speed die,
then assigns that card to the target unit's targetSlot speed die.
```

Important fields:

```csharp
public List<BattlePlayingCardDataInUnitModel> cardAry;
public Queue<BattlePlayingCardDataInUnitModel> cardQueue;
```

Important properties:

```csharp
public int PlayPoint { get; }
public int ReservedPlayPoint { get; }
```

Important methods:

```csharp
public void AddCard(BattleDiceCardModel card, BattleUnitModel target, int targetSlot, bool isEnemyAuto = false);
public void ClearCardAbility();
public void ArrangeCardOrder();
public bool OnApplyCard(BattleDiceCardModel card);
public bool ReserveCost(int value);
public bool SpendCost(int value);
public int GetMaxPlayPoint();
```

How vanilla auto play applies a card:

```csharp
self.cardSlotDetail.AddCard(card, target, targetSlot, false);
```

How we should apply a card:

```text
Set self.cardOrder = idx first.
Then call self.cardSlotDetail.AddCard(card, target, targetSlot, false).
```

Why:

```text
For player card placement, AddCard() uses self.cardOrder to decide which speed die receives the page.
```

Useful checks:

```text
self.cardSlotDetail.cardAry[idx] == null
  means that speed die does not already have a selected page.

self.cardSlotDetail.PlayPoint - self.cardSlotDetail.ReservedPlayPoint
  approximates remaining usable light for selection.
```

## Selected Card Data

### `BattlePlayingCardDataInUnitModel`

Represents a battle page already placed on a speed die.

Important fields used by original logic:

```csharp
public BattleUnitModel owner;
public BattleDiceCardModel card;
public BattleUnitModel target;
public int slotOrder;
public int targetSlotOrder;
public int speedDiceResultValue;
public BattleUnitModel earlyTarget;
public int earlyTargetOrder;
```

How we use it:

```text
Enemy target.cardSlotDetail.cardAry[targetSlot] tells us whether that enemy speed die already has an action.

If target action's target is a librarian, we can score that action as an incoming threat.
```

## Unit / Target Lookup

### `BattleObjectManager`

Global object manager for battle units.

Scope:

```text
This object provides access to units currently in battle.
```

Subject:

```text
The strategy or vanilla auto play asks BattleObjectManager for valid units.
```

Object:

```text
Living ally and enemy BattleUnitModel objects.
```

Effect:

```text
The strategy uses it to find possible targets.
Vanilla auto play uses GetTargetByCardForPlayer(...) to choose a target unit for a selected card.
```

Important access point:

```csharp
BattleObjectManager.instance
```

Important methods:

```csharp
public IList<BattleUnitModel> GetList();
public List<BattleUnitModel> GetList(Faction faction);
public List<BattleUnitModel> GetAliveList(bool includeKnockout = false);
public List<BattleUnitModel> GetAliveList(Faction faction);
public List<BattleUnitModel> GetAliveList_opponent(Faction faction);
public BattleUnitModel GetTargetByCardForPlayer(BattleUnitModel actor, BattleDiceCardModel card, int idx, bool teamkill = false);
public BattleUnitModel GetTargetByCard(BattleUnitModel actor, BattleDiceCardModel card, int idx, bool teamkill = false);
public BattleUnitModel GetTargetByIndex(int index);
```

How vanilla auto play chooses target:

```csharp
BattleObjectManager.instance.GetTargetByCardForPlayer(self, card, idx, self.TeamKill());
```

How custom strategy code can get target candidates:

```text
BattleObjectManager.instance.GetAliveList(opposingFaction)
  -> filter target.IsTargetable(self)
  -> pass candidates to LegalActionFinder / LocalActionEvaluator
```

## Break / Stagger State

### `BattleUnitBreakDetail`

Represents stagger / break state.

Important fields:

```csharp
public int breakGauge;
public int breakLife;
public bool nextTurnBreak;
```

Important methods:

```csharp
public int GetDefaultBreakGauge();
public bool IsBreakLifeZero();
public void TakeBreakDamage(int damage, DamageType type, BattleUnitModel attacker, AtkResist atkResist, KeywordBuf keyword);
```

How we use it:

```text
target.breakDetail.breakGauge
  current stagger gauge.

target.breakDetail.GetDefaultBreakGauge()
  max/default stagger gauge for ratio estimates.

target.IsBreakLifeZero()
  target is staggered / broken.
```

## Resistance

Relevant methods on `BattleUnitModel`:

```csharp
public AtkResist GetResistHP(BehaviourDetail detail);
public AtkResist GetResistBP(BehaviourDetail detail);
```

Relevant detail values:

```text
BehaviourDetail.Slash
BehaviourDetail.Penetrate
BehaviourDetail.Hit
```

How we use it:

```text
When estimating damage:
  use GetResistHP(dice.Detail)

When estimating stagger damage:
  use GetResistBP(dice.Detail)
```

Possible scoring multipliers for first draft:

```text
Vulnerable  1.5
Weak        1.25
Normal      1.0
Endure      0.5
Resist      0.25
```

These are strategy heuristics, not confirmed exact damage formulas.

## Target Changing / Interception

Relevant methods on `BattleUnitModel`:

```csharp
public bool CanChangeAttackTarget(BattleUnitModel target, int myIndex = 0, int targetIndex = 0);
public int ChangeTargetSlot(BattleDiceCardModel card, BattleUnitModel target, int currentSlot, int targetSlot, bool teamkill);
```

How vanilla logic uses it:

```text
After choosing a target slot, vanilla calls ChangeTargetSlot(...)
to let passives/buffs adjust the final slot.
```

For our strategy:

```text
CanChangeAttackTarget(target, mySlot, targetSlot)
  tells whether our speed die can redirect/intercept the target's action.

If true and the target slot is attacking an ally, that candidate action should get a high score.
```

Scope:

```text
These methods evaluate or adjust the relationship between one acting speed die and one target speed die.
```

Subject:

```text
The acting unit's selected speed die.
```

Object:

```text
The target unit's selected speed die and its currently assigned action.
```

Effect:

```text
CanChangeAttackTarget checks whether the acting speed die can redirect or intercept the target speed die.
ChangeTargetSlot lets passives and buffs alter the final target slot before AddCard applies the choice.
```

## Current AutoPlay Architecture Shape

The current non-smoke version uses a scene-level plan, even though the original hook still enters one speed die at a time.

Scope:

```text
The original hook handles one call to PlayTurnAutoForPlayer(int idx).
Internally, the mod creates a BattlePlan for all available speed dice in the current actor faction.
```

Subject:

```text
The acting unit recovered from BattleAllyCardDetail._self starts the planning request.
TacticalPlanner then considers all living actors on that same faction.
```

Object:

```text
All usable hand cards for the actor faction's living units.
All targetable opposing units.
Target speed dice on those opposing units.
```

Effect:

```text
The planner creates SpeedDiceAction entries.
When vanilla calls PlayTurnAutoForPlayer(int idx), AutoPlayController executes the matching action for that actor + idx.
If execution succeeds, the Harmony Prefix returns false so vanilla auto play does not run.
```

Pseudo flow:

```text
Prefix(BattleAllyCardDetail __instance, int idx)
  self = __instance._self

  snapshot = BattleSnapshotReader.Read(self.faction)
  plan = AutoPlayCache.GetOrCreate(snapshot)
  action = plan.Find(self, idx)

  if action is missing:
    return true

  BattlePlanExecutor.Execute(action)

  return false
```

Action execution is intentionally narrow:

```text
ActionExecutor.Execute(action)

Scope:
  Execute one planned speed-dice action.

Subject:
  action.Actor's action.ActorSpeedDiceIndex.

Object:
  action.Card, action.Target, action.TargetSpeedDiceIndex.

Effect:
  self.cardOrder = speedDiceIndex;
  targetSlot = self.ChangeTargetSlot(card, target, speedDiceIndex, targetSlot, self.TeamKill());
  self.cardSlotDetail.AddCard(card, target, targetSlot, false);
```

## Immediate Next API Questions

Before writing a stronger strategy, these should be checked further:

```text
1. How mass attacks should be represented and selected.
2. Whether card.IsValidTarget(...) should be used before AddCard.
3. How personal EGO / floor EGO pages behave in auto selection.
4. Whether all player auto-button calls are per speed die or sometimes per unit/team.
5. Where the UI refresh happens after AddCard and whether manual refresh is ever needed.
```

For the first rule-based test, avoid special pages:

```text
CardRange.Instance
CardRange.FarArea
CardRange.FarAreaEach
```

Add special handling only after the normal-card flow works.
