public static class AutoPlayController
{
    public static bool TryPlay(BattleAllyCardDetail cardDetail, int speedDiceIndex)
    {
        BattleUnitModel actor = BattleSnapshotReader.GetOwner(cardDetail);
        if (actor == null)
        {
            TryDicingLogger.Info("AutoPlay failed: actor is null.");
            return false;
        }

        BattleSnapshot snapshot = BattleSnapshotReader.Read(actor.faction);
        BattlePlan plan = AutoPlayCache.GetOrCreate(snapshot);
        SpeedDiceAction action = plan.Find(actor, speedDiceIndex);
        if (action == null)
        {
            TryDicingLogger.Info("AutoPlay fallback: no planned action. actorId=" + actor.id + ", idx=" + speedDiceIndex);
            return false;
        }

        if (!BattlePlanExecutor.Execute(action))
        {
            TryDicingLogger.Info("AutoPlay fallback: planned action failed. actorId=" + actor.id + ", idx=" + speedDiceIndex);
            action.MarkFailed();
            return false;
        }

        action.MarkExecuted();
        AutoPlayLog.LogSelected(action);
        return true;
    }
}
