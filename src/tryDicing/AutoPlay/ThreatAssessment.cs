public sealed class ThreatAssessment
{
    public ThreatAssessment(
        DeclaredAction enemyAction,
        ThreatLevel level,
        float unansweredDangerScore,
        DamageEstimate damage,
        float hpPressureRatio,
        float breakPressureRatio,
        bool guaranteedKillRisk,
        bool expectedKillRisk,
        bool potentialKillRisk,
        bool guaranteedStaggerRisk,
        bool expectedStaggerRisk,
        bool potentialStaggerRisk,
        string reason)
    {
        EnemyAction = enemyAction;
        Level = level;
        UnansweredDangerScore = unansweredDangerScore;
        Damage = damage;
        HpPressureRatio = hpPressureRatio;
        BreakPressureRatio = breakPressureRatio;
        GuaranteedKillRisk = guaranteedKillRisk;
        ExpectedKillRisk = expectedKillRisk;
        PotentialKillRisk = potentialKillRisk;
        GuaranteedStaggerRisk = guaranteedStaggerRisk;
        ExpectedStaggerRisk = expectedStaggerRisk;
        PotentialStaggerRisk = potentialStaggerRisk;
        Reason = reason;
    }

    public readonly DeclaredAction EnemyAction;
    public readonly ThreatLevel Level;
    public readonly float UnansweredDangerScore;
    public readonly DamageEstimate Damage;
    public readonly float HpPressureRatio;
    public readonly float BreakPressureRatio;
    public readonly bool GuaranteedKillRisk;
    public readonly bool ExpectedKillRisk;
    public readonly bool PotentialKillRisk;
    public readonly bool GuaranteedStaggerRisk;
    public readonly bool ExpectedStaggerRisk;
    public readonly bool PotentialStaggerRisk;
    public readonly string Reason;
}
