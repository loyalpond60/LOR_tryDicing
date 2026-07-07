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
       -> PlanSearch
            -> ThreatResponseMatrix
                 -> ThreatAssessor
                 -> ActionCandidateCollector
                      -> LegalActionFinder
                 -> ThreatResponseAssessor
            -> PlanEvaluator
  -> BattlePlanExecutor
  -> ActionExecutor
```

## Strategy Vocabulary

When discussing strategy, use these three concepts by default:

```text
DeclaredAction:
  What a unit has already assigned to a speed die.

ThreatAssessment:
  How dangerous an opposing DeclaredAction is if unanswered.

ThreatResponseAssessment:
  How one player ActionCandidate relates to one ThreatAssessment.
```

Support terms are implementation details. Introduce them only when the current
discussion needs the exact field or helper:

```text
DamageEstimate:
  Damage range used by threat and response assessment.

ResponseMechanism:
  The way a candidate responds to a threat.

OwnerDamageOutcome:
  The result when a candidate attacks the threat owner.

ThreatResponseMatrix:
  A collection of ThreatAssessment x ActionCandidate response assessments.
  It is V2 planning foundation data, not a planner by itself.
```

Short architecture rule:

```text
DeclaredAction is observed scene data.
ThreatAssessment is derived danger data.
ThreatResponseAssessment is derived relationship data.
ThreatResponseMatrix is only the collection of those relationships.
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
PlanSearch creates a first-version V2 scene plan.
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
Runs custom autoplay first. If custom autoplay executes or intentionally skips a
speed die, skip vanilla behavior. Only unrecoverable hook/setup failures should
fall back to vanilla behavior.
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
Coordinates reading battle state, obtaining a cached V2 plan, and executing the
action for the requested speed die. If the V2 plan has no action for that speed
die, the controller treats it as an intentional skip and prevents vanilla auto
play from filling it.
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
Player units, enemy units, declared actions, hand cards, light, HP, stagger, and speed dice data used by the prototype.
```

Effect:

```text
Creates a simplified snapshot that custom strategy code can read without spreading game-object access everywhere.
```

Current scene-model boundary:

```text
BattleSnapshot is the current SceneModel.
Do not add a separate SceneModel class until there is a concrete need to separate
runtime raw references from a compressed strategy-facing model.
```

Current declared-action read:

```text
BattleSnapshotReader reads BattlePlayingCardDataInUnitModel entries from
unit.cardSlotDetail.cardAry and stores them as DeclaredAction summaries.
DeclaredAction is observed scene data only. It does not assign threat scores.
```

Current player-resource read:

```text
BattleSnapshotReader reads a PlayerAvailableResources summary for the planned side.
PlayerAvailableResources is a team-level wrapper around ActorAvailableResources.
ActorAvailableResources keeps per-actor resource ownership:
  hand
  playPoint
  reservedPlayPoint
  remainingLight
  usable speed dice indices
ActionCandidateCollector uses this resource summary to produce V2 plan-search
candidates.
```

Current damage-estimate helper:

```text
DamageEstimator estimates ordinary attack-dice HP and stagger damage using
runtime target.GetResistHP(detail) and target.GetResistBP(detail).
It reports min / expected / max estimates so threat logic can distinguish
guaranteed, expected, and potential risk.
It is a shared helper for local-action, threat, and plan evaluation.
It is approximate. It feeds ThreatAssessor, ThreatResponseAssessor, PlanSearch
candidate priority, and PlanEvaluator scoring.
```

Current threat-assessment helper:

```text
ThreatAssessor reads opposing DeclaredAction entries and estimates unanswered
enemy threat. It uses DamageEstimator so HP pressure uses HP resistance and
stagger pressure uses BP resistance.
ThreatAssessment stores HP / stagger pressure ratios and separates
Guaranteed / Expected / Potential kill and stagger risk.
Current first-version level rules:
  Guaranteed or Expected kill / stagger risk is Critical.
  Potential kill / stagger risk is Major.
  HP or stagger pressure ratio >= 0.75 is Critical.
  HP or stagger pressure ratio >= 0.40 is Major.
  AttackDiceCount is retained for diagnostics but does not currently raise level
  or score, because multiple attack dice are already reflected in total damage.
ThreatAssessment is derived evaluation data, not observed scene data.
The current V2 planner uses ThreatAssessment through ThreatResponseMatrix and
PlanSearch.
```

Current threat-response helper:

```text
ThreatResponseAssessor compares one ThreatAssessment with one ActionCandidate.
It produces ThreatResponseAssessment relationship data.
ResponseMechanism and OwnerDamageOutcome are fields inside that relationship;
they should not be treated as separate strategy layers.
ThreatResponseMatrix groups those pairwise assessments for future full-scene
plan evaluation. It can be built from a BattleSnapshot by combining
ThreatAssessor and ActionCandidateCollector output. It does not choose actions
by itself; PlanSearch consumes it when building the V2 plan.
ThreatResponseAssessment is derived relationship data.
```

Current plan-evaluation helper:

```text
PlanEvaluator evaluates a selected List<ActionCandidate> with BattleSnapshot and
ThreatResponseMatrix. It returns PlanEvaluation with:
  IsLegal
  terminalProgress
  actionEconomyChange
  resourceFlowChange
  riskChange
  setupFutureValue
  cost
  waste
  totalScore
  explanation
It does not generate action sets by itself. PlanSearch uses it to rank selected
action sets.
```

Current plan-search helper:

```text
PlanSearch performs first-version threat-guided beam search over ActionCandidate
entries. It builds a ThreatResponseMatrix, creates a prioritized candidate pool,
uses PlanEvaluator to score selected action sets, and returns PlanSearchResult.
TacticalPlanner now converts its selected ActionCandidate entries into
SpeedDiceAction entries for execution.
If PlanSearch selects no action for a speed die, that speed die is intentionally
left unused instead of being filled by V1 or vanilla auto play.
```

### TacticalPlanner

Scope:

```text
Current prototype whole-scene V2 plan using PlanSearch.
```

Subject:

```text
BattleSnapshot.
```

Object:

```text
BattleSnapshot, PlanSearchResult, selected ActionCandidates, and selected
SpeedDiceActions.
```

Effect:

```text
Runs PlanSearch for the scene and converts legal selected ActionCandidates into
BattlePlan actions. It does not use V1 greedy fill for unselected speed dice.
```

### ActionCandidateCollector

Scope:

```text
All player-side usable speed dice in the current BattleSnapshot.
```

Subject:

```text
PlayerAvailableResources and existing LegalActionFinder output.
```

Object:

```text
A flat list of independently legal ActionCandidate entries.
```

Effect:

```text
Collects candidates for response-matrix and plan-search work. It does not score,
select, spend light, or remove cards.
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
