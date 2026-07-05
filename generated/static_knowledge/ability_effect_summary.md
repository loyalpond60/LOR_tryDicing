# Ability Effect Summary

Counts:

```text
card ability scripts: 1255
card ability scripts found: 1236
passive abilities: 813
passive abilities found: 738
passive methods summarized: 1602
passive methods with conditions: 979
passive methods with numeric hints: 381
passive methods with card id checks: 53
emotion card abilities: 156
emotion card abilities found: 154
key page profiles: 625
emotion card profiles: 156
ego page profiles: 50
```

## Card Effect Types

```text
add_keyword_buf: 318
dice_power_modifier: 71
stagger_damage: 66
recover_hp: 65
recover_light: 43
draw_cards: 34
damage_modifier: 14
deck_building_rule: 4
resistance_modifier: 3
speed_dice_num_adder: 1
on_take_damage_reaction: 1
```

## Passive Effect Types

```text
targeting_modifier: 80
dice_power_modifier: 60
speed_dice_num_adder: 57
add_keyword_buf: 53
cost_modifier: 48
draw_cards: 47
damage_modifier: 39
damage_reduction: 24
on_take_damage_reaction: 23
resistance_modifier: 19
recover_hp: 12
stagger_damage: 10
start_hp_modifier: 4
survival_floor: 2
damage_increase: 1
```

## Emotion Card Effect Types

```text
dice_power_modifier: 37
add_keyword_buf: 18
on_take_damage_reaction: 18
damage_modifier: 9
damage_reduction: 8
resistance_modifier: 3
recover_hp: 3
targeting_modifier: 2
draw_cards: 2
speed_value_adder: 1
stagger_damage: 1
```

## Key Page Profile Tags

```text
extra_speed_die: 199
power_modifier: 192
survival: 87
targeting_modifier: 60
card_draw: 48
resource_modifier: 48
strength_gain: 47
stagger_pressure: 19
bleed: 12
burn: 11
fragile: 9
protection: 8
paralysis: 8
endurance_gain: 8
weak: 6
stagger_protection: 4
keyword_Quickness: 1
bind: 1
keyword_Disarm: 1
keyword_KeterFinal_FailLying: 1
keyword_KeterFinal_SuccessLying: 1
```

## Emotion Card Profile Tags

```text
power_modifier: 46
survival: 27
strength_gain: 7
fragile: 4
bind: 3
targeting_modifier: 2
bleed: 2
paralysis: 2
card_draw: 2
keyword_Stun: 2
keyword_Decay: 2
stagger_pressure: 1
keyword_Quickness: 1
keyword_Disarm: 1
weak: 1
keyword_Alriune_Debuf: 1
speed_bonus: 1
```

## EGO Page Profile Tags

```text
power_modifier: 11
bleed: 5
strength_gain: 4
stagger_pressure: 4
survival: 3
bind: 3
weak: 3
keyword_Disarm: 2
light_recovery: 2
paralysis: 2
burn: 1
fragile: 1
keyword_DecreaseSpeedTo1: 1
card_draw: 1
extra_speed_die: 1
```

## Card Ability Examples

```text
scriptId=AddBullet1AndDraw found=True triggers=OnUseCard effects=draw_cards
scriptId=AddBullet2 found=True triggers=OnUseCard effects=
scriptId=AngelicaPuppet_OldBoy found=True triggers=OnUseCard effects=draw_cards,recover_light
scriptId=AngelicaPuppet_Special found=True triggers=BeforeRollDice effects=dice_power_modifier
scriptId=AngelicaPuppet_Wheels found=True triggers=OnWinParrying effects=
scriptId=AngelicaPuppet_Zelkova found=True triggers=OnUseCard effects=draw_cards,recover_light
scriptId=ClawEffect found=True triggers=OnUseCard effects=
scriptId=Final_ApocalypseBird_AbsoluteDmg found=True triggers=BeforeGiveDamage,GetMaximumPercentDmg effects=damage_modifier
scriptId=Final_ApocalypseBird_DecreaseLight1Atk found=True triggers=OnSucceedAreaAttack effects=
scriptId=Final_ApocalypseBird_GiveCharm found=True triggers=OnUseCard effects=
scriptId=Final_ApocalypseBird_GiveSin1Atk found=True triggers=OnSucceedAttack effects=
scriptId=Final_ApocalypseBird_GiveSin2Atk found=True triggers=OnSucceedAttack effects=
scriptId=Final_ApocalypseBird_Laser found=True triggers=OnEndBattle,OnSucceedAttack effects=add_keyword_buf
scriptId=Final_ApocalypseBird_MaxUpBySin found=True triggers=BeforeRollDice effects=dice_power_modifier
scriptId=Final_ApocalypseBird_PowerUpLastDice found=True triggers=OnSucceedAttack effects=
scriptId=Final_ApocalypseBird_RecoverHp7Atk found=True triggers=OnWinParrying effects=recover_hp
scriptId=Final_ApocalypseBird_RecoverHpAndBleed found=True triggers=OnSucceedAttack effects=add_keyword_buf,recover_hp
scriptId=Final_BigBird_Darkness found=True triggers=OnUseCard effects=
scriptId=Final_BigBird_Redemption found=True triggers=OnSucceedAttack effects=
scriptId=Jaeheon_AreaBind found=True triggers=OnSucceedAreaAttack effects=add_keyword_buf
scriptId=Jaeheon_AreaDt found=True triggers=OnUseCard effects=
scriptId=Jaeheon_Disarm1PwTr found=True triggers=OnWinParrying effects=
scriptId=Jaeheon_Dt1AD found=True triggers=OnLoseParrying effects=
scriptId=Jaeheon_Dt1FD found=True triggers=OnLoseParrying effects=
scriptId=Jaeheon_Dt1HD found=True triggers=OnLoseParrying effects=
scriptId=Jaeheon_Dt1ND found=True triggers=OnLoseParrying effects=
scriptId=Jaeheon_Mark found=True triggers=OnStartBattle effects=
scriptId=Jaeheon_Weak1PwTr found=True triggers=OnWinParrying effects=
scriptId=Pluto_AreaAtk found=True triggers=OnAfterAreaAtk effects=
scriptId=Pluto_Barrier found=True triggers=OnUseCard effects=
```

## Passive Ability Examples

```text
passiveId=3007 found=True triggers=BeforeRollDice effects=dice_power_modifier
passiveId=3008 found=True triggers=BeforeRollDice effects=dice_power_modifier
passiveId=3009 found=True triggers=BeforeRollDice effects=dice_power_modifier
passiveId=10001 found=True triggers=OnCreated,SpeedDiceNumAdder effects=speed_dice_num_adder
passiveId=10002 found=True triggers= effects=
passiveId=10003 found=True triggers= effects=
passiveId=10004 found=True triggers=OnCreated,SpeedDiceNumAdder effects=speed_dice_num_adder
passiveId=10005 found=True triggers= effects=
passiveId=10006 found=True triggers= effects=
passiveId=10007 found=True triggers= effects=
passiveId=10008 found=True triggers=OnCreated,SpeedDiceNumAdder effects=speed_dice_num_adder
passiveId=10010 found=True triggers= effects=
passiveId=10011 found=True triggers=OnRoundStart effects=
passiveId=10012 found=True triggers=OnRoundStart,OnUseCard,OnWaveStart effects=
passiveId=10013 found=True triggers=OnUseCard,OnWaveStart effects=draw_cards
passiveId=10112 found=True triggers=GetStartHp effects=start_hp_modifier
passiveId=101011 found=True triggers=OnDieOtherUnit,OnRoundEnd,OnRoundStart,OnSelectCardAuto,OnSucceedAttack effects=
passiveId=101012 found=True triggers=OnDieOtherUnit,OnRoundEnd effects=
passiveId=102011 found=True triggers=OnEndParrying,OnRoundStart effects=
passiveId=103011 found=True triggers=OnRoundStart effects=
passiveId=103012 found=True triggers=OnRoundEnd,OnRoundStart effects=recover_hp
passiveId=103021 found=True triggers=OnRoundEnd effects=add_keyword_buf
passiveId=103022 found=True triggers=OnRoundStart effects=recover_hp
passiveId=104011 found=True triggers=OnDie,OnRoundStart,SpeedDiceNumAdder effects=draw_cards,speed_dice_num_adder
passiveId=104012 found=True triggers=OnRoundStart,OnTakeDamageByAttack effects=on_take_damage_reaction
passiveId=104013 found=True triggers=OnRoundStart effects=
passiveId=104014 found=True triggers=GetResistBP,OnDieOtherUnit,OnRoundEndTheLast,OnRoundStart effects=resistance_modifier
passiveId=104021 found=True triggers=ChangeAttackTarget,OnRoundStart effects=targeting_modifier
passiveId=104022 found=True triggers= effects=
passiveId=105010 found=True triggers=OnRoundEndTheLast,OnWaveStart effects=
```

## Emotion Card Ability Examples

```text
scriptId=alriune1 found=True triggers=BeforeTakeDamage effects=
scriptId=alriune2 found=True triggers=OnTakeDamageByAttack effects=add_keyword_buf,on_take_damage_reaction
scriptId=alriune3 found=True triggers=Destroy,Init,IsTargetable,OnDie,OnRoundEnd,OnRoundStart,OnTakeDamageByAttack effects=on_take_damage_reaction
scriptId=bigbadwolf1 found=True triggers=OnSelectEmotion,OnSucceedAttack,OnUseCard,OnWinParrying effects=
scriptId=bigbadwolf2 found=True triggers=BeforeTakeDamage,DirectAttack,Init,IsTargetable,OnDie,OnRoundEnd,OnRoundStart,OnSelectEmotion,OnSuccessAttack,OnWaveStart effects=
scriptId=bigbadwolf3 found=True triggers=BeforeRollDice,OnRoundStart,OnSelectEmotion,OnWinParrying effects=dice_power_modifier
scriptId=bigbird1 found=True triggers=OnRoundEnd,OnRoundStart,OnSelectEmotion,OnWaveStart effects=
scriptId=bigbird2 found=True triggers=CanForcelyAggro,OnRoundStart effects=
scriptId=bigbird3 found=True triggers=OnParryingStart,OnRoundEnd,OnWinParrying effects=
scriptId=bind found=True triggers=OnRoundStart effects=
scriptId=blackswan1 found=True triggers=Destroy,OnDie,OnRoundEnd,OnRoundStart,OnWinParrying effects=add_keyword_buf
scriptId=blackswan2 found=True triggers=ChangeDamage,GetSpeedDiceAdder effects=
scriptId=blackswan3 found=True triggers=OnRoundEnd,OnRoundStart,OnSelectEmotion,OnWaveStart effects=
scriptId=bloodbath found=True triggers=BeforeRollDice,GetBreakDamageReduction,OnLayerChanged,OnSelectEmotion effects=damage_reduction,dice_power_modifier
scriptId=bloodbath2 found=True triggers=GetDamageReduction effects=damage_reduction
scriptId=bloodbath3 found=True triggers=GetCounter,OnRollDice,OnSucceedAttack effects=
scriptId=bloodytree1 found=True triggers=OnGiveDeflect effects=
scriptId=bloodytree2 found=True triggers=BeforeRollDice,OnSelectEmotion,OnWaveStart effects=dice_power_modifier
scriptId=bloodytree3 found=True triggers=BeforeRollDice,OnWinParrying effects=dice_power_modifier
scriptId=bluestar1 found=True triggers=OnSelectEmotion,OnWaveStart effects=
scriptId=bluestar2 found=True triggers=OnRoundEndTheLast effects=add_keyword_buf
scriptId=bluestar3 found=True triggers=BeforeRollDice,Init,OnEndBattlePhase,OnRoundEnd,OnRoundEndTheLast,OnRoundStart,OnSelectEmotion,OnWaveStart effects=dice_power_modifier
scriptId=bossbird1 found=True triggers=IsImmuneDmg,OnSelectEmotion effects=
scriptId=bossbird2 found=True triggers=BeforeRollDice,ChangeAttackTarget,Init,OnDie,OnParryingStart,OnRoundEnd,OnRoundStart,OnSelectEmotion,OnStartTargetedOneSide,OnUseCard,OnWinParrying effects=dice_power_modifier,targeting_modifier
scriptId=bossbird3 found=True triggers=OnSelectEmotion,OnWaveStart effects=
scriptId=bossbird4 found=True triggers=Init,OnRoundEnd,OnSelectEmotion,OnStartParrying,OnStartTargetedOneSide,OnWaveStart effects=
scriptId=bossbird5 found=True triggers=OnRoundStart effects=
scriptId=bossbird6 found=True triggers=BeforeRollDice,Destroy,Init,OnDie,OnRoundEnd,OnRoundStart,OnSucceedAttack,OnTakeDamageByAttack effects=dice_power_modifier,on_take_damage_reaction
scriptId=burnningGirl found=True triggers=OnRoundEnd,OnRoundStart,OnSuccessAttack,OnTakeDamageByAttack effects=on_take_damage_reaction
scriptId=burnningGirl2 found=True triggers=OnPrintEffect,OnSelectEmotion,OnStartCardAction effects=
```

