using HarmonyLib;

[HarmonyPatch(typeof(BattleAllyCardDetail), "PlayTurnAutoForPlayer")]
public static class AutoPlayPatch
{
    public static bool Prefix(BattleAllyCardDetail __instance, int idx)
    {
        TryDicingLogger.Info("PlayTurnAutoForPlayer intercepted. idx=" + idx);
        if (AutoPlayController.TryPlay(__instance, idx))
        {
            return false;
        }

        return true;
    }
}
