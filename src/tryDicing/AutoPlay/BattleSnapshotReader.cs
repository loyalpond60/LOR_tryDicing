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
        return new BattleSnapshot(actorFaction, actors, targets, BuildPlanKey(actors, targets));
    }

    private static string BuildPlanKey(List<BattleUnitModel> actors, List<BattleUnitModel> targets)
    {
        StringBuilder builder = new StringBuilder();
        AppendUnits(builder, "A", actors);
        AppendUnits(builder, "T", targets);
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
}
