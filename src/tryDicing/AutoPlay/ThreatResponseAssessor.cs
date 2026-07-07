public static class ThreatResponseAssessor
{
    public static ThreatResponseAssessment Assess(ThreatAssessment threat, ActionCandidate candidate)
    {
        if (threat == null || threat.EnemyAction == null || candidate == null)
        {
            return BuildNone(threat, candidate, "missing threat or candidate");
        }

        DeclaredAction enemyAction = threat.EnemyAction;
        if (IsClashingThreatDie(enemyAction, candidate))
        {
            return new ThreatResponseAssessment(
                threat,
                candidate,
                ResponseMechanism.ClashThreatDie,
                OwnerDamageOutcome.None,
                null,
                threat.UnansweredDangerScore,
                "candidate clashes with the threatening speed die");
        }

        if (candidate.Target == enemyAction.Owner)
        {
            DamageEstimate ownerDamage = DamageEstimator.Estimate(candidate.Card, candidate.Actor, enemyAction.Owner);
            OwnerDamageOutcome outcome = DetermineOwnerDamageOutcome(enemyAction.Owner, ownerDamage);
            return new ThreatResponseAssessment(
                threat,
                candidate,
                ResponseMechanism.AttackThreatOwner,
                outcome,
                ownerDamage,
                CalculateOwnerAttackResponseScore(threat, outcome),
                "candidate attacks the threat owner; outcome=" + outcome + "; " + ownerDamage.Reason);
        }

        return BuildNone(threat, candidate, "candidate does not interact with this threat");
    }

    private static bool IsClashingThreatDie(DeclaredAction enemyAction, ActionCandidate candidate)
    {
        if (enemyAction.Owner == null || candidate.Target != enemyAction.Owner)
        {
            return false;
        }

        if (candidate.TargetSpeedDiceIndex != enemyAction.SpeedDiceIndex)
        {
            return false;
        }

        if (enemyAction.Target == candidate.Actor)
        {
            return true;
        }

        return CanRedirectToThreatDie(enemyAction, candidate);
    }

    private static bool CanRedirectToThreatDie(DeclaredAction enemyAction, ActionCandidate candidate)
    {
        if (candidate.Actor == null || enemyAction.Owner == null)
        {
            return false;
        }

        return candidate.Actor.CanChangeAttackTarget(
            enemyAction.Owner,
            candidate.ActorSpeedDiceIndex,
            enemyAction.SpeedDiceIndex);
    }

    private static OwnerDamageOutcome DetermineOwnerDamageOutcome(BattleUnitModel owner, DamageEstimate damage)
    {
        if (owner == null || damage == null)
        {
            return OwnerDamageOutcome.None;
        }

        if (damage.MinHpDamage >= owner.hp)
        {
            return OwnerDamageOutcome.GuaranteedKill;
        }

        if (damage.ExpectedHpDamage >= owner.hp)
        {
            return OwnerDamageOutcome.ExpectedKill;
        }

        if (damage.MinBreakDamage >= GetBreakGauge(owner))
        {
            return OwnerDamageOutcome.GuaranteedStagger;
        }

        if (damage.ExpectedBreakDamage >= GetBreakGauge(owner))
        {
            return OwnerDamageOutcome.ExpectedStagger;
        }

        if (damage.MaxHpDamage >= owner.hp)
        {
            return OwnerDamageOutcome.PotentialKill;
        }

        if (damage.MaxBreakDamage >= GetBreakGauge(owner))
        {
            return OwnerDamageOutcome.PotentialStagger;
        }

        if (damage.ExpectedHpDamage > 0f || damage.ExpectedBreakDamage > 0f)
        {
            return OwnerDamageOutcome.Pressure;
        }

        return OwnerDamageOutcome.None;
    }

    private static int GetBreakGauge(BattleUnitModel owner)
    {
        if (owner == null || owner.breakDetail == null)
        {
            return int.MaxValue;
        }

        return owner.breakDetail.breakGauge;
    }

    private static float CalculateOwnerAttackResponseScore(ThreatAssessment threat, OwnerDamageOutcome outcome)
    {
        if (threat == null)
        {
            return 0f;
        }

        float baseScore = threat.UnansweredDangerScore;
        switch (outcome)
        {
            case OwnerDamageOutcome.GuaranteedKill:
                return baseScore * 1.1f;
            case OwnerDamageOutcome.ExpectedKill:
                return baseScore * 0.95f;
            case OwnerDamageOutcome.GuaranteedStagger:
                return baseScore * 0.85f;
            case OwnerDamageOutcome.ExpectedStagger:
                return baseScore * 0.7f;
            case OwnerDamageOutcome.PotentialKill:
                return baseScore * 0.55f;
            case OwnerDamageOutcome.PotentialStagger:
                return baseScore * 0.4f;
            case OwnerDamageOutcome.Pressure:
                return baseScore * 0.15f;
            default:
                return 0f;
        }
    }

    private static ThreatResponseAssessment BuildNone(ThreatAssessment threat, ActionCandidate candidate, string reason)
    {
        return new ThreatResponseAssessment(
            threat,
            candidate,
            ResponseMechanism.None,
            OwnerDamageOutcome.None,
            null,
            0f,
            reason);
    }
}
