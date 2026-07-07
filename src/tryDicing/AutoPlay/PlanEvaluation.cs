public sealed class PlanEvaluation
{
    public PlanEvaluation(
        bool isLegal,
        float terminalProgress,
        float actionEconomyChange,
        float resourceFlowChange,
        float riskChange,
        float setupFutureValue,
        float cost,
        float waste,
        float totalScore,
        string explanation)
    {
        IsLegal = isLegal;
        TerminalProgress = terminalProgress;
        ActionEconomyChange = actionEconomyChange;
        ResourceFlowChange = resourceFlowChange;
        RiskChange = riskChange;
        SetupFutureValue = setupFutureValue;
        Cost = cost;
        Waste = waste;
        TotalScore = totalScore;
        Explanation = explanation;
    }

    public readonly bool IsLegal;
    public readonly float TerminalProgress;
    public readonly float ActionEconomyChange;
    public readonly float ResourceFlowChange;
    public readonly float RiskChange;
    public readonly float SetupFutureValue;
    public readonly float Cost;
    public readonly float Waste;
    public readonly float TotalScore;
    public readonly string Explanation;
}
