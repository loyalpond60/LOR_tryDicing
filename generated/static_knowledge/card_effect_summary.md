# Card Effect Tag Summary

Purpose:

```text
This file summarizes card_effect_tags.json.
It is for checking whether static card tagging is useful before wiring it into C# strategy code.
```

Counts:

```text
cards: 1613
cardsWithAnyTag: 700
```

## All Tags

```text
power_gain: 268
bleed: 123
draw: 94
recover_hp: 70
recover_light: 69
weak: 59
burn: 57
stagger_damage: 55
paralysis: 46
bind: 37
smoke: 27
charge: 21
protection: 20
fragile: 6
cost_limit: 3
```

## Effect Tags

```text
bleed: 123
weak: 59
burn: 57
paralysis: 46
bind: 37
smoke: 27
charge: 21
protection: 20
fragile: 6
```

## Resource Tags

```text
draw: 94
recover_hp: 70
recover_light: 69
```

## Setup Tags

```text
status_setup: 352
power_setup: 268
stagger_setup: 55
```

## Risk Tags

```text
cost_limit: 3
```

## First Tagged Examples

```text
cardId=101001 cost=3 range=Near allTags=bleed abilityScripts=bleeding1atk
cardId=101002 cost=2 range=Near allTags=bleed abilityScripts=bleeding1atk
cardId=101003 cost=2 range=Near allTags=bleed,power_gain abilityScripts=bleeding2pw
cardId=101004 cost=1 range=Near allTags=bleed abilityScripts=bleeding1atk
cardId=101005 cost=1 range=Near allTags=bleed abilityScripts=bleeding1atk
cardId=102002 cost=2 range=Near allTags=fragile,power_gain abilityScripts=powerUpNext2pw,vulnerable2atk
cardId=102004 cost=2 range=Near allTags=power_gain abilityScripts=downgradeNext1pw
cardId=102005 cost=1 range=Near allTags=fragile abilityScripts=vulnerable1atk
cardId=102006 cost=1 range=Near allTags=power_gain,recover_light abilityScripts=energy1pw
cardId=103001 cost=2 range=Near allTags=paralysis abilityScripts=paralysis1atk
cardId=103002 cost=3 range=Near allTags=paralysis abilityScripts=paralysis2atk
cardId=103003 cost=0 range=Near allTags=paralysis,recover_light abilityScripts=energy1paralysis1
cardId=103004 cost=1 range=Near allTags=paralysis,power_gain abilityScripts=paralysis1atk,paralysis1pw
cardId=103005 cost=2 range=Near allTags=power_gain abilityScripts=strength2
cardId=104001 cost=2 range=Near allTags=bleed abilityScripts=bleeding2atk
cardId=104002 cost=2 range=Near allTags=bleed abilityScripts=bleeding1atk
cardId=104003 cost=2 range=Near allTags=bleed,power_gain abilityScripts=bleeding2atk,downgradeNext1pw
cardId=104004 cost=3 range=Near allTags=bleed abilityScripts=bleeding1atk
cardId=104005 cost=0 range=Near allTags=recover_hp abilityScripts=recoverHp1
cardId=104006 cost=2 range=Near allTags=recover_hp abilityScripts=recoverHp1atk
cardId=200003 cost=2 range=Near allTags=power_gain abilityScripts=upgradeNext1pw
cardId=201002 cost=1 range=Near allTags=paralysis abilityScripts=paralysis1atk
cardId=201003 cost=2 range=Near allTags=recover_hp abilityScripts=recoverHp4atk
cardId=201004 cost=1 range=Near allTags=bleed abilityScripts=bleeding2atk_pierre
cardId=201005 cost=2 range=Near allTags=bind abilityScripts=binding2atk
cardId=201006 cost=1 range=Near allTags=recover_hp abilityScripts=pierre
cardId=201007 cost=0 range=Near allTags=bleed abilityScripts=bleeding1atk_pierre
cardId=202001 cost=1 range=Near allTags=paralysis abilityScripts=paralysis1atk
cardId=202002 cost=1 range=Near allTags=power_gain abilityScripts=endurance1pw
cardId=202003 cost=2 range=Near allTags=burn abilityScripts=ruru2
```

