using System.Collections.Generic;

public static class TacticalPlanner
{
    public static BattlePlan CreatePlan(BattleSnapshot snapshot)
    {
        List<SpeedDiceAction> actions = new List<SpeedDiceAction>();
        foreach (BattleUnitModel actor in snapshot.Actors)
        {
            PlanActorActions(actor, snapshot.Targets, actions);
        }

        return new BattlePlan(snapshot.PlanKey, actions);
    }

    private static void PlanActorActions(BattleUnitModel actor, List<BattleUnitModel> targets, List<SpeedDiceAction> actions)
    {
        if (actor == null || actor.allyCardDetail == null || actor.speedDiceResult == null)
        {
            return;
        }

        List<BattleDiceCardModel> availableCards = actor.allyCardDetail.GetHand();
        int remainingLight = actor.cardSlotDetail.PlayPoint - actor.cardSlotDetail.ReservedPlayPoint;

        for (int speedDiceIndex = 0; speedDiceIndex < actor.speedDiceResult.Count; speedDiceIndex++)
        {
            if (!CanUseSpeedDie(actor, speedDiceIndex))
            {
                continue;
            }

            LegalActionSearchResult searchResult = LegalActionFinder.Find(actor, speedDiceIndex, availableCards, remainingLight, targets);
            TryDicingLogger.Info(searchResult.Report.ToLogString());

            List<ActionCandidate> candidates = searchResult.Candidates;
            LocalActionEvaluation selected = SelectBestEvaluation(candidates, remainingLight);
            if (selected == null)
            {
                TryDicingLogger.Info("LocalAction no candidates actorId=" + actor.id + ", idx=" + speedDiceIndex);
                continue;
            }

            ActionCandidate candidate = selected.Candidate;
            actions.Add(new SpeedDiceAction(
                candidate.Actor,
                candidate.ActorSpeedDiceIndex,
                candidate.Card,
                candidate.Target,
                candidate.TargetSpeedDiceIndex,
                "LocalScore=" + selected.TotalScore.ToString("0.0")));

            TryDicingLogger.Info(string.Format(
                "LocalAction selected actorId={0}, idx={1}, candidates={2}, card={3}, cost={4}, targetId={5}, targetSlot={6}, score={7:0.0}, reason={8}",
                actor.id,
                speedDiceIndex,
                candidates.Count,
                candidate.Card.GetName(),
                candidate.Card.GetCost(),
                candidate.Target.id,
                candidate.TargetSpeedDiceIndex,
                selected.TotalScore,
                selected.Reason));

            availableCards.Remove(candidate.Card);
            remainingLight -= candidate.Card.GetCost();
        }
    }

    private static bool CanUseSpeedDie(BattleUnitModel actor, int speedDiceIndex)
    {
        if (actor.turnState == BattleUnitTurnState.BREAK || actor.IsBreakLifeZero() || actor.IsKnockout())
        {
            return false;
        }

        if (actor.speedDiceResult == null || speedDiceIndex < 0 || speedDiceIndex >= actor.speedDiceResult.Count)
        {
            return false;
        }

        if (actor.speedDiceResult[speedDiceIndex].breaked)
        {
            return false;
        }

        if (actor.cardSlotDetail == null || actor.cardSlotDetail.cardAry == null || speedDiceIndex >= actor.cardSlotDetail.cardAry.Count)
        {
            return false;
        }

        return actor.cardSlotDetail.cardAry[speedDiceIndex] == null;
    }

    private static LocalActionEvaluation SelectBestEvaluation(List<ActionCandidate> candidates, int remainingLight)
    {
        LocalActionEvaluation best = null;
        foreach (ActionCandidate candidate in candidates)
        {
            LocalActionEvaluation evaluation = LocalActionEvaluator.Evaluate(candidate, remainingLight);
            if (best == null || evaluation.TotalScore > best.TotalScore)
            {
                best = evaluation;
                continue;
            }

            if (evaluation.TotalScore == best.TotalScore && IsTieBreakerBetter(evaluation.Candidate, best.Candidate))
            {
                best = evaluation;
            }
        }

        return best;
    }

    private static bool IsTieBreakerBetter(ActionCandidate candidate, ActionCandidate currentBest)
    {
        if (candidate.Card.GetCost() != currentBest.Card.GetCost())
        {
            return candidate.Card.GetCost() > currentBest.Card.GetCost();
        }

        if (candidate.InteractionType != currentBest.InteractionType)
        {
            return candidate.InteractionType == InteractionType.Clash;
        }

        return candidate.Target.hp < currentBest.Target.hp;
    }
}
