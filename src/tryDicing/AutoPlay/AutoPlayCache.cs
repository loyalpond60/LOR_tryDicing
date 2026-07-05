public static class AutoPlayCache
{
    private static BattlePlan _currentPlan;

    public static BattlePlan GetOrCreate(BattleSnapshot snapshot)
    {
        if (_currentPlan == null || _currentPlan.PlanKey != snapshot.PlanKey)
        {
            _currentPlan = TacticalPlanner.CreatePlan(snapshot);
        }

        return _currentPlan;
    }

    public static void Clear()
    {
        _currentPlan = null;
    }
}
