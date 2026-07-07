using System.Collections.Generic;

public static class PlanSearch
{
    private const int DefaultBeamWidth = 32;
    private const int DefaultMaxCandidatePool = 80;

    public static PlanSearchResult Search(BattleSnapshot snapshot)
    {
        return Search(snapshot, DefaultBeamWidth, DefaultMaxCandidatePool);
    }

    public static PlanSearchResult Search(BattleSnapshot snapshot, int beamWidth, int maxCandidatePool)
    {
        ThreatResponseMatrix matrix = ThreatResponseMatrix.Build(snapshot);
        List<ActionCandidate> candidatePool = BuildThreatGuidedCandidatePool(matrix, maxCandidatePool);
        PlanEvaluation emptyEvaluation = PlanEvaluator.Evaluate(snapshot, new List<ActionCandidate>(), matrix);
        List<SearchNode> beam = new List<SearchNode>();
        int evaluatedPlanCount = 1;

        beam.Add(new SearchNode(new List<ActionCandidate>(), emptyEvaluation));

        foreach (ActionCandidate candidate in candidatePool)
        {
            List<SearchNode> nextBeam = new List<SearchNode>(beam);
            foreach (SearchNode node in beam)
            {
                if (!CanAddCandidate(snapshot, node.SelectedActions, candidate))
                {
                    continue;
                }

                List<ActionCandidate> selectedActions = new List<ActionCandidate>(node.SelectedActions);
                selectedActions.Add(candidate);
                PlanEvaluation evaluation = PlanEvaluator.Evaluate(snapshot, selectedActions, matrix);
                evaluatedPlanCount++;

                if (evaluation.IsLegal)
                {
                    nextBeam.Add(new SearchNode(selectedActions, evaluation));
                }
            }

            beam = KeepBest(nextBeam, beamWidth);
        }

        SearchNode best = beam.Count == 0 ? new SearchNode(new List<ActionCandidate>(), emptyEvaluation) : beam[0];
        string reason = string.Format(
            "threat-guided beam search; pool={0}, evaluated={1}, selected={2}, score={3:0.0}",
            candidatePool.Count,
            evaluatedPlanCount,
            best.SelectedActions.Count,
            best.Evaluation == null ? 0f : best.Evaluation.TotalScore);

        return new PlanSearchResult(
            best.SelectedActions,
            best.Evaluation,
            matrix,
            candidatePool.Count,
            evaluatedPlanCount,
            reason);
    }

    private static List<ActionCandidate> BuildThreatGuidedCandidatePool(ThreatResponseMatrix matrix, int maxCandidatePool)
    {
        Dictionary<ActionCandidate, float> priorities = new Dictionary<ActionCandidate, float>();
        if (matrix == null)
        {
            return new List<ActionCandidate>();
        }

        if (matrix.Responses != null)
        {
            foreach (ThreatResponseAssessment response in matrix.Responses)
            {
                if (response == null || response.Candidate == null)
                {
                    continue;
                }

                AddPriority(priorities, response.Candidate, CalculateResponsePriority(response));
            }
        }

        if (matrix.Candidates != null)
        {
            foreach (ActionCandidate candidate in matrix.Candidates)
            {
                AddPriority(priorities, candidate, CalculateProactivePriority(candidate));
            }
        }

        List<CandidatePriority> ranked = new List<CandidatePriority>();
        foreach (KeyValuePair<ActionCandidate, float> entry in priorities)
        {
            if (entry.Key != null && entry.Value > 0f)
            {
                ranked.Add(new CandidatePriority(entry.Key, entry.Value));
            }
        }

        ranked.Sort(CompareCandidatePriority);

        List<ActionCandidate> pool = new List<ActionCandidate>();
        int limit = maxCandidatePool <= 0 ? ranked.Count : maxCandidatePool;
        for (int i = 0; i < ranked.Count && i < limit; i++)
        {
            pool.Add(ranked[i].Candidate);
        }

        return pool;
    }

    private static float CalculateResponsePriority(ThreatResponseAssessment response)
    {
        if (response == null || response.Mechanism == ResponseMechanism.None)
        {
            return 0f;
        }

        float priority = response.ResponseScore;
        if (response.Threat != null)
        {
            priority += response.Threat.Level == ThreatLevel.Critical ? 30f : 0f;
            priority += response.Threat.Level == ThreatLevel.Major ? 14f : 0f;
        }

        if (response.Mechanism == ResponseMechanism.ClashThreatDie)
        {
            return priority + 20f;
        }

        switch (response.OwnerDamageOutcome)
        {
            case OwnerDamageOutcome.GuaranteedKill:
                return priority + 35f;
            case OwnerDamageOutcome.ExpectedKill:
                return priority + 28f;
            case OwnerDamageOutcome.GuaranteedStagger:
                return priority + 26f;
            case OwnerDamageOutcome.ExpectedStagger:
                return priority + 20f;
            case OwnerDamageOutcome.PotentialKill:
                return priority + 12f;
            case OwnerDamageOutcome.PotentialStagger:
                return priority + 8f;
            default:
                return 0f;
        }
    }

    private static float CalculateProactivePriority(ActionCandidate candidate)
    {
        if (candidate == null || candidate.Card == null || candidate.Target == null)
        {
            return 0f;
        }

        DamageEstimate damage = DamageEstimator.Estimate(candidate.Card, candidate.Actor, candidate.Target);
        float priority = damage.ExpectedHpDamage * 0.2f + damage.ExpectedBreakDamage * 0.2f;

        if (damage.ExpectedHpDamage >= candidate.Target.hp)
        {
            priority += 18f;
        }
        else if (damage.MaxHpDamage >= candidate.Target.hp)
        {
            priority += 7f;
        }

        if (candidate.Target.breakDetail != null && damage.ExpectedBreakDamage >= candidate.Target.breakDetail.breakGauge)
        {
            priority += 14f;
        }
        else if (candidate.Target.breakDetail != null && damage.MaxBreakDamage >= candidate.Target.breakDetail.breakGauge)
        {
            priority += 5f;
        }

        priority += candidate.Card.GetCost() <= 1 ? 2f : 0f;
        return priority;
    }

    private static void AddPriority(Dictionary<ActionCandidate, float> priorities, ActionCandidate candidate, float priority)
    {
        if (candidate == null || priority <= 0f)
        {
            return;
        }

        float current;
        priorities.TryGetValue(candidate, out current);
        priorities[candidate] = current + priority;
    }

    private static bool CanAddCandidate(BattleSnapshot snapshot, List<ActionCandidate> selectedActions, ActionCandidate candidate)
    {
        if (candidate == null || candidate.Actor == null || candidate.Card == null)
        {
            return false;
        }

        int spentLight = candidate.Card.GetCost();
        foreach (ActionCandidate selectedAction in selectedActions)
        {
            if (selectedAction == null || selectedAction.Actor != candidate.Actor)
            {
                continue;
            }

            if (selectedAction.ActorSpeedDiceIndex == candidate.ActorSpeedDiceIndex)
            {
                return false;
            }

            if (selectedAction.Card == candidate.Card)
            {
                return false;
            }

            if (selectedAction.Card != null)
            {
                spentLight += selectedAction.Card.GetCost();
            }
        }

        return spentLight <= GetRemainingLight(snapshot, candidate.Actor);
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

    private static List<SearchNode> KeepBest(List<SearchNode> nodes, int beamWidth)
    {
        nodes.Sort(CompareSearchNode);

        int limit = beamWidth <= 0 ? nodes.Count : beamWidth;
        List<SearchNode> best = new List<SearchNode>();
        for (int i = 0; i < nodes.Count && i < limit; i++)
        {
            best.Add(nodes[i]);
        }

        return best;
    }

    private static int CompareCandidatePriority(CandidatePriority left, CandidatePriority right)
    {
        return right.Priority.CompareTo(left.Priority);
    }

    private static int CompareSearchNode(SearchNode left, SearchNode right)
    {
        float leftScore = left == null || left.Evaluation == null ? float.MinValue : left.Evaluation.TotalScore;
        float rightScore = right == null || right.Evaluation == null ? float.MinValue : right.Evaluation.TotalScore;
        return rightScore.CompareTo(leftScore);
    }

    private sealed class CandidatePriority
    {
        public CandidatePriority(ActionCandidate candidate, float priority)
        {
            Candidate = candidate;
            Priority = priority;
        }

        public readonly ActionCandidate Candidate;
        public readonly float Priority;
    }

    private sealed class SearchNode
    {
        public SearchNode(List<ActionCandidate> selectedActions, PlanEvaluation evaluation)
        {
            SelectedActions = selectedActions;
            Evaluation = evaluation;
        }

        public readonly List<ActionCandidate> SelectedActions;
        public readonly PlanEvaluation Evaluation;
    }
}
