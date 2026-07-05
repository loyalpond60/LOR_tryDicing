public static class ActionExecutor
{
    public static bool Execute(SpeedDiceAction action)
    {
        BattleUnitModel self = action.Actor;
        int speedDiceIndex = action.ActorSpeedDiceIndex;
        int targetSlot = action.TargetSpeedDiceIndex;

        self.cardOrder = speedDiceIndex;
        targetSlot = self.ChangeTargetSlot(action.Card, action.Target, speedDiceIndex, targetSlot, self.TeamKill());
        self.cardSlotDetail.AddCard(action.Card, action.Target, targetSlot, false);
        return true;
    }
}
