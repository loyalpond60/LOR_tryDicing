using System.Collections.Generic;
using System.Reflection;
using System.Text;

public static class BattleSnapshotReader
{
    private static readonly FieldInfo SelfField = typeof(BattleAllyCardDetail).GetField(
        "_self",
        BindingFlags.Instance | BindingFlags.NonPublic);

    public static BattleUnitModel GetOwner(BattleAllyCardDetail cardDetail)
    {
        if (cardDetail == null)
        {
            return null;
        }

        return SelfField.GetValue(cardDetail) as BattleUnitModel;
    }

    public static BattleSnapshot Read(Faction actorFaction)
    {
        Faction targetFaction = actorFaction == Faction.Enemy ? Faction.Player : Faction.Enemy;
        List<BattleUnitModel> actors = BattleObjectManager.instance.GetAliveList(actorFaction);
        List<BattleUnitModel> targets = BattleObjectManager.instance.GetAliveList(targetFaction);
        List<DeclaredAction> declaredActions = ReadDeclaredActions(actors, targets);
        return new BattleSnapshot(actorFaction, actors, targets, declaredActions, BuildPlanKey(actorFaction, actors, targets, declaredActions));
    }

    private static List<DeclaredAction> ReadDeclaredActions(List<BattleUnitModel> actors, List<BattleUnitModel> targets)
    {
        List<DeclaredAction> actions = new List<DeclaredAction>();
        AppendDeclaredActions(actions, actors);
        AppendDeclaredActions(actions, targets);
        return actions;
    }

    private static void AppendDeclaredActions(List<DeclaredAction> actions, List<BattleUnitModel> units)
    {
        if (units == null)
        {
            return;
        }

        foreach (BattleUnitModel unit in units)
        {
            if (unit == null || unit.cardSlotDetail == null || unit.cardSlotDetail.cardAry == null)
            {
                continue;
            }

            for (int slotIndex = 0; slotIndex < unit.cardSlotDetail.cardAry.Count; slotIndex++)
            {
                BattlePlayingCardDataInUnitModel cardData = unit.cardSlotDetail.cardAry[slotIndex];
                if (cardData == null || cardData.card == null)
                {
                    continue;
                }

                BattleUnitModel target = cardData.target;
                bool isTargetingOpponent = target != null && target.faction != unit.faction;
                int speedDiceValue = GetSpeedDiceValue(unit, slotIndex, cardData);
                actions.Add(new DeclaredAction(
                    unit,
                    unit.faction,
                    slotIndex,
                    cardData.card,
                    target,
                    cardData.targetSlotOrder,
                    speedDiceValue,
                    isTargetingOpponent));
            }
        }
    }

    private static int GetSpeedDiceValue(BattleUnitModel unit, int slotIndex, BattlePlayingCardDataInUnitModel cardData)
    {
        if (cardData != null)
        {
            return cardData.speedDiceResultValue;
        }

        if (unit != null && unit.speedDiceResult != null && slotIndex >= 0 && slotIndex < unit.speedDiceResult.Count)
        {
            return unit.speedDiceResult[slotIndex].value;
        }

        return 0;
    }

    private static string BuildPlanKey(
        Faction actorFaction,
        List<BattleUnitModel> actors,
        List<BattleUnitModel> targets,
        List<DeclaredAction> declaredActions)
    {
        StringBuilder builder = new StringBuilder();
        AppendUnits(builder, "A", actors);
        AppendUnits(builder, "T", targets);
        AppendOpponentDeclaredActions(builder, actorFaction, declaredActions);
        return builder.ToString();
    }

    private static void AppendUnits(StringBuilder builder, string prefix, List<BattleUnitModel> units)
    {
        builder.Append(prefix);
        for (int i = 0; i < units.Count; i++)
        {
            BattleUnitModel unit = units[i];
            builder.Append('|').Append(unit.id).Append(':');
            if (unit.speedDiceResult == null)
            {
                builder.Append("no-dice");
                continue;
            }

            for (int diceIndex = 0; diceIndex < unit.speedDiceResult.Count; diceIndex++)
            {
                if (diceIndex > 0)
                {
                    builder.Append(',');
                }

                builder.Append(unit.speedDiceResult[diceIndex].value);
                builder.Append(unit.speedDiceResult[diceIndex].breaked ? 'b' : 'n');
            }
        }
    }

    private static void AppendOpponentDeclaredActions(StringBuilder builder, Faction actorFaction, List<DeclaredAction> declaredActions)
    {
        builder.Append("|D");
        if (declaredActions == null)
        {
            return;
        }

        foreach (DeclaredAction action in declaredActions)
        {
            if (action == null || action.OwnerFaction == actorFaction)
            {
                continue;
            }

            builder.Append('|');
            builder.Append(action.Owner == null ? 0 : action.Owner.id);
            builder.Append(':').Append(action.SpeedDiceIndex);
            builder.Append(':').Append(action.SpeedDiceValue);
            builder.Append(':').Append(action.Card == null ? "no-card" : action.Card.GetName());
            builder.Append(':').Append(action.Target == null ? 0 : action.Target.id);
            builder.Append(':').Append(action.TargetSpeedDiceIndex);
        }
    }
}
