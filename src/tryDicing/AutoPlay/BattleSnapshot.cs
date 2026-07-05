using System.Collections.Generic;

public sealed class BattleSnapshot
{
    public BattleSnapshot(Faction actorFaction, List<BattleUnitModel> actors, List<BattleUnitModel> targets, string planKey)
    {
        ActorFaction = actorFaction;
        Actors = actors;
        Targets = targets;
        PlanKey = planKey;
    }

    public readonly Faction ActorFaction;
    public readonly List<BattleUnitModel> Actors;
    public readonly List<BattleUnitModel> Targets;
    public readonly string PlanKey;
}
