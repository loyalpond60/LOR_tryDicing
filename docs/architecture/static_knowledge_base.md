# Static Knowledge Base

This document describes the first offline static-data export for Library of Ruina.

It is separate from `BattleSnapshot`.

```text
BattleSnapshot:
  Runtime battle state read from the current combat.

StaticKnowledgeBase:
  Static game definitions exported from BaseMod/StaticInfo.

BattleContext:
  Future strategy-facing view that combines runtime state with static lookups.
```

## Purpose

The strategy system should not ask an agent or neural model to read raw game XML.

Instead, static data should be normalized first:

```text
BaseMod/StaticInfo
  -> Python exporter
  -> generated/static_knowledge/*.json
Assembly-CSharp.dll
  -> dnSpy.Console decompile cache
  -> ability/passive effect extraction
  -> strategy summaries / lookups
  -> evaluator or future agent input
```

The static knowledge export is useful for:

```text
Card definitions.
Card dice, gameplay ability scripts, and non-strategy action scripts.
Passive definitions.
Key page stats and passives.
Emotion card definitions.
EGO page definitions linked to combat cards.
Stage waves and enemy ids.
Enemy book/deck ids.
Deck composition.
```

It is not the final source of legality.

Runtime legality should still use original game methods such as:

```text
actor.CheckCardAvailableForPlayer(card)
target.IsTargetable(actor)
card.IsValidTarget(actor, card, target)
actor.ChangeTargetSlot(...)
```

## Export Tool

Tool:

```text
tools/static_info_exporter/export_static_info.py
```

Default source:

```text
F:\SteamLibrary\steamapps\common\Library Of Ruina\LibraryOfRuina_Data\Managed\BaseMod\StaticInfo
```

Default output:

```text
generated/static_knowledge
```

Run from workspace:

```powershell
python .\tools\static_info_exporter\export_static_info.py
```

The tool reads outside the workspace but writes only inside the workspace.

Ability-effect tool:

```text
tools/static_info_exporter/export_ability_effects.py
```

It reads:

```text
generated/static_knowledge/cards.json
generated/static_knowledge/passives.json
generated/static_knowledge/key_pages.json
generated/static_knowledge/emotion_cards.json
generated/static_knowledge/emotion_egos.json
generated/decompiled/Assembly-CSharp/Assembly-CSharp/*.cs
```

If the decompiled cache is missing, it can call:

```text
tools/dnSpy-netframework/dnSpy.Console.exe
```

to decompile:

```text
F:\SteamLibrary\steamapps\common\Library Of Ruina\LibraryOfRuina_Data\Managed\Assembly-CSharp.dll
```

Run after `export_static_info.py`:

```powershell
python .\tools\static_info_exporter\export_ability_effects.py
```

## Current Output

Current generated files:

```text
cards.json
card_effect_tags.json
card_effect_summary.md
ability_effects.json
passive_effects.json
emotion_card_effects.json
emotion_card_profiles.json
ego_page_profiles.json
key_page_profiles.json
ability_effect_summary.md
passives.json
key_pages.json
stages.json
enemies.json
decks.json
emotion_cards.json
emotion_egos.json
export_errors.json
summary.md
```

Latest export counts:

```text
cards: 1613
passives: 808
key_pages: 625
stages: 134
enemies: 436
decks: 504
emotion_cards: 156
emotion_egos: 50
errors: 0
```

## Current Parser Scope

`cards.json` includes:

```text
id
name
rarity
range
cost
chapter
priority
script
behaviour dice list
abilityScripts
actionScripts
rough tags inferred from script names and descriptions
sourceFiles
```

`card_effect_tags.json` includes strategy-oriented card profiles:

```text
cardId
cost
range
rarity
chapter
priority
diceProfile
effectTags
resourceTags
setupTags
riskTags
allTags
abilityScripts
actionScripts
sourceFiles
```

`card_effect_summary.md` summarizes tag distribution for review:

```text
cards: 1613
cardsWithAnyTag: 704
draw: 94
recover_light: 69
recover_hp: 70
stagger_damage: 55
```

`ability_effects.json` includes script-level card ability extraction:

```text
scriptId
found
className
triggers
effectTypes
effects
```

It analyzes gameplay ability scripts only:

```text
Card.Script
Behaviour.Script
```

It intentionally ignores non-strategy action scripts:

```text
Behaviour.ActionScript
```

because those usually point to:

```text
BehaviourAction_*
DiceAttackEffect_*
```

which describe animation, motion, and visual effects rather than tactical state changes.

Current extraction result:

```text
card ability scripts: 1255
card ability scripts found: 1236
```

The extractor now also records structural markers for complex effects that are not
yet semantically simulated:

```text
BeforeRollDice -> dice_power_modifier
ChangeAttackTarget -> targeting_modifier
GetResistHP / GetResistBP -> resistance_modifier
GetDamageReduction / GetBreakDamageReduction -> damage_reduction
GetDamageIncreaseRate / GetBreakDamageIncreaseRate -> damage_increase
OnGiveDamage / BeforeGiveDamage / GetDamageFactor -> damage_modifier
GetStartHp -> start_hp_modifier
GetMinHp -> survival_floor
SetCost / ChangeCost -> cost_modifier
```

Example meaning:

```text
bleeding1atk:
  trigger = OnSucceedAttack
  effect = Add KeywordBuf.Bleeding 1 to target

energy1pw:
  trigger = OnWinParrying
  effect = RecoverPlayPointByCard(1) for owner
```

`passive_effects.json` includes passive ability extraction:

```text
passiveId
className
found
debugDesc
triggers
effects
profileTags
methodSummaries
  name
  returnType
  parameters
  conditions
  conditionHints
  returns
  numericHints
  diceStatBonuses
  keywordBufApplications
  cardIdChecks
  usesRandom
```

Example meaning:

```text
PassiveAbility_10001:
  effect = speed dice slot +1

PassiveAbility_230001:
  trigger = OnRoundEnd
  effect = Strength 2 to owner when an ally is dead
```

Current method-summary extraction result:

```text
passive methods summarized: 1602
passive methods with conditions: 979
passive methods with numeric hints: 381
passive methods with card id checks: 53
```

Example method-summary meanings:

```text
PassiveAbility_3007.BeforeRollDice:
  conditionHints = dice_detail=Slash
  numericHints = DiceStatBonus.power 1

PassiveAbility_10112.GetStartHp:
  return formula = hp * 3f / 4f

PassiveAbility_230001.OnRoundEnd:
  conditionHints = death_state_condition, unit_list
  keywordBufApplications = Strength 2

PassiveAbility_1404011.ChangeAttackTarget:
  cardIdChecks = 707402, 707404, 707405
  conditionHints = card_id_condition, uses_random
```

`key_page_profiles.json` combines key page stats with passive summaries:

```text
keyPageId
hp
stagger
speedMin
speedMax
passiveIds
effectTypes
profileTags
passiveSummaries
```

`key_pages.json` includes:

```text
id
name
hp
stagger
speedMin
speedMax
resistances
passive ids
rarity
chapter
sourceFiles
```

`emotion_cards.json` includes abnormality/emotion card definitions:

```text
emotion card id
name
positive/negative state
sephirah
emotion level fields
target type
script
sourceFiles
```

Emotion card ids are not globally unique across floors, so this export keeps all
records instead of deduplicating by id.

`emotion_card_effects.json` extracts decompiled `EmotionCardAbility_*` classes:

```text
scriptId
found
className
debugDesc
triggers
effects
effectTypes
profileTags
```

Current extraction result:

```text
emotion card abilities: 156
emotion card abilities found: 154
missing: liu1_atk, liu1_fire
```

`emotion_card_profiles.json` joins emotion card static rows to those extracted
ability effects:

```text
emotionCardId
name
state
sephirah
targetType
script
found
triggers
effectTypes
profileTags
effects
sourceFiles
```

`emotion_egos.json` includes floor EGO definitions:

```text
ego id
sephirah
card id
sourceFiles
```

`ego_page_profiles.json` links each EGO entry to its existing combat-card record
and card ability summaries:

```text
egoId
sephirah
cardId
cardFound
card cost/range/dice profile/ability scripts/action scripts
effectTypes
profileTags
abilitySummaries
sourceFiles
```

`stages.json` includes:

```text
stage id
waves
formation ids
enemy unit ids
invitation books
conditions
chapter
storyType
sourceFiles
```

`enemies.json` includes:

```text
enemy id
book id
deck id
drop tables
exp
height range
sourceFiles
```

`decks.json` includes:

```text
deck id
ordered card ids
card counts
sourceFiles
```

## Known Limits

The first exporter is an inventory layer, not a full semantic interpreter.

Current limitations:

```text
Card tags are rough and based on script/description strings.
card_effect_tags.json is for scoring hints, not exact effect simulation.
ability_effects.json and passive_effects.json are structural extraction from decompiled C#,
not a full C# semantic interpreter.
methodSummaries can extract common conditions, constants, formulas, and simple arrays,
but they do not execute arbitrary C# control flow.
Localized names may display incorrectly in some terminals.
Card scripts and passive scripts are not executed or simulated.
Emotion card and EGO profiles are still profile summaries, not full behavior simulations.
Nested BattleUnitBuf classes are detected through their method/effect text, but not fully modeled yet.
Complex passive/card effects are marked structurally before they are fully interpreted numerically.
Modded static data is not merged yet.
```

This is still valuable because it gives future evaluators a stable lookup layer.

## Future Use

Likely next consumers:

```text
BuildProfileInferer:
  Infer deck and key-page tendencies from key page passives and deck cards.

LocalActionEvaluator:
  Add draw, light recovery, stagger, status, and setup tags to action scoring.

PlanEvaluator:
  Use enemy stage/deck knowledge for threat and resource planning.

DecisionProvider / MCP:
  Expose focused static lookups instead of dumping raw XML to an agent.
```
