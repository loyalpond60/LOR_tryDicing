using System.Collections.Generic;
using HarmonyLib;

[HarmonyPatch(typeof(LevelUpUI), "Init")]
public static class AutoEmotionPassiveChoicePatch
{
    public static bool Prefix(int count, List<EmotionCardXmlInfo> cardList)
    {
        if (cardList == null || cardList.Count == 0)
        {
            return true;
        }

        StageController stage = Singleton<StageController>.Instance;
        StageLibraryFloorModel floor = stage == null ? null : stage.GetCurrentStageFloorModel();
        if (floor == null)
        {
            return true;
        }

        EmotionCardXmlInfo card = RandomUtil.SelectOne<EmotionCardXmlInfo>(cardList);
        if (card == null)
        {
            return true;
        }

        BattleUnitModel target = null;
        if (card.TargetType == EmotionTargetType.SelectOne)
        {
            target = SelectRandomPlayerTarget();
            if (target == null)
            {
                TryDicingLogger.Info("AutoEmotion fallback: no player target for passive card=" + card.Name);
                return true;
            }
        }

        floor.OnPickPassiveCard(card, target);
        if (card.TargetType == EmotionTargetType.AllIncludingEnemy)
        {
            StageWaveModel wave = stage.GetCurrentWaveModel();
            if (wave != null)
            {
                wave.ApplyEmotionCard(card);
            }
        }

        HideLevelUpUi();
        TryDicingLogger.Info(string.Format(
            "AutoEmotion selected passive card={0}, targetType={1}, targetId={2}, candidates={3}, selectedCount={4}",
            card.Name,
            card.TargetType,
            target == null ? 0 : target.id,
            cardList.Count,
            count));
        return false;
    }

    private static BattleUnitModel SelectRandomPlayerTarget()
    {
        List<BattleUnitModel> targets = BattleObjectManager.instance.GetAliveList(Faction.Player);
        if (targets == null)
        {
            return null;
        }

        targets.RemoveAll((BattleUnitModel unit) => unit == null || unit.IsExtinction());
        if (targets.Count == 0)
        {
            return null;
        }

        return RandomUtil.SelectOne<BattleUnitModel>(targets);
    }

    private static void HideLevelUpUi()
    {
        if (SingletonBehavior<BattleManagerUI>.Instance != null && SingletonBehavior<BattleManagerUI>.Instance.ui_levelup != null)
        {
            SingletonBehavior<BattleManagerUI>.Instance.ui_levelup.SetRootCanvas(false);
        }
    }
}

[HarmonyPatch(typeof(LevelUpUI), "InitEgo")]
public static class AutoEmotionEgoChoicePatch
{
    public static bool Prefix(int count, List<EmotionEgoXmlInfo> egoList)
    {
        if (egoList == null || egoList.Count == 0)
        {
            return true;
        }

        StageController stage = Singleton<StageController>.Instance;
        StageLibraryFloorModel floor = stage == null ? null : stage.GetCurrentStageFloorModel();
        if (floor == null)
        {
            return true;
        }

        EmotionEgoXmlInfo ego = RandomUtil.SelectOne<EmotionEgoXmlInfo>(egoList);
        if (ego == null)
        {
            return true;
        }

        floor.OnPickEgoCard(ego);
        HideLevelUpUi();
        TryDicingLogger.Info(string.Format(
            "AutoEmotion selected ego egoId={0}, cardId={1}, candidates={2}, selectedCount={3}",
            ego.id,
            ego.CardId,
            egoList.Count,
            count));
        return false;
    }

    private static void HideLevelUpUi()
    {
        if (SingletonBehavior<BattleManagerUI>.Instance != null && SingletonBehavior<BattleManagerUI>.Instance.ui_levelup != null)
        {
            SingletonBehavior<BattleManagerUI>.Instance.ui_levelup.SetRootCanvas(false);
        }
    }
}
