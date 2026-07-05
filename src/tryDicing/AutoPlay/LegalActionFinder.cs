using System.Collections.Generic;
using LOR_DiceSystem;

public static class LegalActionFinder
{
    public static LegalActionSearchResult Find(
        BattleUnitModel actor,
        int speedDiceIndex,
        List<BattleDiceCardModel> availableCards,
        int remainingLight,
        List<BattleUnitModel> targets)
    {
        List<ActionCandidate> candidates = new List<ActionCandidate>();
        LegalActionSearchReport report = new LegalActionSearchReport(
            actor,
            speedDiceIndex,
            availableCards == null ? 0 : availableCards.Count,
            remainingLight,
            targets == null ? 0 : targets.Count);

        if (!CanUseSpeedDie(actor, speedDiceIndex))
        {
            return new LegalActionSearchResult(candidates, report);
        }

        report.SpeedDieUsable = true;

        if (availableCards == null || targets == null)
        {
            return new LegalActionSearchResult(candidates, report);
        }

        foreach (BattleDiceCardModel card in availableCards)
        {
            report.CardsChecked++;
            if (!CanUseCard(actor, speedDiceIndex, card, remainingLight, report))
            {
                continue;
            }

            report.CardsUsable++;
            AddTargetCandidates(candidates, report, actor, speedDiceIndex, card, targets);
        }

        report.CandidateCount = candidates.Count;
        return new LegalActionSearchResult(candidates, report);
    }

    private static void AddTargetCandidates(
        List<ActionCandidate> candidates,
        LegalActionSearchReport report,
        BattleUnitModel actor,
        int speedDiceIndex,
        BattleDiceCardModel card,
        List<BattleUnitModel> targets)
    {
        foreach (BattleUnitModel target in targets)
        {
            report.TargetsChecked++;
            if (target == null || target == actor || !target.IsTargetable(actor))
            {
                report.BlockedByTarget++;
                continue;
            }

            int maxSlot = GetTargetSlotCount(target);
            if (maxSlot <= 0)
            {
                report.BlockedByTargetSlot++;
                continue;
            }

            for (int targetSlot = 0; targetSlot < maxSlot; targetSlot++)
            {
                report.TargetSlotsChecked++;
                if (!CanTargetSlot(target, targetSlot))
                {
                    report.BlockedByTargetSlot++;
                    continue;
                }

                InteractionType interactionType = HasAssignedCard(target, targetSlot)
                    ? InteractionType.Clash
                    : InteractionType.OneSidedAttack;

                if (interactionType == InteractionType.Clash)
                {
                    report.ClashCount++;
                }
                else if (interactionType == InteractionType.OneSidedAttack)
                {
                    report.OneSidedAttackCount++;
                }

                candidates.Add(new ActionCandidate(actor, speedDiceIndex, card, target, targetSlot, interactionType));
            }
        }
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

    private static bool CanUseCard(
        BattleUnitModel actor,
        int speedDiceIndex,
        BattleDiceCardModel card,
        int remainingLight,
        LegalActionSearchReport report)
    {
        if (card == null)
        {
            report.BlockedByNullCard++;
            return false;
        }

        if (card.GetCost() > remainingLight)
        {
            report.BlockedByLight++;
            return false;
        }

        if (!actor.CheckCardAvailableForPlayer(card))
        {
            report.BlockedByAvailability++;
            return false;
        }

        CardRange range = card.GetSpec().Ranged;
        if (range == CardRange.Instance || range == CardRange.FarArea || range == CardRange.FarAreaEach)
        {
            report.BlockedByRange++;
            return false;
        }

        if (card.GetPriority(actor.GetSpeed(speedDiceIndex)) < 0)
        {
            report.BlockedByPriority++;
            return false;
        }

        return true;
    }

    private static int GetTargetSlotCount(BattleUnitModel target)
    {
        if (target.speedDiceResult == null || target.cardSlotDetail == null || target.cardSlotDetail.cardAry == null)
        {
            return 0;
        }

        int maxSlot = target.speedDiceResult.Count;
        if (!target.IsTargetable_theLast() && maxSlot > 1)
        {
            maxSlot = 1;
        }

        if (maxSlot > target.cardSlotDetail.cardAry.Count)
        {
            maxSlot = target.cardSlotDetail.cardAry.Count;
        }

        return maxSlot;
    }

    private static bool CanTargetSlot(BattleUnitModel target, int targetSlot)
    {
        if (target.speedDiceResult == null || targetSlot < 0 || targetSlot >= target.speedDiceResult.Count)
        {
            return false;
        }

        return !target.speedDiceResult[targetSlot].breaked;
    }

    private static bool HasAssignedCard(BattleUnitModel target, int targetSlot)
    {
        if (target.cardSlotDetail == null || target.cardSlotDetail.cardAry == null)
        {
            return false;
        }

        if (targetSlot < 0 || targetSlot >= target.cardSlotDetail.cardAry.Count)
        {
            return false;
        }

        return target.cardSlotDetail.cardAry[targetSlot] != null;
    }
}
