public static class AutoBattleStateProbe
{
    private static AutoBattleState _lastState = AutoBattleState.Unknown;
    private static string _lastDetails = string.Empty;
    private static float _elapsedSinceLastLog;

    public static void Sample(StageController stage, float deltaTime)
    {
        _elapsedSinceLastLog += deltaTime;

        AutoBattleState state = AutoBattleStateReader.Read(stage);
        string details = AutoBattleStateReader.BuildDetails(stage);
        bool changed = state != _lastState || details != _lastDetails;
        bool heartbeat = _elapsedSinceLastLog >= 5f && state != AutoBattleState.NotInBattle;

        if (!changed && !heartbeat)
        {
            return;
        }

        _lastState = state;
        _lastDetails = details;
        _elapsedSinceLastLog = 0f;

        TryDicingLogger.Info("AutoBattleState=" + state + " | " + details);
    }
}
