public static class LocalActionEvaluator
{
    public static LocalActionEvaluation Evaluate(ActionCandidate candidate, int remainingLight)
    {
        BattleDiceCardModel card = candidate.Card;
        BattleUnitModel target = candidate.Target;

        DiceProbabilityProfile diceProfile = DiceProbabilityCalculator.Calculate(card);
        float cardPowerScore = card.GetCost() * 8f + diceProfile.ExpectedTotalPower;
        float targetScore = ScoreTarget(target);
        float clashScore = candidate.InteractionType == InteractionType.Clash ? 10f : 2f;
        float damageScore = ScoreDamage(target, diceProfile);
        float staggerScore = ScoreStagger(target, diceProfile);
        float resourcePenalty = ScoreResourcePenalty(card, remainingLight);
        float wastePenalty = ScoreWastePenalty(card, target, diceProfile.ExpectedAttackPower);
        float totalScore = cardPowerScore + targetScore + clashScore + damageScore + staggerScore - resourcePenalty - wastePenalty;

        string reason = string.Format(
            "cardPower={0:0.0}, target={1:0.0}, clash={2:0.0}, damage={3:0.0}, stagger={4:0.0}, expectedHp={5:0.0}, maxHp={6:0.0}, expectedStagger={7:0.0}, resourcePenalty={8:0.0}, wastePenalty={9:0.0}, interaction={10}",
            cardPowerScore,
            targetScore,
            clashScore,
            damageScore,
            staggerScore,
            diceProfile.ExpectedAttackPower,
            diceProfile.MaxAttackPower,
            diceProfile.ExpectedAttackPower,
            resourcePenalty,
            wastePenalty,
            candidate.InteractionType);

        return new LocalActionEvaluation(
            candidate,
            cardPowerScore,
            targetScore,
            clashScore,
            damageScore,
            staggerScore,
            resourcePenalty,
            wastePenalty,
            totalScore,
            reason);
    }

    private static float ScoreTarget(BattleUnitModel target)
    {
        if (target == null)
        {
            return 0f;
        }

        float hpPressure = target.MaxHp <= 0 ? 0f : (1f - (target.hp / (float)target.MaxHp)) * 12f;
        float lowHpBonus = target.hp <= 15 ? 8f : 0f;
        float nearBreakBonus = target.breakDetail != null && target.breakDetail.breakGauge <= 20 ? 6f : 0f;
        return hpPressure + lowHpBonus + nearBreakBonus;
    }

    private static float ScoreDamage(BattleUnitModel target, DiceProbabilityProfile diceProfile)
    {
        if (target == null || diceProfile == null || diceProfile.ExpectedAttackPower <= 0f)
        {
            return 0f;
        }

        float pressure = diceProfile.ExpectedAttackPower * 0.35f;
        if (target.hp <= diceProfile.ExpectedAttackPower)
        {
            return pressure + 18f;
        }

        if (target.hp <= diceProfile.MaxAttackPower)
        {
            return pressure + 8f;
        }

        return pressure;
    }

    private static float ScoreStagger(BattleUnitModel target, DiceProbabilityProfile diceProfile)
    {
        if (target == null || target.breakDetail == null || diceProfile == null || diceProfile.ExpectedAttackPower <= 0f)
        {
            return 0f;
        }

        int breakGauge = target.breakDetail.breakGauge;
        float pressure = diceProfile.ExpectedAttackPower * 0.25f;
        if (breakGauge <= diceProfile.ExpectedAttackPower)
        {
            return pressure + 14f;
        }

        if (breakGauge <= diceProfile.MaxAttackPower)
        {
            return pressure + 6f;
        }

        return pressure;
    }

    private static float ScoreResourcePenalty(BattleDiceCardModel card, int remainingLight)
    {
        if (card == null)
        {
            return 0f;
        }

        int lightAfterUse = remainingLight - card.GetCost();
        if (lightAfterUse >= 2)
        {
            return 0f;
        }

        if (lightAfterUse == 1)
        {
            return 2f;
        }

        return 5f;
    }

    private static float ScoreWastePenalty(BattleDiceCardModel card, BattleUnitModel target, float dicePower)
    {
        if (card == null || target == null)
        {
            return 0f;
        }

        float likelyDamage = dicePower;
        float overkill = likelyDamage - target.hp;
        if (overkill <= 0f)
        {
            return 0f;
        }

        float penalty = overkill * 0.5f;
        if (card.GetCost() >= 3 && target.hp <= 10)
        {
            penalty += 8f;
        }

        return penalty;
    }
}
