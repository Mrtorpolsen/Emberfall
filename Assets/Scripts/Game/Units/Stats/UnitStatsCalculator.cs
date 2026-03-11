using System.Collections.Generic;
using UnityEngine;

public class UnitStatsCalculator
{
    public void ApplyModifiers(ref FinalStats stats, UnitStatsDefinition baseStats, IEnumerable<AppliedStatModifier> modifiers)
    {
        foreach (AppliedStatModifier modifier in modifiers)
        {
            foreach (AppliedEffect effect in modifier.Effects)
            {
                float magnitude = effect.Operation switch
                {
                    EffectOperation.Add => effect.Value * modifier.Stacks,
                    EffectOperation.Multiply => 1 + (effect.Value - 1) * modifier.Stacks,
                    EffectOperation.Set => effect.Value,
                    _ => effect.Value
                };

                ApplyEffect(effect.Target, effect.Operation, magnitude, stats, baseStats);
            }
        }
    }

    private void ApplyEffect(EffectTarget target, EffectOperation operation,
        float value, FinalStats stats, UnitStatsDefinition baseStats)
    {
        switch (target)
        {
            case EffectTarget.Health:
                stats.maxHealth += CalcEffectValue(baseStats.maxHealth, operation, value);
                break;

            case EffectTarget.AttackDamage:
                stats.attackDamage += CalcEffectValue(baseStats.attackDamage, operation, value);
                break;

            case EffectTarget.AttackSpeed:
                stats.attackSpeed += CalcEffectValue(baseStats.attackSpeed, operation, value);
                break;

            case EffectTarget.AttackRange:
                stats.attackRange += CalcEffectValue(baseStats.attackRange, operation, value);
                break;

            case EffectTarget.Armor:
                stats.armor += CalcEffectValue(baseStats.armor, operation, value);
                break;

            case EffectTarget.CritChance:
                stats.critChance += CalcEffectValue(baseStats.critChance, operation, value);
                break;

            case EffectTarget.CritDamage:
                stats.critDamage += CalcEffectValue(baseStats.critMultiplier, operation, value);
                break;
        }
    }

    private int CalcEffectValue(int stat, EffectOperation operation, float value)
    {
        int intValue = Mathf.RoundToInt(value);

        switch (operation)
        {
            case EffectOperation.Add:
                return intValue;

            case EffectOperation.Multiply:
                return Mathf.RoundToInt((stat * value) - stat);

            case EffectOperation.Set:
                return intValue - stat;
        }
        return 0;
    }

    private float CalcEffectValue(float stat, EffectOperation operation, float value)
    {
        switch (operation)
        {
            case EffectOperation.Add:
                return value;

            case EffectOperation.Multiply:
                return (value * stat) - stat;

            case EffectOperation.Set:
                return value - stat;
        }

        return 0;
    }

    public FinalStats CalculateEnemyStats(float waveIndex, FinalStats finalStats)
    {
        float wave = waveIndex + 1;
        int hpPercent = Mathf.RoundToInt(2f * Mathf.Pow(wave, 0.85f));
        int dmgPercent = Mathf.RoundToInt(2f * Mathf.Pow(wave, 0.75f));

        float hpMultiplier = 1f + (hpPercent * 0.01f);
        float dmgMultiplier = 1f + (dmgPercent * 0.01f);

        ApplyEnemyEffect(EffectTarget.Health, EffectOperation.Multiply, hpMultiplier, ref finalStats);
        ApplyEnemyEffect(EffectTarget.AttackDamage, EffectOperation.Multiply, dmgMultiplier, ref finalStats);

        return finalStats;
    }

    private void ApplyEnemyEffect(EffectTarget target, EffectOperation operation,
        float value, ref FinalStats stats)
    {
        switch (target)
        {
            case EffectTarget.Health:
                ApplyEmemy(ref stats.maxHealth, operation, value);
                break;

            case EffectTarget.AttackDamage:
                ApplyEmemy(ref stats.attackDamage, operation, value);
                break;

            case EffectTarget.AttackSpeed:
                ApplyEmemy(ref stats.attackSpeed, operation, value);
                break;

            case EffectTarget.AttackRange:
                ApplyEmemy(ref stats.attackRange, operation, value);
                break;

            case EffectTarget.Armor:
                ApplyEmemy(ref stats.armor, operation, value);
                break;

            case EffectTarget.CritChance:
                ApplyEmemy(ref stats.critChance, operation, value);
                break;

            case EffectTarget.CritDamage:
                ApplyEmemy(ref stats.critDamage, operation, value);
                break;
        }
    }

    private void ApplyEmemy(ref int stat, EffectOperation operation, float value)
    {
        int intValue = Mathf.RoundToInt(value);

        switch (operation)
        {
            case EffectOperation.Add:
                stat += intValue;
                break;

            case EffectOperation.Multiply:
                stat = Mathf.RoundToInt(stat * value);
                break;

            case EffectOperation.Set:
                stat = intValue;
                break;
        }
    }

    private void ApplyEmemy(ref float stat, EffectOperation operation, float value)
    {
        switch (operation)
        {
            case EffectOperation.Add:
                stat += value;
                break;

            case EffectOperation.Multiply:
                stat *= value;
                break;

            case EffectOperation.Set:
                stat = value;
                break;
        }
    }
}
