using System.Collections.Generic;

public sealed class PlayerAvailableResources
{
    public PlayerAvailableResources(List<ActorAvailableResources> actorResources)
    {
        ActorResources = actorResources;
        TotalUsableSpeedDiceCount = CountUsableSpeedDice(actorResources);
        TotalHandCount = CountHand(actorResources);
    }

    public readonly List<ActorAvailableResources> ActorResources;
    public readonly int TotalUsableSpeedDiceCount;
    public readonly int TotalHandCount;

    private static int CountUsableSpeedDice(List<ActorAvailableResources> actorResources)
    {
        if (actorResources == null)
        {
            return 0;
        }

        int count = 0;
        foreach (ActorAvailableResources actorResource in actorResources)
        {
            if (actorResource != null && actorResource.UsableSpeedDiceIndices != null)
            {
                count += actorResource.UsableSpeedDiceIndices.Count;
            }
        }

        return count;
    }

    private static int CountHand(List<ActorAvailableResources> actorResources)
    {
        if (actorResources == null)
        {
            return 0;
        }

        int count = 0;
        foreach (ActorAvailableResources actorResource in actorResources)
        {
            if (actorResource != null && actorResource.Hand != null)
            {
                count += actorResource.Hand.Count;
            }
        }

        return count;
    }
}
