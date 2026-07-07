using System.Collections.Generic;
using LOR_DiceSystem;

public static class DamageEstimator
{
    public static DamageEstimate Estimate(BattleDiceCardModel card, BattleUnitModel attacker, BattleUnitModel target)
    {
        if (card == null || target == null)
        {
            return new DamageEstimate(0f, 0f, 0f, 0f, 0f, 0f, 0, true, "missing card or target");
        }

        List<DiceBehaviour> behaviours = card.GetBehaviourList();
        if (behaviours == null || behaviours.Count == 0)
        {
            return new DamageEstimate(0f, 0f, 0f, 0f, 0f, 0f, 0, true, "card has no dice behaviours");
        }

        float minHpDamage = 0f;
        float expectedHpDamage = 0f;
        float maxHpDamage = 0f;
        float minBreakDamage = 0f;
        float expectedBreakDamage = 0f;
        float maxBreakDamage = 0f;
        int attackDiceCount = 0;

        foreach (DiceBehaviour behaviour in behaviours)
        {
            if (behaviour == null || !IsAttackDie(behaviour))
            {
                continue;
            }

            float minRoll = behaviour.Min;
            float expectedRoll = (behaviour.Min + behaviour.Dice) * 0.5f;
            float maxRoll = behaviour.Dice;
            float hpRate = GetResistRate(target.GetResistHP(behaviour.Detail));
            float breakRate = GetResistRate(target.GetResistBP(behaviour.Detail));

            minHpDamage += minRoll * hpRate;
            expectedHpDamage += expectedRoll * hpRate;
            maxHpDamage += maxRoll * hpRate;
            minBreakDamage += minRoll * breakRate;
            expectedBreakDamage += expectedRoll * breakRate;
            maxBreakDamage += maxRoll * breakRate;
            attackDiceCount++;
        }

        string reason = string.Format(
            "approx ordinary attack dice estimate; attackDice={0}, minHp={1:0.0}, expectedHp={2:0.0}, maxHp={3:0.0}, minBreak={4:0.0}, expectedBreak={5:0.0}, maxBreak={6:0.0}",
            attackDiceCount,
            minHpDamage,
            expectedHpDamage,
            maxHpDamage,
            minBreakDamage,
            expectedBreakDamage,
            maxBreakDamage);

        return new DamageEstimate(
            minHpDamage,
            expectedHpDamage,
            maxHpDamage,
            minBreakDamage,
            expectedBreakDamage,
            maxBreakDamage,
            attackDiceCount,
            true,
            reason);
    }

    private static bool IsAttackDie(DiceBehaviour behaviour)
    {
        return behaviour.Type.ToString() == "Atk";
    }

    private static float GetResistRate(AtkResist resist)
    {
        switch (resist.ToString())
        {
            case "Weak":
                return 2f;
            case "Vulnerable":
                return 1.5f;
            case "Normal":
                return 1f;
            case "Endure":
                return 0.5f;
            case "Resist":
                return 0.25f;
            case "Immune":
                return 0f;
            default:
                return 1f;
        }
    }
}
