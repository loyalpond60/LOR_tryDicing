using HarmonyLib;
using UnityEngine;

[HarmonyPatch(typeof(StageController), "OnUpdate")]
public static class AutoBattleProbePatch
{
    public static void Postfix(StageController __instance)
    {
        try
        {
            float deltaTime = Time.deltaTime;
            AutoBattleStateProbe.Sample(__instance, deltaTime);
            AutoBattleController.Update(__instance, deltaTime);
        }
        catch (System.Exception ex)
        {
            TryDicingLogger.Error("AutoBattle update failed: " + ex);
        }
    }
}
