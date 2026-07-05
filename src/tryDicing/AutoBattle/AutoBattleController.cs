public static class AutoBattleController
{
    private const float MinPhaseDelay = 0.35f;
    private const float StartBattleDelayAfterAutoCard = 0.60f;

    private static StageController.StagePhase _lastPhase;
    private static bool _hasLastPhase;
    private static float _phaseElapsed;
    private static int _currentRound = -1;
    private static bool _skippedRoundStartUi;
    private static bool _stoppedSpeedDice;
    private static bool _setAutoCards;
    private static bool _startedBattle;
    private static float _elapsedAfterAutoCards;

    public static void Update(StageController stage, float deltaTime)
    {
        AutoBattleState state = AutoBattleStateReader.Read(stage);
        TrackPhase(stage, deltaTime);

        if (state == AutoBattleState.NotInBattle || state == AutoBattleState.BattleEnded)
        {
            ResetRound();
            return;
        }

        if (stage == null || stage.State != StageController.StageState.Battle)
        {
            return;
        }

        TrackRound(stage.RoundTurn);

        if (_phaseElapsed < MinPhaseDelay)
        {
            return;
        }

        if (TryAdvanceRoll(stage))
        {
            return;
        }

        if (TrySetAutoCards(stage))
        {
            return;
        }

        TryStartBattle(stage, deltaTime);
    }

    private static void TrackPhase(StageController stage, float deltaTime)
    {
        if (stage == null)
        {
            _hasLastPhase = false;
            _phaseElapsed = 0f;
            return;
        }

        if (!_hasLastPhase || _lastPhase != stage.Phase)
        {
            _lastPhase = stage.Phase;
            _hasLastPhase = true;
            _phaseElapsed = 0f;
            return;
        }

        _phaseElapsed += deltaTime;
    }

    private static void TrackRound(int round)
    {
        if (_currentRound == round)
        {
            return;
        }

        _currentRound = round;
        _skippedRoundStartUi = false;
        _stoppedSpeedDice = false;
        _setAutoCards = false;
        _startedBattle = false;
        _elapsedAfterAutoCards = 0f;
        AutoPlayCache.Clear();
        TryDicingLogger.Info("AutoBattle round begin. round=" + round);
    }

    private static bool TryAdvanceRoll(StageController stage)
    {
        if (stage.Phase == StageController.StagePhase.RoundStartPhase_UI && !_skippedRoundStartUi)
        {
            if (AutoBattleActionInvoker.SkipRoundStartUi(stage))
            {
                _skippedRoundStartUi = true;
                TryDicingLogger.Info("AutoBattle action: SkipRoundStartUi round=" + stage.RoundTurn);
                return true;
            }
        }

        if (stage.Phase == StageController.StagePhase.RoundStartPhase_System && !_stoppedSpeedDice)
        {
            if (AutoBattleActionInvoker.StopSpeedDiceRoll(stage))
            {
                _stoppedSpeedDice = true;
                TryDicingLogger.Info("AutoBattle action: StopSpeedDiceRoll round=" + stage.RoundTurn);
                return true;
            }
        }

        return false;
    }

    private static bool TrySetAutoCards(StageController stage)
    {
        if (stage.Phase != StageController.StagePhase.ApplyLibrarianCardPhase || _setAutoCards)
        {
            return false;
        }

        if (AutoBattleActionInvoker.SetAutoCardForPlayer(stage))
        {
            _setAutoCards = true;
            _elapsedAfterAutoCards = 0f;
            TryDicingLogger.Info("AutoBattle action: SetAutoCardForPlayer round=" + stage.RoundTurn);
            return true;
        }

        return false;
    }

    private static void TryStartBattle(StageController stage, float deltaTime)
    {
        if (stage.Phase != StageController.StagePhase.ApplyLibrarianCardPhase || !_setAutoCards || _startedBattle)
        {
            return;
        }

        _elapsedAfterAutoCards += deltaTime;
        if (_elapsedAfterAutoCards < StartBattleDelayAfterAutoCard)
        {
            return;
        }

        if (AutoBattleActionInvoker.CompleteApplyingLibrarianCardPhase(stage))
        {
            _startedBattle = true;
            TryDicingLogger.Info("AutoBattle action: CompleteApplyingLibrarianCardPhase round=" + stage.RoundTurn);
        }
    }

    private static void ResetRound()
    {
        _currentRound = -1;
        _skippedRoundStartUi = false;
        _stoppedSpeedDice = false;
        _setAutoCards = false;
        _startedBattle = false;
        _elapsedAfterAutoCards = 0f;
    }
}
