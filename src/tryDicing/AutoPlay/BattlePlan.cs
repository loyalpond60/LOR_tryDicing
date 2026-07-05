using System.Collections.Generic;

public sealed class BattlePlan
{
    public BattlePlan(string planKey, List<SpeedDiceAction> actions)
    {
        PlanKey = planKey;
        Actions = actions;
    }

    public readonly string PlanKey;
    public readonly List<SpeedDiceAction> Actions;

    public SpeedDiceAction Find(BattleUnitModel actor, int speedDiceIndex)
    {
        foreach (SpeedDiceAction action in Actions)
        {
            if (!action.IsExecuted && !action.IsFailed && action.Actor == actor && action.ActorSpeedDiceIndex == speedDiceIndex)
            {
                return action;
            }
        }

        return null;
    }
}
