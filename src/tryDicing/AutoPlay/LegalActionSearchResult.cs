using System.Collections.Generic;

public sealed class LegalActionSearchResult
{
    public LegalActionSearchResult(List<ActionCandidate> candidates, LegalActionSearchReport report)
    {
        Candidates = candidates;
        Report = report;
    }

    public readonly List<ActionCandidate> Candidates;
    public readonly LegalActionSearchReport Report;
}
