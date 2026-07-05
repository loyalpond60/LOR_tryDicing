# Battle Strategy Model

This document defines how the `tryDicing` strategy system should understand and evaluate Library of Ruina combat.

The document is intentionally built in small confirmed sections. Do not treat unfinished sections as final design.

## 1. Design Goal

This strategy system is not meant to replace the player's deckbuilding decisions.

The player is responsible for:

```text
Choosing key pages.
Assigning passives.
Building decks.
Deciding each character build's intended playstyle.
```

The program / agent is responsible for:

```text
Understanding the build supplied by the player.
Choosing reasonable combat actions during battle.
Assigning battle pages, speed dice, and targets.
Avoiding obvious waste such as severe overkill, poor light usage, or leaving dangerous enemy actions unanswered.
```

The core goal is:

```text
Automatically play combat turns in a way that respects the player's build intent
and approaches the kind of tactical choices a human player would make.
```

## 2. Responsibility Split

### Build Strategy

Build Strategy is about:

```text
What this character is designed to be.
```

It includes:

```text
Key page.
Passive setup.
Deck composition.
Character role.
Resource cycle.
Key battle pages.
Combo or setup plan.
```

This layer is mainly controlled by the player.

### Battle Strategy

Battle Strategy is about:

```text
What this character should do in the current scene.
```

It includes:

```text
Which battle page each speed die should use.
Which enemy or enemy speed die should be targeted.
Whether a dangerous enemy action should be intercepted.
Whether resources should be spent, conserved, or recovered.
Whether the current scene should focus on killing, staggering, defending, or setting up.
```

This layer is handled by the program and agent.

### Relationship

Battle Strategy should not reject or override Build Strategy.

Instead:

```text
Battle Strategy should first understand the player's Build Strategy,
then make tactical decisions based on the current combat state.
```

## 3. Decision Layers

The strategy system uses four decision layers:

```text
BuildProfile
LocalActionValue
PlanValue
Agent Tactical Decision
```

These layers separate:

```text
What the character is designed to do.
What one speed-dice action is worth.
What a full scene plan is worth.
What tactical objective should be pursued right now.
```

### 3.1 BuildProfile

BuildProfile is the character-understanding layer.

It answers:

```text
What did the player design this character to do?
```

Scope:

```text
One player-controlled character.
```

Subject:

```text
The character's key page, passive setup, deck, and current combat resources.
```

Object:

```text
Character role, resource rhythm, damage or stagger bias, risk profile,
combo needs, and key-card policy.
```

Effect:

```text
It tells later evaluators that the same action may have different value
depending on the build that performs it.
```

Example:

```text
Using a defensive page to intercept a dangerous enemy action:

Tank build:
  High value because it matches the character's role.

DamageDealer build:
  Medium value unless the interception is necessary.

Fragile resource build:
  Lower value because it may waste the character's intended role.
```

### 3.2 LocalActionValue

LocalActionValue is the single-action evaluation layer.

It answers:

```text
If only this speed die is considered, is this action worth taking?
```

Scope:

```text
One speed die of one acting unit.
```

Subject:

```text
The acting unit, the acting speed die, and the selected battle page.
```

Object:

```text
The target unit and, when applicable, the target speed die.
```

Effect:

```text
It estimates the direct value and risk of one legal action.
```

It may consider:

```text
Legality.
Whether the action creates a clash or one-sided attack.
Clash win rate.
Expected HP damage.
Expected stagger damage.
Expected damage taken by the actor.
Kill chance.
Stagger chance.
Overkill.
Light cost.
Card draw or light recovery.
Fit with the actor's BuildProfile.
```

It does not decide:

```text
Whether too many allies are targeting the same nearly dead enemy.
Whether another ally is left unprotected.
Whether the full scene spends too many resources.
Whether the full scene achieves the current tactical objective.
```

Those questions belong to PlanValue.

### 3.3 PlanValue

PlanValue is the full-scene evaluation layer.

It answers:

```text
If all player speed-dice actions are considered together,
is this scene plan tactically good?
```

Scope:

```text
All available player speed dice in the current scene.
```

Subject:

```text
A set of planned actions.
```

Object:

```text
All allied units, all enemy units, enemy speed dice,
current resources, and the current scene objective.
```

Effect:

```text
It estimates the tactical quality of the whole plan.
```

It may consider:

```text
Whether the most dangerous enemy actions are handled.
Whether fragile or endangered allies are protected.
Whether focus fire is useful or excessive.
Whether severe overkill is avoided.
Whether a kill or stagger is secured.
Whether next-scene resources remain playable.
Whether any usable speed dice are wasted.
Whether the plan matches the current tactical objective.
```

Example:

```text
Three high-damage actions all target the same enemy with 8 HP remaining.

LocalActionValue:
  Each action may look acceptable by itself.

PlanValue:
  The plan is poor if it causes severe overkill and ignores other threats.
```

### 3.4 Agent Tactical Decision

Agent Tactical Decision is the tactical-choice layer.

It answers:

```text
What should this scene try to accomplish?
```

Scope:

```text
The current scene, optionally considering expected next-scene consequences.
```

Subject:

```text
Battle summary, BuildProfiles, LocalActionEvaluations, and PlanEvaluations.
```

Object:

```text
The scene objective and final BattlePlan.
```

Effect:

```text
It chooses between multiple reasonable plans by deciding what matters most right now.
```

It may choose to:

```text
Play safely and survive.
Focus fire a dangerous enemy.
Stagger an enemy now and kill it later.
Recover light or cards for a future burst.
Let a durable character take a bad trade to protect a fragile character.
Spend key resources because the current opportunity is worth it.
```

It should not:

```text
Tell the player to change the build.
Reject the player's intended build style.
Bypass legal action constraints.
Operate keyboard or UI inputs directly.
```

### 3.5 Key Terms and Relationships

The strategy model should keep its key terms distinct, but they are not independent.

Current high-level relationship:

```text
BuildProfile
  ↓
BattleContext
  ↓
AvailableResources
  ↓
ActionCandidate
  ↓
LocalActionEvaluation
  ↓
FeasibleOutcome
  ↓
SceneObjectiveHypothesis
  ↓
CandidateBattlePlan
  ↓
PlanEvaluation
  ↓
AgentDecision
```

#### BuildProfile

BuildProfile describes what each player-built character is intended to do.

It is based on:

```text
Key page.
Passives.
Deck.
Player-provided build intent.
Current combat resources when relevant.
```

It affects how actions and plans are interpreted.

It does not directly choose actions.

#### BattleContext

BattleContext describes the current objective battle state.

It includes:

```text
Current scene / round.
All allied units.
All enemy units.
Speed dice.
Cards in hand.
Already assigned cards.
HP and stagger state.
Light and hand state.
Buffs, debuffs, and visible enemy actions.
```

BattleContext is the raw situation that strategy evaluation reads from.

#### AvailableResources

AvailableResources describes what the player side can actually use in the current scene.

It is derived from BattleContext.

It includes:

```text
Usable speed dice.
Speed values.
Current light.
Cards in hand.
Cards that can currently be played.
Available clash opportunities.
Available one-sided attack opportunities.
Available intercept opportunities.
Available resource recovery actions.
Available draw actions.
Available setup actions.
Key pages currently available or unavailable.
```

AvailableResources prevents the strategy from proposing objectives that the current hand, light,
speed dice, or targeting state cannot realistically support.

#### ActionCandidate

ActionCandidate represents one possible action for one player speed die.

It describes:

```text
Which actor acts.
Which actor speed die is used.
Which card is used.
Which target is selected.
Which target speed die is selected when applicable.
Whether the action is a clash or one-sided attack.
```

ActionCandidate may still need legality checks.

#### LocalActionEvaluation

LocalActionEvaluation is the evaluation report for one ActionCandidate.

It estimates:

```text
Legality.
Clash value.
Damage value.
Stagger value.
Defense value.
Resource value.
Setup value.
Enemy effect suppression value.
Waste.
Build fit.
```

It does not decide whether the full scene plan is good.

#### FeasibleOutcome

FeasibleOutcome describes what the current resources and evaluated actions make realistically possible this scene.

It is not a desired goal yet.

It is evidence for what goals may be worth considering.

Examples:

```text
Enemy A can probably be staggered.
Enemy B can probably be killed.
A dangerous enemy die can be intercepted.
The team can recover resources safely.
A build-specific mechanic can be advanced.
An enemy buff or status die can be prevented.
No safe kill is available this scene.
No safe stagger is available this scene.
```

FeasibleOutcome should be produced before SceneObjectiveHypothesis.

Important rule:

```text
Objectives must be derived from feasible outcomes, not from abstract wishes.
```

#### SceneObjectiveHypothesis

SceneObjectiveHypothesis is a hypothesis about what the current scene may try to accomplish.

It is not a fixed command.

It should be derived from FeasibleOutcomes.

It may propose goals such as:

```text
Survive a dangerous scene.
Stagger a specific enemy.
Kill a specific enemy.
Recover light or cards.
Set up a build-specific mechanic.
Spend resources for a payoff window.
Prevent an enemy mechanic from advancing.
```

The hypothesis should be checked against actual candidate plans.

If evaluations show that an objective is unsafe, unrealistic, or too expensive,
that objective should be revised, downgraded, or rejected.

#### CandidateBattlePlan

CandidateBattlePlan is a full set of selected player actions generated under a SceneObjectiveHypothesis.

It answers:

```text
If this is what the scene is trying to accomplish,
which actions should each available speed die take?
```

Because plans are generated under an objective hypothesis,
the same BattleContext may produce different CandidateBattlePlans for different hypotheses.

#### PlanEvaluation

PlanEvaluation evaluates a CandidateBattlePlan under its SceneObjectiveHypothesis.

It answers:

```text
Does this plan actually support the objective it was built for?
Are the costs and risks acceptable?
What happens if the plan fails?
```

PlanEvaluation should evaluate the objective-plan pair, not the plan in isolation.

#### AgentDecision

AgentDecision chooses the final evaluated objective-plan pair.

It should include:

```text
Selected objective hypothesis.
Selected battle plan.
Reason for selection.
Known risks.
Important rejected alternatives when useful.
```

#### Objective And Plan Relationship

Objective and plan should be treated as a pair.

```text
SceneObjectiveHypothesis proposes what the scene may try to do.
CandidateBattlePlan proposes how to do it.
PlanEvaluation checks whether that objective-plan pair is actually coherent.
AgentDecision chooses one evaluated objective-plan pair.
```

Important rule:

```text
The objective helps generate the plan.
The plan evaluation validates or rejects the objective.
The objective itself must be grounded in feasible outcomes.
```

This prevents the strategy from forcing a bad objective after evidence shows it is not realistic.

### 3.6 Program / Agent Responsibility Split

The strategy system should avoid sending raw battle data or the full search space to the agent.

Core rule:

```text
Program computes the search space and evaluations.
Agent chooses among compressed, evaluated strategic alternatives.
```

In other words:

```text
Stable rules, legality, enumeration, probability, and numeric evaluation belong to the program.
Tactical judgment, objective tradeoffs, and exception handling belong to the agent.
```

#### Program Responsibilities

The program should handle:

```text
Reading BattleContext from the game.
Deriving AvailableResources from BattleContext.
Reading or inferring BuildProfile data.
Enumerating legal ActionCandidates.
Rejecting illegal actions.
Calculating LocalActionEvaluations.
Deriving FeasibleOutcomes from local evaluations and resources.
Generating SceneObjectiveHypotheses.
Generating CandidateBattlePlans for each objective hypothesis.
Calculating PlanEvaluations.
Compressing battle state into agent-readable summaries.
Validating the final selected plan before execution.
Executing the final validated plan in-game.
```

The program should not ask the agent to:

```text
Guess whether an action is legal.
Manually enumerate all possible targets.
Calculate basic dice probabilities from raw dice lists.
Read complete card XML or passive data during battle.
Inspect full game object dumps.
Choose from thousands of unranked action combinations.
```

#### Agent Responsibilities

The agent should handle:

```text
Choosing between evaluated objective-plan pairs.
Reordering objective priorities when the situation calls for it.
Recognizing when a nominally high-scoring plan violates player intent.
Recognizing when a lower-scoring plan is strategically safer.
Explaining the selected plan at a tactical level.
Requesting additional focused information when the compressed report is insufficient.
```

The agent output should be small and structured:

```text
selectedPlanId
selectedObjectiveId
optionalPlanAdjustments
reason
knownRisks
fallbackPreference
```

The program must validate any selected or adjusted plan before execution.

#### Responsibility By Term

```text
BuildProfile:
  Program infers it.
  Player may override it.
  Agent may use it or make temporary tactical exceptions.

BattleContext:
  Program reads it.
  Agent receives only a compressed summary.

AvailableResources:
  Program derives it from BattleContext.
  Agent receives only resource bottlenecks and strategic implications.

ActionCandidate:
  Program enumerates it.
  Agent should not generate raw candidates from scratch.

LocalActionEvaluation:
  Program calculates it.
  Agent sees only ranked or relevant summaries.

FeasibleOutcome:
  Program derives likely outcomes from resources and local evaluations.
  Agent uses it to understand what objectives are realistic.

SceneObjectiveHypothesis:
  Program generates candidate hypotheses from FeasibleOutcomes.
  Agent may choose, reorder, refine, or reject them.

CandidateBattlePlan:
  Program generates candidate plans under objective hypotheses.
  Agent chooses or lightly adjusts among evaluated plans.

PlanEvaluation:
  Program calculates it.
  Agent uses the evaluation report for tactical judgment.

AgentDecision:
  Agent selects an evaluated objective-plan pair.
  Program validates and executes it.
```

#### Token Control Rule

The agent should receive:

```text
Battle summary.
Build summaries.
Top objective hypotheses.
Top candidate plans.
Plan evaluation summaries.
Known risks and rejected high-risk options when useful.
```

The agent should not receive by default:

```text
Full raw BattleContext.
Full card XML.
Full passive XML.
Full candidate action list.
Full dice-by-dice details for every possible action.
Large unfiltered logs.
```

If the agent needs more detail, it should request focused information through tools, such as:

```text
Inspect one unit.
Inspect one card.
Inspect one enemy action.
Inspect one candidate plan.
Compare two plans.
```

## 4. BuildProfile

BuildProfile describes how the strategy system understands a player-built character.

It is not a traditional RPG class label.

Library of Ruina builds often create value through mixed mechanisms:

```text
Winning clashes.
Dealing HP damage.
Applying stagger pressure.
Applying abnormal statuses or debuffs.
Building up a mechanic before a payoff turn.
Cycling light and cards.
Protecting allies by intercepting dangerous speed dice.
Turning setup into burst damage later.
```

Because of that, BuildProfile should not classify a character as only "tank", "damage dealer", or "support".

Instead, it should describe which combat axes the build cares about.

First-version BuildProfile fields:

```text
combatIdentity
resourcePlan
pressureAxis
defenseRiskProfile
setupProfile
mechanicTags
keyCardPolicy
```

### 4.1 combatIdentity

combatIdentity describes the build's main sources of combat value.

It should be treated as weighted traits rather than a single fixed role.

Possible traits:

```text
clashControl
hpDamage
staggerPressure
statusPressure
resourceCycle
mechanicSetup
mechanicPayoff
protection
burstFinisher
```

Examples:

```text
A strong clash build:
  clashControl: high
  protection: medium
  hpDamage: medium

A status setup build:
  statusPressure: high
  mechanicSetup: high
  mechanicPayoff: medium

A burst payoff build:
  mechanicPayoff: high
  burstFinisher: high
  resourceCycle: medium
```

Purpose:

```text
combatIdentity tells the evaluator what kinds of value should be rewarded for this character.
```

### 4.2 resourcePlan

resourcePlan describes how the build manages light and hand size.

Possible values:

```text
LowCostCycle
HighCostBurst
LightPositive
DrawHungry
DrawStable
EmotionSpike
Balanced
```

Meanings:

```text
LowCostCycle:
  The deck uses mostly low-cost pages and prefers stable repeated actions.

HighCostBurst:
  The deck relies on expensive payoff pages and should avoid spending them casually.

LightPositive:
  The deck has meaningful light recovery and can often spend more aggressively.

DrawHungry:
  The deck tends to run out of hand cards and should value draw effects.

DrawStable:
  The deck has enough draw to maintain hand size.

EmotionSpike:
  The deck may rely on emotion level increases to refill or expand light.

Balanced:
  No strong resource bias is known.
```

Purpose:

```text
resourcePlan changes how the evaluator values light cost, card draw,
light recovery, saving pages, and spending resources for a payoff.
```

### 4.3 pressureAxis

pressureAxis describes how the build applies pressure to the enemy.

It answers:

```text
What kind of problem does this character create for the enemy?
```

Possible axes:

```text
hpPressure
staggerPressure
clashPressure
statusPressure
resourcePressure
mechanicPressure
```

Meanings:

```text
hpPressure:
  The build mainly threatens enemy HP and kills.

staggerPressure:
  The build mainly threatens stagger / break.

clashPressure:
  The build mainly wins clashes or neutralizes dangerous enemy dice.

statusPressure:
  The build creates value by applying abnormal statuses, debuffs, or damage-over-time effects.

resourcePressure:
  The build creates value by improving its own light/card cycle or degrading the enemy's options.

mechanicPressure:
  The build creates pressure through a game-specific mechanic that may later create a payoff.
```

Purpose:

```text
pressureAxis helps the strategy avoid judging every page only by immediate HP damage.
```

### 4.4 defenseRiskProfile

defenseRiskProfile describes how safely this character can take bad trades or enemy attention.

Possible values:

```text
Fragile
Normal
Durable
ClashReliable
GuardReliable
DodgeReliable
SacrificeAllowed
```

Meanings:

```text
Fragile:
  The character should avoid taking hits or stagger pressure.

Durable:
  The character can accept some damage or unfavorable trades.

ClashReliable:
  The character is expected to handle important clashes.

GuardReliable:
  The character can use guard dice or defensive pages to reduce risk.

DodgeReliable:
  The character can use evade dice or evasive pages to reduce risk.

SacrificeAllowed:
  The character may take a short-term loss if it improves the overall plan.
```

Purpose:

```text
defenseRiskProfile affects which character should intercept dangerous enemy actions,
which character should be protected, and which risks are acceptable.
```

### 4.5 setupProfile

setupProfile describes whether the build needs conditions before its strongest actions become valuable.

First-version values:

```text
None
NeedsLightSetup
NeedsDrawSetup
NeedsMechanicSetup
NeedsBuffSetup
NeedsEnemyStagger
NeedsSafeWindow
```

Meanings:

```text
NeedsLightSetup:
  The build should recover or save light before payoff actions.

NeedsDrawSetup:
  The build should stabilize hand size before spending key pages.

NeedsMechanicSetup:
  The build depends on some game-specific mechanic before payoff.

NeedsBuffSetup:
  The build wants strength, endurance, protection, or other buffs before payoff.

NeedsEnemyStagger:
  The build wants the enemy to be staggered before spending key damage pages.

NeedsSafeWindow:
  The build wants a low-risk scene before spending setup or payoff pages.
```

Important:

```text
Concrete mechanics such as smoke, charge, burn, bleed, or custom mod mechanics
should be represented as mechanicTags, not as top-level setupProfile values.
```

Purpose:

```text
setupProfile lets the strategy accept short-term low damage when the action prepares a stronger future scene.
```

### 4.6 mechanicTags

mechanicTags describe concrete game-specific mechanics involved in the build.

Examples:

```text
smoke
charge
burn
bleed
fragile
strength
endurance
protection
custom
```

Purpose:

```text
mechanicTags let the evaluator and agent recognize specific build mechanics
without hard-coding every mechanic as a top-level profile category.
```

### 4.7 keyCardPolicy

keyCardPolicy describes how special or important battle pages should be used.

Possible policies:

```text
SpendFreely
PreferWhenClashing
PreferForKill
PreferForStagger
PreferWhenSafe
SaveForBoss
SaveUntilSetupReady
EmergencyOnly
```

Examples:

```text
Expensive payoff page:
  SaveUntilSetupReady or PreferForKill.

Strong defensive page:
  PreferWhenClashing or EmergencyOnly.

Light recovery page:
  More valuable when light is low.

Card draw page:
  More valuable when hand size is low.

Special page:
  SaveForBoss or EmergencyOnly.
```

Purpose:

```text
keyCardPolicy prevents important pages from being spent only because their immediate local score is high.
```

### 4.8 BuildProfile Usage Rule

BuildProfile does not decide actions directly.

Instead:

```text
BuildProfile changes how actions and plans are interpreted.
```

Example:

```text
The same expensive damage page may be interpreted differently:

HighCostBurst build:
  Save it until a payoff window.

burstFinisher identity:
  Use it when it can secure a kill.

resourceCycle identity:
  Avoid it unless the plan remains resource-stable.
```

## 5. LocalActionValue

LocalActionValue describes the direct tactical value of one legal speed-dice action.

It is not only a damage score.

It answers:

```text
If one acting unit uses one battle page on one speed die against one target,
what direct value, risk, waste, and future setup does that action create?
```

Scope:

```text
One speed die of one acting unit.
```

Subject:

```text
The acting unit, acting speed die, selected battle page, and current actor BuildProfile.
```

Object:

```text
The target unit and, when applicable, the target speed die and enemy action.
```

Effect:

```text
It creates a structured evaluation report that can be used by PlanValue and agent decision making.
```

### 5.1 Action Shape

A LocalActionValue evaluation is based on an action candidate:

```text
actor
actorSpeedDiceIndex
actorSpeed
card
target
targetSpeedDiceIndex
targetSpeed
interactionType
intentTags
```

### 5.2 interactionType

interactionType describes how the action is resolved by game rules.

First-version values:

```text
Clash
OneSidedAttack
Invalid
```

Meanings:

```text
Clash:
  The action contests an enemy speed die.

OneSidedAttack:
  The action attacks without an enemy speed die contesting it.

Invalid:
  The action is not legal and should not be selected.
```

interactionType should stay small and rules-focused.

It should not contain tactical goals such as protecting allies, preventing buffs, or setting up a mechanic.
Those belong to intentTags.

### 5.3 intentTags

intentTags describe what the action is trying to accomplish tactically.

An action may have multiple intent tags.

First-version tags:

```text
DealDamage
StaggerTarget
InterceptThreat
ProtectAlly
RecoverResource
SetupMechanic
PreventEnemyDamage
PreventEnemyStaggerDamage
PreventEnemyStatus
PreventEnemyBuff
PreventEnemyResourceGain
PreventEnemyMechanicSetup
```

Purpose:

```text
intentTags allow one action to carry multiple tactical meanings.
```

Example:

```text
A clash against an enemy die that was targeting a fragile ally may be:

interactionType:
  Clash

intentTags:
  InterceptThreat
  ProtectAlly
  PreventEnemyDamage
```

Another example:

```text
A low-damage page that increases the actor's build-specific mechanic may be:

interactionType:
  OneSidedAttack

intentTags:
  SetupMechanic
```

### 5.4 Evaluation Components

LocalActionValue should be represented as a report, not only as a single number.

First-version components:

```text
LegalityEvaluation
ClashEvaluation
DamageEvaluation
StaggerEvaluation
DefenseEvaluation
ResourceEvaluation
SetupEvaluation
EnemyEffectSuppressionEvaluation
WasteEvaluation
BuildFitEvaluation
localScore
explanation
```

### 5.5 LegalityEvaluation

LegalityEvaluation decides whether the action can be selected.

It should check:

```text
Whether the actor can act.
Whether the actor speed die exists and is not broken.
Whether the actor speed die is already assigned.
Whether the card is in hand.
Whether the actor has enough light.
Whether the card is available for the actor.
Whether the target is targetable.
Whether the target speed die exists when required.
Whether the card range and special restrictions are satisfied.
```

Rule:

```text
Invalid actions must not be selected, even if they would appear tactically valuable.
```

### 5.6 ClashEvaluation

ClashEvaluation estimates the value and risk of contesting an enemy die.

It may include:

```text
clashWinRate
clashDrawRate
clashLoseRate
expectedWinMargin
expectedLoseMargin
enemyDieSuppressionValue
```

Important:

```text
Winning a clash is not always valuable only because it deals damage.
It may also prevent the enemy die from applying damage, stagger damage,
statuses, buffs, resource gain, or mechanic setup.
```

First-version calculation may use dice min/max enumeration and ignore complex passives or scripts until those are modeled.

### 5.7 DamageEvaluation

DamageEvaluation estimates HP pressure.

It may include:

```text
expectedHpDamage
killChance
targetHpRemainingAfterExpectedDamage
damageEfficiency
```

It should consider:

```text
Attack dice.
Damage type.
Target HP resistance.
The target's remaining HP.
```

DamageEvaluation should not reward raw damage without considering waste.
Overkill is handled by WasteEvaluation and PlanValue.

### 5.8 StaggerEvaluation

StaggerEvaluation estimates stagger / break pressure.

It may include:

```text
expectedStaggerDamage
staggerChance
targetStaggerRemainingAfterExpectedDamage
staggerEfficiency
```

It should consider:

```text
Attack dice and relevant defensive dice.
Damage type.
Target stagger resistance.
The target's current stagger gauge.
```

Stagger value can be high even when immediate HP damage is low,
because stagger may create a future kill window or prevent enemy actions.

### 5.9 DefenseEvaluation

DefenseEvaluation estimates defensive and protection value.

It may include:

```text
expectedSelfHpLoss
expectedSelfStaggerLoss
incomingThreatReduced
allyProtectedValue
actorRiskAfterAction
```

It should consider:

```text
Whether this action intercepts an enemy action.
Who the enemy action was originally targeting.
How endangered that ally is.
How dangerous the enemy action is.
Whether the actor's defenseRiskProfile makes this trade acceptable.
```

### 5.10 ResourceEvaluation

ResourceEvaluation estimates light and card-flow value.

It may include:

```text
lightCost
expectedLightGain
expectedCardDraw
handSizeAfterAction
lightAfterAction
resourceStress
```

Baseline assumption:

```text
Without passive, card, or emotion-level effects:
  A character recovers 1 light per scene.
  A character draws 1 card per scene.
```

ResourceEvaluation allows low immediate value actions to be valid when they prepare future output,
for example by recovering light or drawing cards.

### 5.11 SetupEvaluation

SetupEvaluation estimates whether the action advances a future payoff condition.

It is separate from BuildFitEvaluation.

It may include:

```text
mechanicProgress
buffSetupValue
statusSetupValue
futurePayoffPotential
setupRisk
```

First-version rule:

```text
If the actor BuildProfile has NeedsMechanicSetup
and the action shares relevant mechanicTags,
the action may receive setup value.
```

Extensibility rule:

```text
SetupEvaluation should start as a coarse tag-based model.
Concrete mechanics such as smoke, charge, burn, bleed, or custom mechanics
can later add specialized evaluators without changing the high-level structure.
```

### 5.12 EnemyEffectSuppressionEvaluation

EnemyEffectSuppressionEvaluation estimates the value of preventing an enemy die from resolving.

This is necessary because clash value is not only about winning damage trades.

It may include:

```text
preventedHpDamage
preventedStaggerDamage
preventedStatusValue
preventedBuffValue
preventedResourceGainValue
preventedMechanicSetupValue
```

Examples:

```text
An enemy die may be important because it applies burn, bleed, paralysis, fragile, or another status.

An enemy die may be important because it grants strength, protection, light, cards, or a mechanic stack.

Preventing that die can be valuable even when the player's page deals little damage.
```

### 5.13 WasteEvaluation

WasteEvaluation estimates avoidable waste.

It may include:

```text
overkillDamage
overStaggerDamage
excessiveLightSpend
keyCardWaste
lowValueTargetPenalty
```

Purpose:

```text
WasteEvaluation prevents the strategy from spending high-value resources
only because an action has a high immediate damage estimate.
```

### 5.14 BuildFitEvaluation

BuildFitEvaluation estimates whether the action fits the actor's BuildProfile.

It may include:

```text
matchesCombatIdentity
supportsResourcePlan
advancesSetupProfile
respectsKeyCardPolicy
fitsPressureAxis
fitsDefenseRiskProfile
```

Difference from SetupEvaluation:

```text
SetupEvaluation asks:
  Does this action advance a future condition?

BuildFitEvaluation asks:
  Does this action make sense for this character's intended build?
```

Example:

```text
A low-damage mechanic page may have:
  High SetupEvaluation if it advances a needed mechanic.
  High BuildFitEvaluation if the build is designed around that mechanic.
```

### 5.15 LocalActionValue Usage Rule

LocalActionValue should not choose the full scene plan by itself.

Instead:

```text
It produces structured evidence for PlanValue and Agent Tactical Decision.
```

Core principle:

```text
Clashing is not the goal.
Damage is not the only goal.
The action's tactical purpose must be represented explicitly.
```

## 6. Implementation Versions

This section defines the intended implementation roadmap.

The purpose is to prevent the strategy system from trying to implement every concept at once.

Each version should produce a useful, testable system before the next layer is added.

### V0 Current Working Prototype

Goal:

```text
Prove that the mod can load, patch original methods, assign cards, and advance battle automatically.
```

Current status:

```text
Implemented.
```

Capabilities:

```text
Harmony patch installation.
Interception of BattleAllyCardDetail.PlayTurnAutoForPlayer(int idx).
Automatic battle flow through StageController phases.
Basic card assignment through ActionExecutor.
Simple rule-based action selection.
```

Current strategy:

```text
Choose the highest-cost playable non-special page.
Target the living enemy with the lowest HP.
Use the first available target speed die.
```

Purpose:

```text
Validate the control pipeline before building a smarter strategy.
```

### V1 Legal Action + Local Evaluation

Goal:

```text
Replace direct hard-coded action choice with legal action enumeration and LocalActionEvaluation.
```

Planned components:

```text
BattleContextReader
AvailableResourceReader
LegalActionFinder
ActionCandidate
DiceProbabilityCalculator
LocalActionEvaluator
```

Supported scope:

```text
Normal melee pages.
Clash and OneSidedAttack.
Basic legality checks.
Basic dice min/max probability.
Expected HP damage.
Expected stagger damage.
Basic overkill.
Light cost.
Basic card draw and light recovery value.
```

Out of scope:

```text
Full passive modeling.
Full card script modeling.
Mass attacks.
Counter dice.
Special stage mechanics.
Agent integration.
```

Strategy behavior:

```text
For each usable speed die, select the highest local-evaluation action.
```

Purpose:

```text
Make the strategy explainable at the single-action level.
```

### V1.5 Minimal BuildProfile

Goal:

```text
Let local evaluation understand a minimal version of player build intent.
```

Planned components:

```text
BuildProfile
BuildProfileInferer
Minimal UserBuildIntent override
ResolvedBuildProfile
KeyCardPolicy basics
MechanicTags basics
```

Supported scope:

```text
combatIdentity as coarse weighted traits.
resourcePlan.
pressureAxis.
defenseRiskProfile.
setupProfile.
mechanicTags.
keyCardPolicy.
```

Purpose:

```text
Prevent V1 scoring from becoming only a damage or clash score.
```

### V2 Scene Plan Evaluation

Goal:

```text
Evaluate a full scene plan instead of selecting each speed die independently.
```

Planned components:

```text
CandidateBattlePlanGenerator
PlanEvaluator
ThreatCoverageEvaluator
AllyRiskManagementEvaluator
ResourceFutureEvaluator
RedundancyWasteEvaluator
OutcomeConfirmationEvaluator
```

Supported scope:

```text
Avoid team-wide overkill.
Prefer useful focus fire.
Handle dangerous enemy dice.
Protect or intentionally risk allies based on payoff.
Estimate whether resources remain playable next scene.
Avoid redundant low-value actions.
```

Strategy behavior:

```text
Generate several candidate plans.
Select the highest PlanEvaluation result.
```

Purpose:

```text
Move from locally good actions to coherent scene-level tactics.
```

### V3 UserBuildIntent + ResolvedBuildProfile

Goal:

```text
Give the player a way to express intended build behavior without letting the system rewrite the build.
```

Planned components:

```text
UserBuildIntent configuration.
BuildProfile override / merge rules.
ResolvedBuildProfile.
Key card policy overrides.
Natural-language notes for later agent use.
```

Purpose:

```text
Preserve the player's deckbuilding and build-expression role
while allowing the strategy to play according to that intent.
```

### V4 FeasibleOutcome + Objective-Plan Pair

Goal:

```text
Derive scene objectives from feasible outcomes instead of abstract wishes.
```

Planned components:

```text
FeasibleOutcomeDeriver
SceneObjectiveHypothesisGenerator
ObjectivePlanPair
ObjectiveAwarePlanGenerator
ObjectiveAwarePlanEvaluator
```

Supported behavior:

```text
Derive likely kills.
Derive likely staggers.
Derive likely threat interceptions.
Derive likely resource recovery lines.
Derive likely setup progress.
Derive likely enemy-effect suppression.
Generate objective-plan pairs only when grounded in feasible outcomes.
```

Purpose:

```text
Let the strategy choose what the scene should try to accomplish
based on what current resources can actually support.
```

### V4.5 Mock / Rule AgentDecisionProvider

Goal:

```text
Test the AgentDecisionProvider interface before connecting an external agent.
```

Planned providers:

```text
MockAgentDecisionProvider
RuleAgentDecisionProvider
```

MockAgentDecisionProvider:

```text
Uses a fixed simple rule such as selecting the highest evaluated plan.
Its purpose is integration testing, not intelligence.
```

RuleAgentDecisionProvider:

```text
Uses local deterministic rules to choose among evaluated objective-plan pairs.
Its purpose is to provide a non-LLM baseline and fallback.
```

Purpose:

```text
Validate request/response structures, plan validation, fallback behavior,
and execution before MCP or LLM integration.
```

### V5 MCP Agent Integration

Goal:

```text
Allow an external agent to participate in tactical decision making through MCP-backed tools.
```

Planned components:

```text
Local Decision Bridge
MCP Server
AgentContextExporter
ExternalAgentDecisionProvider
PlanChoiceRequest
PlanChoiceResponse
ProgramValidator
Timeout and fallback handling
```

Agent input:

```text
Compressed battle summary.
Build summaries.
Feasible outcomes.
Top objective-plan pairs.
Plan evaluation summaries.
Known risks.
```

Agent output:

```text
selectedPlanId
selectedObjectiveId
optionalPlanAdjustments
reason
knownRisks
fallbackPreference
```

Purpose:

```text
Let an agent choose among compressed, evaluated strategic alternatives
without receiving raw game dumps or full candidate lists by default.
```

### V6 Advanced LoR Mechanics + Learning

Goal:

```text
Gradually model complex Library of Ruina mechanics and learn from battle results.
```

Possible additions:

```text
Mass attacks.
Counter dice.
Full ranged rules.
E.G.O pages.
Abnormality pages.
Emotion coins and emotion levels.
Complex passive scripts.
Complex card scripts.
Special stage mechanics.
Battle result logging.
Weight tuning.
Stage-specific knowledge.
Build performance feedback.
```

Purpose:

```text
Improve strategic accuracy after the core decision architecture is stable.
```

### Version Dependency

Primary dependency chain:

```text
V0 Current Working Prototype
  ↓
V1 Legal Action + Local Evaluation
  ↓
V1.5 Minimal BuildProfile
  ↓
V2 Scene Plan Evaluation
  ↓
V3 UserBuildIntent + ResolvedBuildProfile
  ↓
V4 FeasibleOutcome + Objective-Plan Pair
  ↓
V4.5 Mock / Rule AgentDecisionProvider
  ↓
V5 MCP Agent Integration
  ↓
V6 Advanced LoR Mechanics + Learning
```

Important note:

```text
Later design terms may be documented before they are implemented.
Implementation should still follow the version boundary unless there is a clear reason to change it.
```

## 7. Agent / MCP Integration

The external-agent design must keep the game mod separate from any chat UI or Codex thread control.

Core rule:

```text
The game mod must not directly control Codex or any chat interface.
The game mod communicates with a local bridge.
Agents participate through MCP-backed tools exposed by that bridge.
```

### 7.1 Architecture

Planned data flow:

```text
Library of Ruina Mod
  -> Local Decision Bridge
  -> MCP Server
  -> Agent / Codex thread
```

Decision flow back:

```text
Agent / Codex thread
  -> MCP Server
  -> Local Decision Bridge
  -> Library of Ruina Mod
```

### 7.2 Library of Ruina Mod Responsibilities

The mod should:

```text
Read current battle state.
Produce or request compressed decision data.
Pause decision progress during ApplyLibrarianCardPhase when external decision is enabled.
Send a decision request to the Local Decision Bridge.
Receive a selected plan or timeout/fallback result.
Validate that the selected plan is still legal.
Execute the validated plan in-game.
```

The mod should not:

```text
Open a Codex thread.
Control a Codex UI.
Depend on a specific chat window.
Send full raw game object dumps to the agent by default.
Execute unvalidated agent output.
```

### 7.3 Local Decision Bridge Responsibilities

The Local Decision Bridge should:

```text
Hold the current decision request.
Hold compressed battle summaries.
Hold feasible outcomes.
Hold candidate objective-plan pairs.
Hold plan evaluations.
Expose local HTTP or WebSocket endpoints for the mod.
Accept agent plan choices.
Return selected plans to the mod.
Handle timeouts.
Trigger fallback providers when needed.
```

The bridge is the boundary between the Unity mod and external agent tooling.

### 7.4 MCP Server Responsibilities

The MCP Server should wrap the Local Decision Bridge as tools.

It should not directly read game memory or modify game files.

First-version tools:

```text
get_battle_summary()
list_feasible_outcomes()
list_candidate_plans()
inspect_plan(planId)
submit_plan_choice(planId, reason)
```

Possible later tools:

```text
inspect_unit(unitId)
inspect_action(actionId)
compare_plans(planIdA, planIdB)
request_more_candidates(objectiveType)
```

Purpose:

```text
Let the agent query focused, compressed decision information
instead of receiving the full search space or raw game state.
```

### 7.5 Agent Responsibilities

The agent should:

```text
Read battle summaries.
Review feasible outcomes.
Review candidate objective-plan pairs.
Inspect specific plans or actions only when needed.
Choose an evaluated plan.
Submit a concise reason and known risks.
```

The agent should not:

```text
Invent raw illegal actions.
Guess card legality.
Manually calculate basic dice probabilities.
Request full card XML or passive XML by default.
Require full candidate action lists by default.
Operate keyboard, mouse, or game UI directly.
```

First-version agent output:

```text
selectedPlanId
selectedObjectiveId
reason
knownRisks
fallbackPreference
```

Advanced output may include:

```text
optionalPlanAdjustments
```

But every adjustment must be validated by the program before execution.

### 7.6 Timeout And Fallback

External agent decision making must be optional and recoverable.

Fallback cases:

```text
Agent does not respond before timeout.
Bridge is unavailable.
MCP server is unavailable.
Agent returns an unknown planId.
Agent returns an illegal plan adjustment.
Program validation fails.
```

Fallback order:

```text
RuleAgentDecisionProvider.
Highest PlanEvaluation candidate.
Current local rule-based strategy.
Vanilla auto play as last resort.
```

Purpose:

```text
The game should remain playable even when external agent integration fails.
```

### 7.7 Token Control

The MCP integration should preserve the existing token-control rule:

```text
Program computes the search space and evaluations.
Agent chooses among compressed, evaluated strategic alternatives.
```

Default agent context should include:

```text
Battle summary.
Build summaries.
Feasible outcomes.
Top candidate plans.
Plan evaluation summaries.
Known risks.
```

Default agent context should not include:

```text
Raw BattleContext.
Full card XML.
Full passive XML.
Full candidate action list.
Full dice-by-dice detail for every possible action.
Large unfiltered logs.
```

## 8. PlanEvaluation

PlanEvaluation evaluates a CandidateBattlePlan under a SceneObjectiveHypothesis.

It is not only a plan score.

It answers:

```text
Is this plan a coherent and acceptable way to pursue this objective hypothesis,
given the current resources and feasible outcomes?
```

### 8.1 Resource-Grounded Objective Rule

PlanEvaluation must treat the objective and plan as a pair.

It should not treat SceneObjectiveHypothesis as an unquestionable command.

Rule:

```text
A SceneObjectiveHypothesis must be grounded in AvailableResources and FeasibleOutcomes.
PlanEvaluation evaluates whether the CandidateBattlePlan supports the objective,
and whether the objective itself is realistic under current resources.
```

If the plan cannot support the objective, or the objective is unrealistic under current resources,
the objective-plan pair should be downgraded or rejected.

Example:

```text
SceneObjectiveHypothesis:
  Stagger Enemy A.

FeasibleOutcome:
  Current local evaluations suggest only a low stagger chance with available cards and light.

CandidateBattlePlan:
  Spends most available resources attempting to stagger Enemy A.

PlanEvaluation:
  Should penalize or reject the pair because the objective is not sufficiently supported by current resources.
```

Core principle:

```text
Objectives must come from what the current scene can plausibly accomplish,
not from abstract wishes.
```

### 8.2 Inputs

PlanEvaluation may use:

```text
BattleContext
AvailableResources
BuildProfiles
FeasibleOutcomes
SceneObjectiveHypothesis
CandidateBattlePlan
LocalActionEvaluations
```

### 8.3 Output Shape

PlanEvaluation should be a structured report.

First-version components:

```text
objectiveFit
outcomeConfirmation
threatCoverage
allyRiskManagement
resourceFuture
buildIntentCoherence
setupPayoffTiming
redundancyWaste
failureRisk
totalScore
explanation
```

totalScore is useful for ranking, but it should not replace the structured report.

### 8.4 objectiveFit

objectiveFit estimates whether the plan is actually trying to accomplish its SceneObjectiveHypothesis.

Examples:

```text
Objective:
  Recover resources.

High objectiveFit:
  Several actions recover light or draw cards while keeping risk acceptable.

Low objectiveFit:
  Most actions spend resources aggressively and do not improve next-scene stability.
```

```text
Objective:
  Stagger Enemy A.

High objectiveFit:
  Multiple actions apply meaningful stagger pressure to Enemy A.

Low objectiveFit:
  Only one low-stagger action targets Enemy A while the rest of the plan does unrelated work.
```

### 8.5 outcomeConfirmation

outcomeConfirmation estimates whether the plan has enough chance to secure the outcome it claims to pursue.

It is not limited to kill or stagger.

It may include:

```text
primaryOutcomeChance
secondaryOutcomeChance
confirmedOutcomes
unconfirmedClaims
```

Possible outcomes:

```text
Kill a target.
Stagger a target.
Preserve a key ally.
Prevent an enemy mechanic.
Complete player-side setup.
Safely pass a dangerous scene.
Put an enemy into a next-scene payoff state.
```

Purpose:

```text
outcomeConfirmation prevents the strategy from claiming an objective
when the plan has too little chance to actually achieve it.
```

### 8.6 threatCoverage

threatCoverage estimates whether important enemy threats are handled.

Threats include:

```text
High HP damage.
High stagger damage.
Dangerous statuses.
Enemy buffs.
Enemy resource gain.
Enemy mechanic setup.
Special stage effects.
```

It may include:

```text
coveredThreats
uncoveredThreats
highestUncoveredThreat
threatCoverageScore
```

Purpose:

```text
threatCoverage helps the plan avoid ignoring dangerous enemy actions
only because another target offers higher immediate damage.
```

### 8.7 allyRiskManagement

allyRiskManagement estimates whether allied risks are intentional, compensated, and acceptable.

It should not assume that all allied damage, stagger, or death is always bad.

It should ask:

```text
Does the plan expose a unit to expected HP loss or stagger loss that exceeds its safe margin,
without a matching payoff or intentional trigger?
```

It may include:

```text
expectedAllyDeaths
expectedAllyStaggers
intentionalSacrifices
acceptedDamageForTrigger
unplannedCriticalRisks
protectedKeyUnits
```

Purpose:

```text
allyRiskManagement allows intentional damage, stagger, or sacrifice when the build or plan benefits from it,
while still penalizing unplanned critical losses.
```

### 8.8 resourceFuture

resourceFuture estimates whether the team remains playable next scene.

It may include:

```text
lightAfterPlan
expectedLightNextScene
handAfterPlan
expectedHandNextScene
resourceStress
keyCardSpent
resourceRecoveryGained
```

It should consider the baseline rule:

```text
Without passive, card, or emotion-level effects:
  A character recovers 1 light per scene.
  A character draws 1 card per scene.
```

Purpose:

```text
resourceFuture lets the strategy accept lower immediate output
when resource recovery or card flow creates a better next scene.
```

### 8.9 buildIntentCoherence

buildIntentCoherence estimates whether the plan respects player build intent.

It should allow tactical exceptions.

It may include:

```text
charactersUsedAccordingToBuild
justifiedBuildExceptions
unjustifiedBuildViolations
keyCardPolicyViolations
```

Examples:

```text
A payoff character using a key page during a real payoff window:
  Coherent.

A payoff character spending a key page on a low-value target:
  Likely incoherent.

A damage-focused character intercepting a dangerous enemy die to prevent lethal damage:
  Justified exception.

A damage-focused character intercepting a low-threat die with no clear benefit:
  Unjustified violation.
```

### 8.10 setupPayoffTiming

setupPayoffTiming estimates whether the plan is using setup and payoff actions at the right time.

It may include:

```text
setupActions
payoffActions
prematurePayoff
missedPayoffWindow
setupProgress
```

It should consider:

```text
Whether resources are ready.
Whether build-specific mechanics are ready.
Whether the target is staggered or near stagger.
Whether the scene is safe enough.
Whether delaying payoff risks missing the opportunity.
```

Purpose:

```text
setupPayoffTiming prevents the strategy from always setting up
or always spending payoff pages without checking timing.
```

### 8.11 redundancyWaste

redundancyWaste estimates scene-level waste.

It may include:

```text
totalOverkill
totalOverStagger
redundantThreatCoverage
unusedSpeedDice
unnecessaryKeyCardSpend
```

This is different from single-action waste.

Example:

```text
Each action may be locally reasonable,
but the full plan may still be wasteful if all actions overcommit to a target already likely to die.
```

### 8.12 failureRisk

failureRisk estimates what happens if the primary objective is not achieved.

It may include:

```text
failureConsequence
fallbackState
riskIfPrimaryOutcomeFails
isPlanBStillPlayable
```

Purpose:

```text
failureRisk lets the strategy distinguish a high-upside safe plan
from a high-upside plan that collapses if its main outcome fails.
```

### 8.13 totalScore

totalScore is a ranking aid.

It should not be treated as the only decision signal.

Rule:

```text
AgentDecisionProvider may use totalScore for sorting,
but should also consider explanation, risks, outcome confirmation, and objective feasibility.
```

Purpose:

```text
totalScore helps compare plans quickly,
while the structured report preserves why the score exists.
```
