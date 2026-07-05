public static class AutoPlayLog
{
    public static void LogSelected(SpeedDiceAction action)
    {
        TryDicingLogger.Info(string.Format(
            "Plan action executed actorId={0}, idx={1}, card={2}, cost={3}, targetId={4}, targetHp={5:0}/{6}, targetSlot={7}, intent={8}",
            action.Actor.id,
            action.ActorSpeedDiceIndex,
            action.Card.GetName(),
            action.Card.GetCost(),
            action.Target.id,
            action.Target.hp,
            action.Target.MaxHp,
            action.TargetSpeedDiceIndex,
            action.Intent));
    }
}
