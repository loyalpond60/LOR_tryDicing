public sealed class ActionCandidate
{
    public ActionCandidate(
        BattleUnitModel actor,
        int actorSpeedDiceIndex,
        BattleDiceCardModel card,
        BattleUnitModel target,
        int targetSpeedDiceIndex,
        InteractionType interactionType)
    {
        Actor = actor;
        ActorSpeedDiceIndex = actorSpeedDiceIndex;
        Card = card;
        Target = target;
        TargetSpeedDiceIndex = targetSpeedDiceIndex;
        InteractionType = interactionType;
    }

    public readonly BattleUnitModel Actor;
    public readonly int ActorSpeedDiceIndex;
    public readonly BattleDiceCardModel Card;
    public readonly BattleUnitModel Target;
    public readonly int TargetSpeedDiceIndex;
    public readonly InteractionType InteractionType;
}
