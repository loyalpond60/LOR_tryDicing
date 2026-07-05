using System;

public sealed class LegalActionSearchReport
{
    public LegalActionSearchReport(
        BattleUnitModel actor,
        int actorSpeedDiceIndex,
        int handCount,
        int remainingLight,
        int targetCount)
    {
        ActorId = actor == null ? -1 : actor.id;
        ActorSpeedDiceIndex = actorSpeedDiceIndex;
        SpeedValue = ReadSpeedValue(actor, actorSpeedDiceIndex);
        HandCount = handCount;
        RemainingLight = remainingLight;
        TargetCount = targetCount;
    }

    public readonly int ActorId;
    public readonly int ActorSpeedDiceIndex;
    public readonly int SpeedValue;
    public readonly int HandCount;
    public readonly int RemainingLight;
    public readonly int TargetCount;

    public bool SpeedDieUsable;
    public int CardsChecked;
    public int CardsUsable;
    public int BlockedByNullCard;
    public int BlockedByLight;
    public int BlockedByAvailability;
    public int BlockedByRange;
    public int BlockedByPriority;
    public int TargetsChecked;
    public int TargetSlotsChecked;
    public int BlockedByTarget;
    public int BlockedByTargetSlot;
    public int CandidateCount;
    public int ClashCount;
    public int OneSidedAttackCount;

    public string ToLogString()
    {
        return string.Format(
            "LocalAction search actorId={0}, idx={1}, speed={2}, hand={3}, light={4}, targets={5}, usableSpeedDie={6}, cardsChecked={7}, cardsUsable={8}, blockedByLight={9}, blockedByAvailability={10}, blockedByRange={11}, blockedByPriority={12}, targetsChecked={13}, targetSlotsChecked={14}, blockedByTarget={15}, blockedByTargetSlot={16}, candidates={17}, clash={18}, oneSided={19}",
            ActorId,
            ActorSpeedDiceIndex,
            SpeedValue,
            HandCount,
            RemainingLight,
            TargetCount,
            SpeedDieUsable,
            CardsChecked,
            CardsUsable,
            BlockedByLight,
            BlockedByAvailability,
            BlockedByRange,
            BlockedByPriority,
            TargetsChecked,
            TargetSlotsChecked,
            BlockedByTarget,
            BlockedByTargetSlot,
            CandidateCount,
            ClashCount,
            OneSidedAttackCount);
    }

    private static int ReadSpeedValue(BattleUnitModel actor, int speedDiceIndex)
    {
        try
        {
            if (actor == null || actor.speedDiceResult == null || speedDiceIndex < 0 || speedDiceIndex >= actor.speedDiceResult.Count)
            {
                return -1;
            }

            return actor.speedDiceResult[speedDiceIndex].value;
        }
        catch (Exception)
        {
            return -1;
        }
    }
}
