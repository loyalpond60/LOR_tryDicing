using System.Collections.Generic;

public static class AutoBattleStateReader
{
    public static AutoBattleState Read(StageController stage)
    {
        if (stage == null || stage.State != StageController.StageState.Battle)
        {
            return AutoBattleState.NotInBattle;
        }

        if (stage.battleState == StageController.BattleState.None ||
            stage.Phase == StageController.StagePhase.EndBattle ||
            stage.Phase == StageController.StagePhase.EndBattle2)
        {
            return AutoBattleState.BattleEnded;
        }

        if (stage.Phase == StageController.StagePhase.RoundStartPhase_UI ||
            stage.Phase == StageController.StagePhase.RoundStartPhase_System ||
            stage.Phase == StageController.StagePhase.DrawCardPhase ||
            stage.Phase == StageController.StagePhase.SortUnitPhase)
        {
            return AutoBattleState.WaitingRoll;
        }

        if (stage.Phase == StageController.StagePhase.ApplyLibrarianCardPhase)
        {
            return HasUnassignedUsablePlayerDice() ? AutoBattleState.WaitingCards : AutoBattleState.ReadyToStartBattle;
        }

        if (stage.Phase == StageController.StagePhase.ApplyEnemyCardPhase)
        {
            return AutoBattleState.WaitingRoll;
        }

        if (stage.battleState == StageController.BattleState.Battle)
        {
            return AutoBattleState.ResolvingBattle;
        }

        return AutoBattleState.Unknown;
    }

    public static string BuildDetails(StageController stage)
    {
        if (stage == null)
        {
            return "stage=null";
        }

        int totalDice;
        int assignedDice;
        int brokenDice;
        CountPlayerDice(out totalDice, out assignedDice, out brokenDice);
        return string.Format(
            "rawState={0}, phase={1}, battleState={2}, round={3}, playerDice={4}, assigned={5}, broken={6}",
            stage.State,
            stage.Phase,
            stage.battleState,
            stage.RoundTurn,
            totalDice,
            assignedDice,
            brokenDice);
    }

    private static bool HasUnassignedUsablePlayerDice()
    {
        List<BattleUnitModel> players = BattleObjectManager.instance.GetAliveList(Faction.Player);
        foreach (BattleUnitModel unit in players)
        {
            if (unit == null || unit.speedDiceResult == null || unit.cardSlotDetail == null || unit.cardSlotDetail.cardAry == null)
            {
                continue;
            }

            for (int i = 0; i < unit.speedDiceResult.Count && i < unit.cardSlotDetail.cardAry.Count; i++)
            {
                if (!unit.speedDiceResult[i].breaked && unit.cardSlotDetail.cardAry[i] == null)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static void CountPlayerDice(out int totalDice, out int assignedDice, out int brokenDice)
    {
        totalDice = 0;
        assignedDice = 0;
        brokenDice = 0;

        List<BattleUnitModel> players = BattleObjectManager.instance.GetAliveList(Faction.Player);
        foreach (BattleUnitModel unit in players)
        {
            if (unit == null || unit.speedDiceResult == null)
            {
                continue;
            }

            totalDice += unit.speedDiceResult.Count;
            for (int i = 0; i < unit.speedDiceResult.Count; i++)
            {
                if (unit.speedDiceResult[i].breaked)
                {
                    brokenDice++;
                }

                if (unit.cardSlotDetail != null &&
                    unit.cardSlotDetail.cardAry != null &&
                    i < unit.cardSlotDetail.cardAry.Count &&
                    unit.cardSlotDetail.cardAry[i] != null)
                {
                    assignedDice++;
                }
            }
        }
    }
}
