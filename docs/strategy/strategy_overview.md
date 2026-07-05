# Strategy Overview

This document describes the high-level strategy philosophy.

Detailed terms live in:

```text
victory_exchange_model.md
key_terms.md
build_profile.md
local_action_evaluation.md
plan_evaluation.md
implementation_versions.md
```

## Core Goal

The system should automatically play combat turns in a way that seeks victory under the player's fixed build constraints. Build intent can guide evaluation, but the primary strategic goal is to extract a winning line from the key pages, passives, decks, floor, and battle state the player supplied.

The player keeps control over:

```text
Key pages.
Passives.
Deck composition.
Build concept.
```

The program and agent handle:

```text
Choosing battle pages.
Choosing targets.
Intercepting threats.
Avoiding obvious waste.
Managing light and hand resources.
Choosing between local value and scene-level value.
Judging resource exchanges between HP, stagger, light, cards, actions, and status effects.
Seeking irreversible gains and preventing irreversible losses.
```

## Main Principle

The system should not start from abstract wishes like:

```text
Kill the boss now.
Always win every clash.
Always spend strongest cards.
```

It should start from:

```text
What resources are available now?
What legal actions are possible now?
What outcomes are feasible now?
Which objective-plan pair is best under those constraints?
```

## Exchange Philosophy

The strategy system should treat combat as resource exchange.

A plan spends or risks resources:

```text
HP.
Stagger margin.
Light.
Cards.
Speed dice.
Future action quality.
Temporary status windows.
```

It attempts to gain or preserve resources:

```text
Enemy HP damage.
Enemy stagger damage.
Enemy action suppression.
Enemy death.
Boss phase or mechanic progress.
Resource recovery.
A playable next scene.
```

The basic strategy is not simply maximum damage and minimum damage taken. The basic
strategy is to make exchanges that move the battle toward victory while preserving
future action ability.

The important strategic question is:

```text
Does this exchange create irreversible gain, prevent irreversible loss, or open a
winning future that would otherwise be closed?
```

Detailed definitions live in `victory_exchange_model.md`.

## Two Evaluation Layers

### Local Action

Local action evaluation asks:

```text
If only this speed die is considered, is this action useful?
```

It estimates things like:

```text
Legality.
Clash value.
Expected HP damage.
Expected stagger damage.
Resource value.
Setup value.
Enemy effect suppression.
Waste.
Build fit.
```

### Whole Plan

Plan evaluation asks:

```text
If all player speed dice are considered together, is this scene plan coherent?
```

It estimates things like:

```text
Threat coverage.
Outcome confirmation.
Resource future.
Ally risk management.
Team-wide overkill.
Build intent coherence.
Failure risk.
```

## Agent Role

The program should do rule-heavy and data-heavy work:

```text
Read state.
Enumerate legal actions.
Calculate scores.
Generate candidate plans.
Validate final decisions.
Execute final decisions.
```

The agent should do compressed tactical judgment:

```text
Choose among evaluated objective-plan pairs.
Notice strategic exceptions.
Explain why one evaluated plan is preferred.
Request focused detail only when needed.
```

The agent should not receive full raw battle dumps by default.




