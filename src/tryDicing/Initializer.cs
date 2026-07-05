using HarmonyLib;

public class Initializer : ModInitializer
{
    public override void OnInitializeMod()
    {
        new Harmony("tryDicing.autoplay.smoke").PatchAll();
        TryDicingLogger.Info("Harmony smoke patch installed. fileLog=" + TryDicingLogger.LogFilePath);
    }
}
