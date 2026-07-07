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
        if (plan == null)
        {
            TryDicingLogger.Error("AutoPlay skip: V2 plan is null. actorId=" + actor.id + ", idx=" + speedDiceIndex);
            return true;
        }

        SpeedDiceAction action = plan.Find(actor, speedDiceIndex);
        if (action == null)
        {
            TryDicingLogger.Info("AutoPlay skip: V2 plan has no action. actorId=" + actor.id + ", idx=" + speedDiceIndex);
            return true;
        }

        if (!BattlePlanExecutor.Execute(action))
        {
            TryDicingLogger.Error("AutoPlay blocked invalid V2 planned action. actorId=" + actor.id + ", idx=" + speedDiceIndex);
            action.MarkFailed();
            return true;
        }

        action.MarkExecuted();
        AutoPlayLog.LogSelected(action);
        return true;
    }
}
