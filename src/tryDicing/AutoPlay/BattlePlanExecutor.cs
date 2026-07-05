public static class BattlePlanExecutor
{
    public static bool Execute(SpeedDiceAction action)
    {
        if (action == null || action.Actor == null || action.Card == null || action.Target == null)
        {
            return false;
        }

        if (!action.Actor.CheckCardAvailableForPlayer(action.Card))
        {
            return false;
        }

        return ActionExecutor.Execute(action);
    }
}
