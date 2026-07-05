public sealed class LocalActionEvaluation
{
    public LocalActionEvaluation(
        ActionCandidate candidate,
        float cardPowerScore,
        float targetScore,
        float clashScore,
        float damageScore,
        float staggerScore,
        float resourcePenalty,
        float wastePenalty,
        float totalScore,
        string reason)
    {
        Candidate = candidate;
        CardPowerScore = cardPowerScore;
        TargetScore = targetScore;
        ClashScore = clashScore;
        DamageScore = damageScore;
        StaggerScore = staggerScore;
        ResourcePenalty = resourcePenalty;
        WastePenalty = wastePenalty;
        TotalScore = totalScore;
        Reason = reason;
    }

    public readonly ActionCandidate Candidate;
    public readonly float CardPowerScore;
    public readonly float TargetScore;
    public readonly float ClashScore;
    public readonly float DamageScore;
    public readonly float StaggerScore;
    public readonly float ResourcePenalty;
    public readonly float WastePenalty;
    public readonly float TotalScore;
    public readonly string Reason;
}
