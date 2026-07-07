using System.Collections.Generic;

public sealed class BattleSnapshot
{
    public BattleSnapshot(
        Faction actorFaction,
        List<BattleUnitModel> actors,
        List<BattleUnitModel> targets,
        List<DeclaredAction> declaredActions,
        PlayerAvailableResources playerResources,
        string planKey)
    {
        ActorFaction = actorFaction;
        Actors = actors;
        Targets = targets;
        DeclaredActions = declaredActions;
        PlayerResources = playerResources;
        PlanKey = planKey;
    }

    public readonly Faction ActorFaction;
    public readonly List<BattleUnitModel> Actors;
    public readonly List<BattleUnitModel> Targets;
    public readonly List<DeclaredAction> DeclaredActions;
    public readonly PlayerAvailableResources PlayerResources;
    public readonly string PlanKey;
}
