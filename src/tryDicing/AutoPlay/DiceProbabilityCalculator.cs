using System.Collections.Generic;
using LOR_DiceSystem;

public static class DiceProbabilityCalculator
{
    public static DiceProbabilityProfile Calculate(BattleDiceCardModel card)
    {
        if (card == null)
        {
            return new DiceProbabilityProfile(0f, 0f, 0f, 0f, 0, 0);
        }

        List<DiceBehaviour> behaviours = card.GetBehaviourList();
        if (behaviours == null || behaviours.Count == 0)
        {
            float fallback = card.GetCost() * 4f;
            return new DiceProbabilityProfile(fallback, fallback, 0f, 0f, 0, 0);
        }

        float expectedAttackPower = 0f;
        float maxAttackPower = 0f;
        float expectedDefensePower = 0f;
        float maxDefensePower = 0f;
        int attackDiceCount = 0;
        int defenseDiceCount = 0;

        foreach (DiceBehaviour behaviour in behaviours)
        {
            if (behaviour == null)
            {
                continue;
            }

            float average = (behaviour.Min + behaviour.Dice) * 0.5f;
            if (IsAttackDie(behaviour))
            {
                expectedAttackPower += average;
                maxAttackPower += behaviour.Dice;
                attackDiceCount++;
            }
            else
            {
                expectedDefensePower += average;
                maxDefensePower += behaviour.Dice;
                defenseDiceCount++;
            }
        }

        return new DiceProbabilityProfile(
            expectedAttackPower,
            maxAttackPower,
            expectedDefensePower,
            maxDefensePower,
            attackDiceCount,
            defenseDiceCount);
    }

    private static bool IsAttackDie(DiceBehaviour behaviour)
    {
        return behaviour.Type.ToString() == "Atk";
    }
}
