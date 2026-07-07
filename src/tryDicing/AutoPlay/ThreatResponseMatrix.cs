using System.Collections.Generic;

public sealed class ThreatResponseMatrix
{
    public ThreatResponseMatrix(
        List<ThreatAssessment> threats,
        List<ActionCandidate> candidates,
        List<ThreatResponseAssessment> responses)
    {
        Threats = threats ?? new List<ThreatAssessment>();
        Candidates = candidates ?? new List<ActionCandidate>();
        Responses = responses ?? new List<ThreatResponseAssessment>();
    }

    public readonly List<ThreatAssessment> Threats;
    public readonly List<ActionCandidate> Candidates;
    public readonly List<ThreatResponseAssessment> Responses;

    public static ThreatResponseMatrix Build(BattleSnapshot snapshot)
    {
        if (snapshot == null)
        {
            return new ThreatResponseMatrix(
                new List<ThreatAssessment>(),
                new List<ActionCandidate>(),
                new List<ThreatResponseAssessment>());
        }

        List<ThreatAssessment> threats = ThreatAssessor.Assess(snapshot);
        List<ActionCandidate> candidates = ActionCandidateCollector.Collect(snapshot);
        return Build(threats, candidates);
    }

    public static ThreatResponseMatrix Build(List<ThreatAssessment> threats, List<ActionCandidate> candidates)
    {
        List<ThreatResponseAssessment> responses = new List<ThreatResponseAssessment>();
        if (threats == null || candidates == null)
        {
            return new ThreatResponseMatrix(threats, candidates, responses);
        }

        foreach (ThreatAssessment threat in threats)
        {
            foreach (ActionCandidate candidate in candidates)
            {
                responses.Add(ThreatResponseAssessor.Assess(threat, candidate));
            }
        }

        return new ThreatResponseMatrix(threats, candidates, responses);
    }
}
