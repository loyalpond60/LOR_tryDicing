# Victory Exchange Model

This document defines the current strategic philosophy for tryDicing.

It is a conceptual strategy document, not an implementation specification.
Its purpose is to keep future scoring, planning, agent integration, and learning work
anchored to one question:

```text
What exchange does this plan make, and does that exchange move the battle toward victory?
```

## Core Claim

Library of Ruina combat should be modeled first as resource exchange.

The player supplies fixed constraints:

```text
Key pages.
Passives.
Decks.
Floor and librarians.
Current battle state.
```

Within those constraints, the strategy system should seek victory by making favorable
exchanges over time.

The system does not need to preserve a build's aesthetic style unless a later user
configuration explicitly says so. Build information is still important because it
changes what exchanges are possible and how expensive they are.

## Resources

Core resources:

```text
HP.
Stagger gauge.
Light.
Cards in hand.
Cards remaining or recoverable later.
Speed dice and action opportunities.
```

Secondary resources:

```text
Buffs and debuffs.
E.G.O availability.
Emotion state.
Temporary page effects.
Passive-triggered resources.
Stage-specific counters or conditions.
```

Secondary resources matter because they change the value, cost, or timing of core
resource exchanges.

Examples:

```text
A healing effect converts a later condition into HP.
A strength buff converts a page into more damage.
A protection buff converts enemy actions into less HP loss.
A draw effect converts one action into future card availability.
A boss counter may convert ignored mechanics into future failure.
```

## Exchange

A plan is a proposed exchange.

It spends or risks some resources:

```text
Light.
Cards.
HP.
Stagger margin.
Speed dice.
Key page timing.
Future hand stability.
Future action quality.
```

It attempts to gain or preserve other resources:

```text
Enemy HP damage.
Enemy stagger damage.
Enemy action suppression.
Enemy death.
Boss phase progress.
Resource recovery.
Safe future actions.
A playable next scene.
```

The basic strategy for ordinary fights is to prefer exchanges that improve the battle
state while keeping future action ability alive.

A good plan does not merely maximize damage or minimize incoming damage. It improves
the exchange rate between what the player side spends and what the enemy side loses.

## Irreversible Gain

Irreversible gain is the part of an exchange that moves the battle toward victory in a
way that ordinary next-scene variance cannot easily erase.

Short definition:

```text
Irreversible gain = persistent advantage that closes bad futures or opens winning futures.
```

It is not necessarily permanent in an absolute sense. Some gains can be reversed, but
only if the enemy spends meaningful resources, loses timing, or gives up pressure.

### Strong Irreversible Gain

Strong irreversible gains directly change the future battle structure.

Examples:

```text
Enemy death.
Permanent enemy action reduction.
Clearing a wave.
Pushing a boss into a favorable phase.
Removing or exhausting a boss-specific resource.
Opening a damage window that could not otherwise exist.
```

### Semi-Irreversible Gain

Semi-irreversible gains create a window or positional advantage that must be converted
before it decays.

Examples:

```text
Staggering an enemy.
Canceling or beating a key enemy die.
Creating a burst window.
Stabilizing light and hand enough to keep playing.
Establishing a temporary buff state that enables a payoff.
```

These are valuable when the system can use them. They are less valuable when there is
no follow-up.

### Preventing Irreversible Loss

Some plans do not create obvious gain, but prevent a loss that would permanently damage
future winning chances.

Examples:

```text
Keeping an ally alive.
Preventing an ally from being staggered at a critical time.
Preserving a key speed die or action next scene.
Stopping a boss countdown or special mechanic.
Preventing a status effect that would break future resources.
```

This should be treated as exchange value, not as passive defense. Preserving future
action ability can be equivalent to gaining action advantage.

## Why Damage Is Not Enough

Damage is often useful, but it is not automatically irreversible gain.

A damage plan may be shallow if:

```text
The enemy keeps all meaningful actions.
The damage does not reach a kill, stagger, phase, or mechanic threshold.
The player spends key resources and becomes unplayable next scene.
The boss mechanic punishes the damage timing.
```

A lower-damage plan may be better if it:

```text
Removes a dangerous action.
Creates a confirmed stagger or kill next scene.
Preserves enough resources to keep acting.
Stops a boss mechanic from changing the exchange rules.
```

## Bosses And Exchange Distortion

Boss fights are difficult because they often rewrite the value of ordinary exchanges.

In ordinary fights, favorable damage, stagger, and resource trades may be enough.

In boss fights, the system must notice when the normal exchange model is distorted by:

```text
Phase thresholds.
Invulnerability or damage windows.
Punishment for attacking or failing to attack.
Special counters.
Forced target rules.
Scripted enemy pages.
Status requirements.
Long-term resource traps.
```

The intelligent layer should not be mystical. Its job is to detect when surface value
is not real value.

Examples:

```text
High damage may be bad if it triggers an unfavorable phase too early.
Low damage may be good if it disables a mechanic.
Accepting damage may be good if it opens the only winning line.
Conservative play may be bad if it only delays an unavoidable failure.
```

## Plan Evaluation Implication

PlanEvaluation should ask:

```text
What does this plan spend or risk?
What does it gain or preserve?
Which gains are irreversible or semi-irreversible?
Which irreversible losses does it prevent?
Does the exchange improve the long-term victory path?
What happens if the plan's primary conversion fails?
```

This keeps plan evaluation broader than local damage, clash success, or immediate
survival.

## Agent And Model Implication

Agents, LLMs, or neural models should not be treated as the intelligence itself.

The intelligence is the victory-oriented exchange process.

Agents or models may help with:

```text
Judging whether a proposed exchange is worth its risk.
Recognizing when ordinary exchange value is distorted by boss mechanics.
Comparing conservative and high-variance victory paths.
Explaining why a low-immediate-value plan may preserve or create long-term value.
Identifying when candidate plans are too narrow.
```

They should not bypass legality, invent unvalidated actions, or directly write game
state.

## Design Rule

When adding a new strategic feature, ask:

```text
Does this improve the system's ability to judge resource exchange?
Does this help identify irreversible gain or prevent irreversible loss?
Does this help detect when boss mechanics distort ordinary exchange value?
Does this preserve validated execution and legal action boundaries?
```

If the answer is no, the feature may be conceptually adjacent but should not become a
core strategy requirement yet.
