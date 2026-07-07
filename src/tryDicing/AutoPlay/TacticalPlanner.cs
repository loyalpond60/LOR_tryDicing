using System;
using System.Collections.Generic;

public static class TacticalPlanner
{
    public static BattlePlan CreatePlan(BattleSnapshot snapshot)
    {
        if (snapshot == null)
        {
            TryDicingLogger.Error("V2Plan failed: snapshot is null.");
            return new BattlePlan(string.Empty, new List<SpeedDiceAction>());
        }

        try
        {
            PlanSearchResult result = PlanSearch.Search(snapshot);
            LogV2PlanSearch(result);

            if (result == null || result.Evaluation == null || !result.Evaluation.IsLegal)
            {
                TryDicingLogger.Error("V2Plan rejected: search result is null, missing evaluation, or illegal.");
                return new BattlePlan(snapshot.PlanKey, new List<SpeedDiceAction>());
            }

            return new BattlePlan(snapshot.PlanKey, ConvertSelectedActions(result));
        }
        catch (Exception ex)
        {
            TryDicingLogger.Error("V2Plan search failed: " + ex);
            return new BattlePlan(snapshot.PlanKey, new List<SpeedDiceAction>());
        }
    }

    private static List<SpeedDiceAction> ConvertSelectedActions(PlanSearchResult result)
    {
        List<SpeedDiceAction> actions = new List<SpeedDiceAction>();
        if (result == null || result.SelectedActions == null)
        {
            return actions;
        }

        float totalScore = result.Evaluation == null ? 0f : result.Evaluation.TotalScore;
        foreach (ActionCandidate candidate in result.SelectedActions)
        {
            if (candidate == null || candidate.Actor == null || candidate.Card == null || candidate.Target == null)
            {
                TryDicingLogger.Error("V2Plan skipped invalid selected candidate during conversion.");
                continue;
            }

            actions.Add(new SpeedDiceAction(
                candidate.Actor,
                candidate.ActorSpeedDiceIndex,
                candidate.Card,
                candidate.Target,
                candidate.TargetSpeedDiceIndex,
                "V2PlanSearch score=" + totalScore.ToString("0.0")));
        }

        return actions;
    }

    private static void LogV2PlanSearch(PlanSearchResult result)
    {
        if (result == null)
        {
            TryDicingLogger.Error("V2Plan search returned null result.");
            return;
        }

        PlanEvaluation evaluation = result.Evaluation;
        float totalScore = evaluation == null ? 0f : evaluation.TotalScore;
        bool isLegal = evaluation != null && evaluation.IsLegal;
        int threatCount = result.Matrix == null || result.Matrix.Threats == null ? 0 : result.Matrix.Threats.Count;
        int responseCount = result.Matrix == null || result.Matrix.Responses == null ? 0 : result.Matrix.Responses.Count;

        TryDicingLogger.Info(string.Format(
            "V2Plan search pool={0}, evaluated={1}, selected={2}, score={3:0.0}, legal={4}, threats={5}, responses={6}, reason={7}",
            result.CandidatePoolCount,
            result.EvaluatedPlanCount,
            result.SelectedActions.Count,
            totalScore,
            isLegal,
            threatCount,
            responseCount,
            result.Reason));

        if (evaluation != null)
        {
            TryDicingLogger.Info(string.Format(
                "V2Plan eval terminal={0:0.0}, actionEconomy={1:0.0}, resource={2:0.0}, risk={3:0.0}, setup={4:0.0}, cost={5:0.0}, waste={6:0.0}, explanation={7}",
                evaluation.TerminalProgress,
                evaluation.ActionEconomyChange,
                evaluation.ResourceFlowChange,
                evaluation.RiskChange,
                evaluation.SetupFutureValue,
                evaluation.Cost,
                evaluation.Waste,
                evaluation.Explanation));
        }

        foreach (ActionCandidate action in result.SelectedActions)
        {
            LogV2PlanAction(action);
        }
    }

    private static void LogV2PlanAction(ActionCandidate action)
    {
        if (action == null)
        {
            return;
        }

        string cardName = action.Card == null ? "<null>" : action.Card.GetName();
        int cardCost = action.Card == null ? 0 : action.Card.GetCost();
        string actorId = action.Actor == null ? "<null>" : action.Actor.id.ToString();
        string targetId = action.Target == null ? "<null>" : action.Target.id.ToString();

        TryDicingLogger.Info(string.Format(
            "V2Plan action actorId={0}, idx={1}, card={2}, cost={3}, targetId={4}, targetSlot={5}, interaction={6}",
            actorId,
            action.ActorSpeedDiceIndex,
            cardName,
            cardCost,
            targetId,
            action.TargetSpeedDiceIndex,
            action.InteractionType));
    }
}
