# BuildProfile

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

Because of that, BuildProfile should describe combat axes instead of forcing one role.

## First-Version Fields

```text
buildKeywordProfile
combatIdentity
resourcePlan
pressureAxis
defenseRiskProfile
setupProfile
mechanicTags
keyCardPolicy
```

## buildKeywordProfile

buildKeywordProfile is the first simple way to infer build intent from passives
and deck text.

It should not try to fully understand every combo at first.

It should extract repeated or strategically important keywords from:

```text
passive effects
deck card effects
card effect tags
resource effects
status keywords
cost and hand manipulation text
```

Possible first-version keyword groups:

```text
primaryKeywords
secondaryKeywords
resourceKeywords
setupKeywords
payoffKeywords
```

Example keywords:

```text
draw
light
cost reduction
copy
charge
smoke
burn
bleed
fragile
strength
endurance
protection
stagger
clash
discard
singleton
on hit
on clash win
```

Purpose:

```text
Detect how a build probably creates value before full passive and card-script
modeling exists.
```

Usage:

```text
If copy and cost reduction appear often, copy actions may be setup actions.
If draw and light appear often, resource recovery actions may have higher build fit.
If burn or bleed appears often, status application may be proactive value.
If clash and protection appear often, intercepting threats may have higher build fit.
```

The profile modifies evaluation. It does not choose actions directly.

## combatIdentity

combatIdentity describes the build's main sources of combat value.

Treat these as weighted traits:

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

Purpose:

```text
Reward actions that match how the player-built character is meant to create value.
```

## resourcePlan

resourcePlan describes how the build manages light and hand size.

Possible first-version values:

```text
LowCostCycle
HighCostBurst
LightPositive
DrawHungry
DrawStable
EmotionSpike
Balanced
```

Purpose:

```text
Change how the evaluator values light cost, card draw, light recovery, saving pages, and spending resources.
```

Baseline rule:

```text
Without passive, card, or emotion-level effects:
  A character recovers 1 light per scene.
  A character draws 1 card per scene.
```

## pressureAxis

pressureAxis describes what kind of problem the build creates for the enemy.

Possible axes:

```text
hpPressure
staggerPressure
clashPressure
statusPressure
resourcePressure
mechanicPressure
```

Purpose:

```text
Help the strategy understand whether damage, stagger, clash control, status, or setup pressure should be valued more.
```

## defenseRiskProfile

defenseRiskProfile describes how acceptable it is for this character to take damage, stagger damage, or risk death.

Possible first-version values:

```text
Fragile
Normal
Durable
ClashReliable
GuardReliable
DodgeReliable
SacrificeAllowed
```

Important:

```text
Ally damage, stagger, or death is not always forbidden.
Some builds or passives may intentionally accept risk for payoff.
```

## setupProfile

setupProfile describes whether the build needs setup before payoff.

Keep this general at first:

```text
None
NeedsLightSetup
NeedsDrawSetup
NeedsMechanicSetup
NeedsBuffSetup
NeedsEnemyStagger
NeedsSafeWindow
```

Do not hard-code specific systems like Smoke or Charge into setupProfile at the first layer.

Use mechanicTags for specific mechanics.

## Long-Term Setup Value

Some builds create value by changing future affordability or payoff readiness
rather than by immediate damage.

Example:

```text
A high-cost key card becomes cheaper when more copies of that same card are in
hand.

Another card can copy that key card into hand.
```

In that case, the copy action is not mainly valuable because of its current
damage. It is valuable because it changes the future cost curve of the key card.

This kind of action should be treated as:

```text
build-specific setup
future option value
resource curve manipulation
```

Evaluation sketch:

```text
setupValue =
  futurePayoffValue(after setup)
- futurePayoffValue(before setup)
- setupCost
- timingRisk
- handClogRisk
```

Important questions:

```text
Does this setup make a future high-value payoff move from infeasible to feasible?
Does it only make an already feasible payoff cheaper?
Is there a real payoff window before the setup decays or becomes irrelevant?
Does taking the setup action leave a current threat unanswered?
```

Rule:

```text
Setup is high value when it opens a future payoff line that was otherwise closed.
Setup is medium value when it improves an already available line.
Setup is low value when the payoff window is absent or current survival is at risk.
```

## mechanicTags

mechanicTags describe specific mechanics the build cares about.

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

These are tags, not fixed architecture.

## keyCardPolicy

keyCardPolicy describes when important cards should be spent.

Possible first-version values:

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

Purpose:

```text
Prevent the strategy from wasting important pages just because their immediate local score is high.
```

## Usage Rule

BuildProfile should not directly choose actions.

It should modify evaluation:

```text
The same action may have different value depending on which build performs it.
```
