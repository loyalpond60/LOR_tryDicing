public sealed class SpeedDiceAction
{
    public SpeedDiceAction(
        BattleUnitModel actor,
        int actorSpeedDiceIndex,
        BattleDiceCardModel card,
        BattleUnitModel target,
        int targetSpeedDiceIndex,
        string intent)
    {
        Actor = actor;
        ActorSpeedDiceIndex = actorSpeedDiceIndex;
        Card = card;
        Target = target;
        TargetSpeedDiceIndex = targetSpeedDiceIndex;
        Intent = intent;
    }

    public readonly BattleUnitModel Actor;
    public readonly int ActorSpeedDiceIndex;
    public readonly BattleDiceCardModel Card;
    public readonly BattleUnitModel Target;
    public readonly int TargetSpeedDiceIndex;
    public readonly string Intent;

    public bool IsExecuted { get; private set; }
    public bool IsFailed { get; private set; }

    public void MarkExecuted()
    {
        IsExecuted = true;
    }

    public void MarkFailed()
    {
        IsFailed = true;
    }
}
