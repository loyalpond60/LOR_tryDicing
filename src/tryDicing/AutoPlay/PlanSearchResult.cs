using System.Collections.Generic;

public sealed class PlanSearchResult
{
    public PlanSearchResult(
        List<ActionCandidate> selectedActions,
        PlanEvaluation evaluation,
        ThreatResponseMatrix matrix,
        int candidatePoolCount,
        int evaluatedPlanCount,
        string reason)
    {
        SelectedActions = selectedActions ?? new List<ActionCandidate>();
        Evaluation = evaluation;
        Matrix = matrix;
        CandidatePoolCount = candidatePoolCount;
        EvaluatedPlanCount = evaluatedPlanCount;
        Reason = reason;
    }

    public readonly List<ActionCandidate> SelectedActions;
    public readonly PlanEvaluation Evaluation;
    public readonly ThreatResponseMatrix Matrix;
    public readonly int CandidatePoolCount;
    public readonly int EvaluatedPlanCount;
    public readonly string Reason;
}
