using System;
using System.Reflection;
using HarmonyLib;

public static class AutoBattleActionInvoker
{
    private static readonly MethodInfo CheckInputMethod =
        AccessTools.Method(typeof(StageController), "CheckInput", new[] { typeof(bool) });

    private static readonly MethodInfo StopSpeedDiceRollMethod =
        AccessTools.Method(typeof(StageController), "StopSpeedDiceRoll");

    private static readonly MethodInfo SetAutoCardForPlayerMethod =
        AccessTools.Method(typeof(StageController), "SetAutoCardForPlayer");

    private static readonly MethodInfo CompleteApplyingLibrarianCardPhaseMethod =
        AccessTools.Method(typeof(StageController), "CompleteApplyingLibrarianCardPhase", new[] { typeof(bool) });

    public static bool SkipRoundStartUi(StageController stage)
    {
        return Invoke(stage, CheckInputMethod, new object[] { true }, "CheckInput(true)");
    }

    public static bool StopSpeedDiceRoll(StageController stage)
    {
        return Invoke(stage, StopSpeedDiceRollMethod, null, "StopSpeedDiceRoll()");
    }

    public static bool SetAutoCardForPlayer(StageController stage)
    {
        return Invoke(stage, SetAutoCardForPlayerMethod, null, "SetAutoCardForPlayer()");
    }

    public static bool CompleteApplyingLibrarianCardPhase(StageController stage)
    {
        return Invoke(stage, CompleteApplyingLibrarianCardPhaseMethod, new object[] { true }, "CompleteApplyingLibrarianCardPhase(true)");
    }

    private static bool Invoke(StageController stage, MethodInfo method, object[] args, string actionName)
    {
        if (stage == null)
        {
            TryDicingLogger.Info("AutoBattle action skipped: stage is null. action=" + actionName);
            return false;
        }

        if (method == null)
        {
            TryDicingLogger.Error("AutoBattle action missing original method: " + actionName);
            return false;
        }

        try
        {
            method.Invoke(stage, args);
            return true;
        }
        catch (TargetInvocationException ex)
        {
            Exception inner = ex.InnerException ?? ex;
            TryDicingLogger.Error("AutoBattle action failed: " + actionName + " | " + inner);
            return false;
        }
        catch (Exception ex)
        {
            TryDicingLogger.Error("AutoBattle action failed: " + actionName + " | " + ex);
            return false;
        }
    }
}
