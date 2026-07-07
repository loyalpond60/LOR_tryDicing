public sealed class ThreatResponseAssessment
{
    public ThreatResponseAssessment(
        ThreatAssessment threat,
        ActionCandidate candidate,
        ResponseMechanism mechanism,
        OwnerDamageOutcome ownerDamageOutcome,
        DamageEstimate ownerDamage,
        float responseScore,
        string reason)
    {
        Threat = threat;
        Candidate = candidate;
        Mechanism = mechanism;
        OwnerDamageOutcome = ownerDamageOutcome;
        OwnerDamage = ownerDamage;
        ResponseScore = responseScore;
        Reason = reason;
    }

    public readonly ThreatAssessment Threat;
    public readonly ActionCandidate Candidate;
    public readonly ResponseMechanism Mechanism;
    public readonly OwnerDamageOutcome OwnerDamageOutcome;
    public readonly DamageEstimate OwnerDamage;
    public readonly float ResponseScore;
    public readonly string Reason;
}
