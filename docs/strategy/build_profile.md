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
combatIdentity
resourcePlan
pressureAxis
defenseRiskProfile
setupProfile
mechanicTags
keyCardPolicy
```

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

