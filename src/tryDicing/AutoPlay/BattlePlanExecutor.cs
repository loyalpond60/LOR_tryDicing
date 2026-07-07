using System.Collections.Generic;
using LOR_DiceSystem;

public static class BattlePlanExecutor
{
    public static bool Execute(SpeedDiceAction action)
    {
        if (!IsStillLegal(action))
        {
            return false;
        }

        return ActionExecutor.Execute(action);
    }

    private static bool IsStillLegal(SpeedDiceAction action)
    {
        if (action == null || action.Actor == null || action.Card == null || action.Target == null)
        {
            return false;
        }

        if (!CanUseSpeedDie(action.Actor, action.ActorSpeedDiceIndex))
        {
            return false;
        }

        if (!CanUseCard(action.Actor, action.ActorSpeedDiceIndex, action.Card))
        {
            return false;
        }

        if (!CanTargetSlot(action.Actor, action.Target, action.TargetSpeedDiceIndex))
        {
            return false;
        }

        return true;
    }

    private static bool CanUseSpeedDie(BattleUnitModel actor, int speedDiceIndex)
    {
        if (actor == null || actor.allyCardDetail == null || actor.speedDiceResult == null)
        {
            return false;
        }

        if (actor.turnState == BattleUnitTurnState.BREAK || actor.IsBreakLifeZero() || actor.IsKnockout())
        {
            return false;
        }

        if (speedDiceIndex < 0 || speedDiceIndex >= actor.speedDiceResult.Count)
        {
            return false;
        }

        if (actor.speedDiceResult[speedDiceIndex].breaked)
        {
            return false;
        }

        if (actor.cardSlotDetail == null || actor.cardSlotDetail.cardAry == null || speedDiceIndex >= actor.cardSlotDetail.cardAry.Count)
        {
            return false;
        }

        return actor.cardSlotDetail.cardAry[speedDiceIndex] == null;
    }

    private static bool CanUseCard(BattleUnitModel actor, int speedDiceIndex, BattleDiceCardModel card)
    {
        if (actor == null || actor.allyCardDetail == null || actor.cardSlotDetail == null || card == null)
        {
            return false;
        }

        List<BattleDiceCardModel> hand = actor.allyCardDetail.GetHand();
        if (hand == null || !hand.Contains(card))
        {
            return false;
        }

        int remainingLight = actor.cardSlotDetail.PlayPoint - actor.cardSlotDetail.ReservedPlayPoint;
        if (card.GetCost() > remainingLight)
        {
            return false;
        }

        if (!actor.CheckCardAvailableForPlayer(card))
        {
            return false;
        }

        CardRange range = card.GetSpec().Ranged;
        if (range == CardRange.Instance || range == CardRange.FarArea || range == CardRange.FarAreaEach)
        {
            return false;
        }

        return card.GetPriority(actor.GetSpeed(speedDiceIndex)) >= 0;
    }

    private static bool CanTargetSlot(BattleUnitModel actor, BattleUnitModel target, int targetSlot)
    {
        if (actor == null || target == null || target == actor || !target.IsTargetable(actor))
        {
            return false;
        }

        if (target.speedDiceResult == null || target.cardSlotDetail == null || target.cardSlotDetail.cardAry == null)
        {
            return false;
        }

        if (targetSlot < 0 || targetSlot >= target.speedDiceResult.Count || targetSlot >= target.cardSlotDetail.cardAry.Count)
        {
            return false;
        }

        if (!target.IsTargetable_theLast() && targetSlot > 0)
        {
            return false;
        }

        return !target.speedDiceResult[targetSlot].breaked;
    }
}
