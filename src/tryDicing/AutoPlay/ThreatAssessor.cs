using System.Collections.Generic;

public static class ThreatAssessor
{
    public static List<ThreatAssessment> Assess(BattleSnapshot snapshot)
    {
        List<ThreatAssessment> assessments = new List<ThreatAssessment>();
        if (snapshot == null || snapshot.DeclaredActions == null)
        {
            return assessments;
        }

        foreach (DeclaredAction action in snapshot.DeclaredActions)
        {
            if (action == null || action.OwnerFaction == snapshot.ActorFaction)
            {
                continue;
            }

            assessments.Add(AssessEnemyAction(action));
        }

        return assessments;
    }

    private static ThreatAssessment AssessEnemyAction(DeclaredAction action)
    {
        DamageEstimate damage = DamageEstimator.Estimate(action.Card, action.Owner, action.Target);
        bool guaranteedKillRisk = HasGuaranteedKillRisk(action, damage);
        bool expectedKillRisk = HasExpectedKillRisk(action, damage);
        bool potentialKillRisk = HasPotentialKillRisk(action, damage);
        bool guaranteedStaggerRisk = HasGuaranteedStaggerRisk(action, damage);
        bool expectedStaggerRisk = HasExpectedStaggerRisk(action, damage);
        bool potentialStaggerRisk = HasPotentialStaggerRisk(action, damage);
        float hpPressureRatio = CalculateHpPressureRatio(action, damage);
        float breakPressureRatio = CalculateBreakPressureRatio(action, damage);

        ThreatLevel level = DetermineLevel(
            guaranteedKillRisk,
            expectedKillRisk,
            potentialKillRisk,
            guaranteedStaggerRisk,
            expectedStaggerRisk,
            potentialStaggerRisk,
            hpPressureRatio,
            breakPressureRatio);

        float score = CalculateUnansweredDangerScore(
            damage,
            level,
            guaranteedKillRisk,
            expectedKillRisk,
            potentialKillRisk,
            guaranteedStaggerRisk,
            expectedStaggerRisk,
            potentialStaggerRisk);

        string reason = BuildReason(
            level,
            guaranteedKillRisk,
            expectedKillRisk,
            potentialKillRisk,
            guaranteedStaggerRisk,
            expectedStaggerRisk,
            potentialStaggerRisk,
            hpPressureRatio,
            breakPressureRatio);

        return new ThreatAssessment(
            action,
            level,
            score,
            damage,
            hpPressureRatio,
            breakPressureRatio,
            guaranteedKillRisk,
            expectedKillRisk,
            potentialKillRisk,
            guaranteedStaggerRisk,
            expectedStaggerRisk,
            potentialStaggerRisk,
            reason + "; hpPressure=" + hpPressureRatio.ToString("0.00") + ", breakPressure=" + breakPressureRatio.ToString("0.00") + "; " + damage.Reason);
    }

    private static ThreatLevel DetermineLevel(
        bool guaranteedKillRisk,
        bool expectedKillRisk,
        bool potentialKillRisk,
        bool guaranteedStaggerRisk,
        bool expectedStaggerRisk,
        bool potentialStaggerRisk,
        float hpPressureRatio,
        float breakPressureRatio)
    {
        if (guaranteedKillRisk
            || expectedKillRisk
            || guaranteedStaggerRisk
            || expectedStaggerRisk
            || hpPressureRatio >= 0.75f
            || breakPressureRatio >= 0.75f)
        {
            return ThreatLevel.Critical;
        }

        if (potentialKillRisk
            || potentialStaggerRisk
            || hpPressureRatio >= 0.4f
            || breakPressureRatio >= 0.4f)
        {
            return ThreatLevel.Major;
        }

        return ThreatLevel.Minor;
    }

    private static float CalculateUnansweredDangerScore(
        DamageEstimate damage,
        ThreatLevel level,
        bool guaranteedKillRisk,
        bool expectedKillRisk,
        bool potentialKillRisk,
        bool guaranteedStaggerRisk,
        bool expectedStaggerRisk,
        bool potentialStaggerRisk)
    {
        float burstHp = damage.MaxHpDamage - damage.ExpectedHpDamage;
        float burstBreak = damage.MaxBreakDamage - damage.ExpectedBreakDamage;
        float score = damage.ExpectedHpDamage * 0.45f
            + damage.ExpectedBreakDamage * 0.55f
            + burstHp * 0.15f
            + burstBreak * 0.15f;

        if (guaranteedKillRisk)
        {
            score += 30f;
        }
        else if (expectedKillRisk)
        {
            score += 24f;
        }
        else if (potentialKillRisk)
        {
            score += 12f;
        }

        if (guaranteedStaggerRisk)
        {
            score += 24f;
        }
        else if (expectedStaggerRisk)
        {
            score += 18f;
        }
        else if (potentialStaggerRisk)
        {
            score += 8f;
        }

        if (level == ThreatLevel.Critical)
        {
            score += 40f;
        }
        else if (level == ThreatLevel.Major)
        {
            score += 18f;
        }

        return score;
    }

    private static string BuildReason(
        ThreatLevel level,
        bool guaranteedKillRisk,
        bool expectedKillRisk,
        bool potentialKillRisk,
        bool guaranteedStaggerRisk,
        bool expectedStaggerRisk,
        bool potentialStaggerRisk,
        float hpPressureRatio,
        float breakPressureRatio)
    {
        if (guaranteedKillRisk)
        {
            return "guaranteed HP damage can kill target";
        }

        if (expectedKillRisk)
        {
            return "expected HP damage can kill target";
        }

        if (guaranteedStaggerRisk)
        {
            return "guaranteed break damage can stagger target";
        }

        if (expectedStaggerRisk)
        {
            return "expected break damage can stagger target";
        }

        if (hpPressureRatio >= 0.75f)
        {
            return "critical HP pressure";
        }

        if (breakPressureRatio >= 0.75f)
        {
            return "critical break pressure";
        }

        if (potentialKillRisk)
        {
            return "potential HP damage can kill target";
        }

        if (potentialStaggerRisk)
        {
            return "potential break damage can stagger target";
        }

        if (hpPressureRatio >= 0.4f)
        {
            return "major HP pressure";
        }

        if (breakPressureRatio >= 0.4f)
        {
            return "major break pressure";
        }

        return level == ThreatLevel.Major ? "major enemy action" : "minor enemy action";
    }

    private static bool HasGuaranteedKillRisk(DeclaredAction action, DamageEstimate damage)
    {
        return action.Target != null && action.IsTargetingOpponent && damage.MinHpDamage >= action.Target.hp;
    }

    private static bool HasExpectedKillRisk(DeclaredAction action, DamageEstimate damage)
    {
        return action.Target != null && action.IsTargetingOpponent && damage.ExpectedHpDamage >= action.Target.hp;
    }

    private static bool HasPotentialKillRisk(DeclaredAction action, DamageEstimate damage)
    {
        return action.Target != null && action.IsTargetingOpponent && damage.MaxHpDamage >= action.Target.hp;
    }

    private static bool HasGuaranteedStaggerRisk(DeclaredAction action, DamageEstimate damage)
    {
        return action.Target != null
            && action.Target.breakDetail != null
            && action.IsTargetingOpponent
            && damage.MinBreakDamage >= action.Target.breakDetail.breakGauge;
    }

    private static bool HasExpectedStaggerRisk(DeclaredAction action, DamageEstimate damage)
    {
        return action.Target != null
            && action.Target.breakDetail != null
            && action.IsTargetingOpponent
            && damage.ExpectedBreakDamage >= action.Target.breakDetail.breakGauge;
    }

    private static bool HasPotentialStaggerRisk(DeclaredAction action, DamageEstimate damage)
    {
        return action.Target != null
            && action.Target.breakDetail != null
            && action.IsTargetingOpponent
            && damage.MaxBreakDamage >= action.Target.breakDetail.breakGauge;
    }

    private static float CalculateHpPressureRatio(DeclaredAction action, DamageEstimate damage)
    {
        if (action.Target == null || action.Target.hp <= 0)
        {
            return 0f;
        }

        return damage.ExpectedHpDamage / action.Target.hp;
    }

    private static float CalculateBreakPressureRatio(DeclaredAction action, DamageEstimate damage)
    {
        if (action.Target == null || action.Target.breakDetail == null || action.Target.breakDetail.breakGauge <= 0)
        {
            return 0f;
        }

        return damage.ExpectedBreakDamage / action.Target.breakDetail.breakGauge;
    }
}
