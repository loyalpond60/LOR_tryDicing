public sealed class DeclaredAction
{
    public DeclaredAction(
        BattleUnitModel owner,
        Faction ownerFaction,
        int speedDiceIndex,
        BattleDiceCardModel card,
        BattleUnitModel target,
        int targetSpeedDiceIndex,
        int speedDiceValue,
        bool isTargetingOpponent)
    {
        Owner = owner;
        OwnerFaction = ownerFaction;
        SpeedDiceIndex = speedDiceIndex;
        Card = card;
        Target = target;
        TargetSpeedDiceIndex = targetSpeedDiceIndex;
        SpeedDiceValue = speedDiceValue;
        IsTargetingOpponent = isTargetingOpponent;
    }

    public readonly BattleUnitModel Owner;
    public readonly Faction OwnerFaction;
    public readonly int SpeedDiceIndex;
    public readonly BattleDiceCardModel Card;
    public readonly BattleUnitModel Target;
    public readonly int TargetSpeedDiceIndex;
    public readonly int SpeedDiceValue;
    public readonly bool IsTargetingOpponent;
}
