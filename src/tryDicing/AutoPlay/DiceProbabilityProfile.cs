public sealed class DiceProbabilityProfile
{
    public DiceProbabilityProfile(
        float expectedAttackPower,
        float maxAttackPower,
        float expectedDefensePower,
        float maxDefensePower,
        int attackDiceCount,
        int defenseDiceCount)
    {
        ExpectedAttackPower = expectedAttackPower;
        MaxAttackPower = maxAttackPower;
        ExpectedDefensePower = expectedDefensePower;
        MaxDefensePower = maxDefensePower;
        AttackDiceCount = attackDiceCount;
        DefenseDiceCount = defenseDiceCount;
    }

    public readonly float ExpectedAttackPower;
    public readonly float MaxAttackPower;
    public readonly float ExpectedDefensePower;
    public readonly float MaxDefensePower;
    public readonly int AttackDiceCount;
    public readonly int DefenseDiceCount;

    public float ExpectedTotalPower
    {
        get { return ExpectedAttackPower + ExpectedDefensePower * 0.45f; }
    }
}
