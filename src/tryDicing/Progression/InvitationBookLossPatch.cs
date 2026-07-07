using System.Collections.Generic;
using HarmonyLib;

[HarmonyPatch(typeof(StageController), "GameOver")]
public static class InvitationBookLossPatch
{
    public static void Prefix(StageController __instance, bool iswin)
    {
        if (__instance == null || iswin)
        {
            return;
        }

        List<LorId> usedBooks = __instance.UsedBooks;
        if (usedBooks == null || usedBooks.Count == 0)
        {
            return;
        }

        int count = usedBooks.Count;
        usedBooks.Clear();
        TryDicingLogger.Info("Invitation book loss suppressed. count=" + count);
    }
}
