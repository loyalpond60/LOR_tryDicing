using System.Collections.Generic;

public static class PlanEvaluator
{
    public static PlanEvaluation Evaluate(
        BattleSnapshot snapshot,
        List<ActionCandidate> selectedActions,
        ThreatResponseMatrix matrix)
    {
        List<ActionCandidate> actions = selectedActions ?? new List<ActionCandidate>();
        ThreatResponseMatrix responseMatrix = matrix ?? ThreatResponseMatrix.Build(snapshot);
        bool isLegal = IsLegal(snapshot, actions);

        float terminalProgress = CalculateTerminalProgress(actions);
        float actionEconomyChange = CalculateActionEconomyChange(actions, responseMatrix);
        float riskChange = CalculateRiskChange(responseMatrix, actions);
        float cost = CalculateCost(actions);
        float waste = CalculateWaste(actions, responseMatrix);
        float resourceFlowChange = CalculateResourceFlowChange(snapshot, actions);
        float setupFutureValue = 0f;
        float totalScore = terminalProgress
            + actionEconomyChange
            + resourceFlowChange
            + riskChange
            + setupFutureValue
            - cost
            - waste;

        if (!isLegal)
        {
            totalScore -= 1000f;
        }

        string explanation = string.Format(
            "legal={0}, terminal={1:0.0}, actionEconomy={2:0.0}, resourceFlow={3:0.0}, risk={4:0.0}, setup={5:0.0}, cost={6:0.0}, waste={7:0.0}, total={8:0.0}",
            isLegal,
            terminalProgress,
            actionEconomyChange,
            resourceFlowChange,
            riskChange,
            setupFutureValue,
            cost,
            waste,
            totalScore);

        return new PlanEvaluation(
            isLegal,
            terminalProgress,
            actionEconomyChange,
            resourceFlowChange,
            riskChange,
            setupFutureValue,
            cost,
            waste,
            totalScore,
            explanation);
    }

    private static bool IsLegal(BattleSnapshot snapshot, List<ActionCandidate> actions)
    {
        if (actions == null)
        {
            return true;
        }

        Dictionary<BattleUnitModel, HashSet<int>> usedSpeedDice = new Dictionary<BattleUnitModel, HashSet<int>>();
        Dictionary<BattleUnitModel, HashSet<BattleDiceCardModel>> usedCards = new Dictionary<BattleUnitModel, HashSet<BattleDiceCardModel>>();
        Dictionary<BattleUnitModel, int> spentLight = new Dictionary<BattleUnitModel, int>();

        foreach (ActionCandidate action in actions)
        {
            if (action == null || action.Actor == null || action.Card == null)
            {
                return false;
            }

            if (!AddUsedSpeedDie(usedSpeedDice, action.Actor, action.ActorSpeedDiceIndex))
            {
                return false;
            }

            if (!AddUsedCard(usedCards, action.Actor, action.Card))
            {
                return false;
            }

            AddSpentLight(spentLight, action.Actor, action.Card.GetCost());
        }

        foreach (KeyValuePair<BattleUnitModel, int> entry in spentLight)
        {
            if (entry.Value > GetRemainingLight(snapshot, entry.Key))
            {
                return false;
            }
        }

        return true;
    }

    private static bool AddUsedSpeedDie(Dictionary<BattleUnitModel, HashSet<int>> usedSpeedDice, BattleUnitModel actor, int speedDiceIndex)
    {
        HashSet<int> actorDice;
        if (!usedSpeedDice.TryGetValue(actor, out actorDice))
        {
            actorDice = new HashSet<int>();
            usedSpeedDice.Add(actor, actorDice);
        }

        return actorDice.Add(speedDiceIndex);
    }

    private static bool AddUsedCard(Dictionary<BattleUnitModel, HashSet<BattleDiceCardModel>> usedCards, BattleUnitModel actor, BattleDiceCardModel card)
    {
        HashSet<BattleDiceCardModel> actorCards;
        if (!usedCards.TryGetValue(actor, out actorCards))
        {
            actorCards = new HashSet<BattleDiceCardModel>();
            usedCards.Add(actor, actorCards);
        }

        return actorCards.Add(card);
    }

    private static void AddSpentLight(Dictionary<BattleUnitModel, int> spentLight, BattleUnitModel actor, int cost)
    {
        int current;
        spentLight.TryGetValue(actor, out current);
        spentLight[actor] = current + cost;
    }

    private static int GetRemainingLight(BattleSnapshot snapshot, BattleUnitModel actor)
    {
        if (snapshot == null || snapshot.PlayerResources == null || snapshot.PlayerResources.ActorResources == null)
        {
            return 0;
        }

        foreach (ActorAvailableResources actorResources in snapshot.PlayerResources.ActorResources)
        {
            if (actorResources != null && actorResources.Actor == actor)
            {
                return actorResources.RemainingLight;
            }
        }

        return 0;
    }

    private static float CalculateTerminalProgress(List<ActionCandidate> actions)
    {
        float score = 0f;
        foreach (ActionCandidate action in actions)
        {
            if (action == null || action.Target == null)
            {
                continue;
            }

            DamageEstimate damage = DamageEstimator.Estimate(action.Card, action.Actor, action.Target);
            score += damage.ExpectedHpDamage * 0.35f;
            if (damage.ExpectedHpDamage >= action.Target.hp)
            {
                score += 30f;
            }
            else if (damage.MaxHpDamage >= action.Target.hp)
            {
                score += 12f;
            }
        }

        return score;
    }

    private static float CalculateActionEconomyChange(List<ActionCandidate> actions, ThreatResponseMatrix matrix)
    {
        float score = 0f;
        foreach (ActionCandidate action in actions)
        {
            if (action == null || action.Target == null)
            {
                continue;
            }

            DamageEstimate damage = DamageEstimator.Estimate(action.Card, action.Actor, action.Target);
            if (action.Target.breakDetail != null && damage.ExpectedBreakDamage >= action.Target.breakDetail.breakGauge)
            {
                score += 22f;
            }
            else if (action.Target.breakDetail != null && damage.MaxBreakDamage >= action.Target.breakDetail.breakGauge)
            {
                score += 9f;
            }
        }

        foreach (ThreatResponseAssessment response in GetSelectedResponses(matrix, actions))
        {
            if (response.Mechanism == ResponseMechanism.ClashThreatDie)
            {
                score += response.Threat.Level == ThreatLevel.Critical ? 18f : 10f;
            }
            else if (IsStrongOwnerDisable(response.OwnerDamageOutcome))
            {
                score += response.ResponseScore * 0.35f;
            }
        }

        return score;
    }

    private static float CalculateRiskChange(ThreatResponseMatrix matrix, List<ActionCandidate> actions)
    {
        if (matrix == null || matrix.Threats == null)
        {
            return 0f;
        }

        float score = 0f;
        foreach (ThreatAssessment threat in matrix.Threats)
        {
            ThreatCoverage coverage = GetThreatCoverage(matrix, actions, threat);
            if (coverage == ThreatCoverage.Full)
            {
                score += threat.UnansweredDangerScore * 0.45f;
                continue;
            }

            if (coverage == ThreatCoverage.Partial)
            {
                score += threat.UnansweredDangerScore * 0.18f;
                continue;
            }

            if (threat.Level == ThreatLevel.Critical)
            {
                score -= threat.UnansweredDangerScore * 0.7f;
            }
            else if (threat.Level == ThreatLevel.Major)
            {
                score -= threat.UnansweredDangerScore * 0.35f;
            }
            else
            {
                score -= threat.UnansweredDangerScore * 0.08f;
            }
        }

        return score;
    }

    private static float CalculateCost(List<ActionCandidate> actions)
    {
        float cost = 0f;
        foreach (ActionCandidate action in actions)
        {
            if (action == null || action.Card == null)
            {
                continue;
            }

            cost += action.Card.GetCost() * 2f;
            cost += 1f;
        }

        return cost;
    }

    private static float CalculateWaste(List<ActionCandidate> actions, ThreatResponseMatrix matrix)
    {
        float waste = 0f;
        Dictionary<ThreatAssessment, int> fullCoverageCount = new Dictionary<ThreatAssessment, int>();

        foreach (ActionCandidate action in actions)
        {
            if (action == null || action.Target == null)
            {
                continue;
            }

            DamageEstimate damage = DamageEstimator.Estimate(action.Card, action.Actor, action.Target);
            float overkill = damage.ExpectedHpDamage - action.Target.hp;
            if (overkill > 0f)
            {
                waste += overkill * 0.4f;
            }
        }

        foreach (ThreatResponseAssessment response in GetSelectedResponses(matrix, actions))
        {
            if (GetResponseCoverage(response) != ThreatCoverage.Full)
            {
                continue;
            }

            int count;
            fullCoverageCount.TryGetValue(response.Threat, out count);
            fullCoverageCount[response.Threat] = count + 1;
        }

        foreach (KeyValuePair<ThreatAssessment, int> entry in fullCoverageCount)
        {
            if (entry.Value > 1)
            {
                waste += (entry.Value - 1) * 10f;
            }
        }

        return waste;
    }

    private static float CalculateResourceFlowChange(BattleSnapshot snapshot, List<ActionCandidate> actions)
    {
        if (snapshot == null || snapshot.PlayerResources == null || snapshot.PlayerResources.ActorResources == null)
        {
            return 0f;
        }

        float score = 0f;
        foreach (ActorAvailableResources actorResources in snapshot.PlayerResources.ActorResources)
        {
            if (actorResources == null || actorResources.Actor == null)
            {
                continue;
            }

            int spent = 0;
            foreach (ActionCandidate action in actions)
            {
                if (action != null && action.Actor == actorResources.Actor && action.Card != null)
                {
                    spent += action.Card.GetCost();
                }
            }

            int remaining = actorResources.RemainingLight - spent;
            if (remaining >= 2)
            {
                score += 2f;
            }
            else if (remaining < 0)
            {
                score -= 20f;
            }
            else
            {
                score -= 3f;
            }
        }

        return score;
    }

    private static IEnumerable<ThreatResponseAssessment> GetSelectedResponses(ThreatResponseMatrix matrix, List<ActionCandidate> actions)
    {
        if (matrix == null || matrix.Responses == null || actions == null)
        {
            yield break;
        }

        foreach (ThreatResponseAssessment response in matrix.Responses)
        {
            if (response != null && actions.Contains(response.Candidate))
            {
                yield return response;
            }
        }
    }

    private static ThreatCoverage GetThreatCoverage(ThreatResponseMatrix matrix, List<ActionCandidate> actions, ThreatAssessment threat)
    {
        ThreatCoverage best = ThreatCoverage.None;
        foreach (ThreatResponseAssessment response in GetSelectedResponses(matrix, actions))
        {
            if (response.Threat != threat)
            {
                continue;
            }

            ThreatCoverage coverage = GetResponseCoverage(response);
            if (coverage == ThreatCoverage.Full)
            {
                return ThreatCoverage.Full;
            }

            if (coverage == ThreatCoverage.Partial)
            {
                best = ThreatCoverage.Partial;
            }
        }

        ThreatCoverage combinedOwnerCoverage = GetCombinedOwnerCoverage(actions, threat);
        if (combinedOwnerCoverage == ThreatCoverage.Full)
        {
            return ThreatCoverage.Full;
        }

        if (combinedOwnerCoverage == ThreatCoverage.Partial)
        {
            best = ThreatCoverage.Partial;
        }

        return best;
    }

    private static ThreatCoverage GetCombinedOwnerCoverage(List<ActionCandidate> actions, ThreatAssessment threat)
    {
        if (actions == null || threat == null || threat.EnemyAction == null || threat.EnemyAction.Owner == null)
        {
            return ThreatCoverage.None;
        }

        BattleUnitModel owner = threat.EnemyAction.Owner;
        float minHpDamage = 0f;
        float expectedHpDamage = 0f;
        float maxHpDamage = 0f;
        float minBreakDamage = 0f;
        float expectedBreakDamage = 0f;
        float maxBreakDamage = 0f;

        foreach (ActionCandidate action in actions)
        {
            if (!CanContributeBeforeThreat(action, threat))
            {
                continue;
            }

            DamageEstimate damage = DamageEstimator.Estimate(action.Card, action.Actor, owner);
            minHpDamage += damage.MinHpDamage;
            expectedHpDamage += damage.ExpectedHpDamage;
            maxHpDamage += damage.MaxHpDamage;
            minBreakDamage += damage.MinBreakDamage;
            expectedBreakDamage += damage.ExpectedBreakDamage;
            maxBreakDamage += damage.MaxBreakDamage;
        }

        if (minHpDamage >= owner.hp || minBreakDamage >= GetBreakGauge(owner))
        {
            return ThreatCoverage.Full;
        }

        if (expectedHpDamage >= owner.hp || expectedBreakDamage >= GetBreakGauge(owner))
        {
            return ThreatCoverage.Full;
        }

        if (maxHpDamage >= owner.hp || maxBreakDamage >= GetBreakGauge(owner))
        {
            return ThreatCoverage.Partial;
        }

        return ThreatCoverage.None;
    }

    private static bool CanContributeBeforeThreat(ActionCandidate action, ThreatAssessment threat)
    {
        if (action == null || action.Actor == null || action.Target == null || action.Card == null)
        {
            return false;
        }

        if (threat == null || threat.EnemyAction == null || threat.EnemyAction.Owner == null)
        {
            return false;
        }

        if (action.Target != threat.EnemyAction.Owner)
        {
            return false;
        }

        return action.Actor.GetSpeed(action.ActorSpeedDiceIndex) > threat.EnemyAction.SpeedDiceValue;
    }

    private static int GetBreakGauge(BattleUnitModel unit)
    {
        if (unit == null || unit.breakDetail == null)
        {
            return int.MaxValue;
        }

        return unit.breakDetail.breakGauge;
    }

    private static ThreatCoverage GetResponseCoverage(ThreatResponseAssessment response)
    {
        if (response == null)
        {
            return ThreatCoverage.None;
        }

        if (response.Mechanism == ResponseMechanism.ClashThreatDie)
        {
            return ThreatCoverage.Full;
        }

        if (IsStrongOwnerDisable(response.OwnerDamageOutcome))
        {
            return ThreatCoverage.Full;
        }

        if (response.OwnerDamageOutcome == OwnerDamageOutcome.PotentialKill
            || response.OwnerDamageOutcome == OwnerDamageOutcome.PotentialStagger)
        {
            return ThreatCoverage.Partial;
        }

        return ThreatCoverage.None;
    }

    private static bool IsStrongOwnerDisable(OwnerDamageOutcome outcome)
    {
        return outcome == OwnerDamageOutcome.GuaranteedKill
            || outcome == OwnerDamageOutcome.ExpectedKill
            || outcome == OwnerDamageOutcome.GuaranteedStagger
            || outcome == OwnerDamageOutcome.ExpectedStagger;
    }

    private enum ThreatCoverage
    {
        None,
        Partial,
        Full
    }
}
