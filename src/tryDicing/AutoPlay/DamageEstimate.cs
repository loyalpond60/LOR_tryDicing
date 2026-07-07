public sealed class DamageEstimate
{
    public DamageEstimate(
        float minHpDamage,
        float expectedHpDamage,
        float maxHpDamage,
        float minBreakDamage,
        float expectedBreakDamage,
        float maxBreakDamage,
        int attackDiceCount,
        bool isApproximate,
        string reason)
    {
        MinHpDamage = minHpDamage;
        ExpectedHpDamage = expectedHpDamage;
        MaxHpDamage = maxHpDamage;
        MinBreakDamage = minBreakDamage;
        ExpectedBreakDamage = expectedBreakDamage;
        MaxBreakDamage = maxBreakDamage;
        AttackDiceCount = attackDiceCount;
        IsApproximate = isApproximate;
        Reason = reason;
    }

    public readonly float MinHpDamage;
    public readonly float ExpectedHpDamage;
    public readonly float MaxHpDamage;
    public readonly float MinBreakDamage;
    public readonly float ExpectedBreakDamage;
    public readonly float MaxBreakDamage;
    public readonly int AttackDiceCount;
    public readonly bool IsApproximate;
    public readonly string Reason;
}
