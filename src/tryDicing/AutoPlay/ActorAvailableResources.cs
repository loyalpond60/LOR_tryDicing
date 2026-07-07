using System.Collections.Generic;

public sealed class ActorAvailableResources
{
    public ActorAvailableResources(
        BattleUnitModel actor,
        int playPoint,
        int reservedPlayPoint,
        int remainingLight,
        List<BattleDiceCardModel> hand,
        List<int> usableSpeedDiceIndices)
    {
        Actor = actor;
        PlayPoint = playPoint;
        ReservedPlayPoint = reservedPlayPoint;
        RemainingLight = remainingLight;
        Hand = hand;
        UsableSpeedDiceIndices = usableSpeedDiceIndices;
    }

    public readonly BattleUnitModel Actor;
    public readonly int PlayPoint;
    public readonly int ReservedPlayPoint;
    public readonly int RemainingLight;
    public readonly List<BattleDiceCardModel> Hand;
    public readonly List<int> UsableSpeedDiceIndices;
}
