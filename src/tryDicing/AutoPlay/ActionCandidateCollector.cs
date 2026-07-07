using System.Collections.Generic;

public static class ActionCandidateCollector
{
    public static List<ActionCandidate> Collect(BattleSnapshot snapshot)
    {
        List<ActionCandidate> candidates = new List<ActionCandidate>();
        if (snapshot == null || snapshot.PlayerResources == null || snapshot.PlayerResources.ActorResources == null)
        {
            return candidates;
        }

        foreach (ActorAvailableResources actorResources in snapshot.PlayerResources.ActorResources)
        {
            AddActorCandidates(candidates, actorResources, snapshot.Targets);
        }

        return candidates;
    }

    private static void AddActorCandidates(
        List<ActionCandidate> candidates,
        ActorAvailableResources actorResources,
        List<BattleUnitModel> targets)
    {
        if (actorResources == null || actorResources.UsableSpeedDiceIndices == null)
        {
            return;
        }

        foreach (int speedDiceIndex in actorResources.UsableSpeedDiceIndices)
        {
            LegalActionSearchResult result = LegalActionFinder.Find(
                actorResources.Actor,
                speedDiceIndex,
                actorResources.Hand,
                actorResources.RemainingLight,
                targets);

            if (result != null && result.Candidates != null)
            {
                candidates.AddRange(result.Candidates);
            }
        }
    }
}
