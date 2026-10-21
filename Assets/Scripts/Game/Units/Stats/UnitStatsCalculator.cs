using System;
using System.Collections.Generic;
using UnityEngine;

public class UnitStatsCalculator
{
    // Keeps enemy ranged units from outranging the player's ranged units once scaling kicks in
    private const float EnemyRangedRangeMultiplier = 0.8f;

    public void ApplyModifiers(ref FinalStats stats, UnitStatsDefinition baseStats, IEnumerable<AppliedStatModifier> modifiers)
    {
        foreach (AppliedStatModifier modifier in modifiers)
        {
            foreach (StatEffect effect in modifier.Effects)
            {
                float magnitude = effect.Operation switch
                {
                    EffectOperation.Add => effect.Value * modifier.Stacks,
                    EffectOperation.Subtract => effect.Value * modifier.Stacks,
                    EffectOperation.Multiply => 1 + (effect.Value - 1) * modifier.Stacks,
                    EffectOperation.Set => effect.Value,
                    _ => effect.Value
                };

                ApplyEffect(effect, magnitude, ref stats, baseStats);
            }
        }
    }

    private void ApplyEffect(StatEffect effect, float magnitude, ref FinalStats stats, UnitStatsDefinition baseStats)
    {
        float baseValue = GetBaseStat(effect.Target, baseStats);

        if (effect.Operation == EffectOperation.Set)
        {
            SetStat(effect.Target, magnitude, ref stats);
            return;
        }

        float delta = CalculateDelta(baseValue, effect.Operation, magnitude);

        AddToStat(effect.Target, delta, ref stats);
    }

    private void AddToStat(StatType target, float delta, ref FinalStats stats)
    {
        switch (target)
        {
            case StatType.Health:
                stats.maxHealth += Mathf.RoundToInt(delta);
                break;

            case StatType.AttackDamage:
                stats.attackDamage += Mathf.RoundToInt(delta);
                break;

            case StatType.AttackSpeed:
                stats.attackSpeed += delta;
                break;

            case StatType.AttackRange:
                stats.attackRange += delta;
                break;

            case StatType.Armor:
                stats.armor += Mathf.RoundToInt(delta);
                break;

            case StatType.CritChance:
                stats.critChance += delta;
                break;

            case StatType.CritDamage:
                stats.critDamage += delta;
                break;
        }
    }

    private void SetStat(StatType target, float value, ref FinalStats stats)
    {
        switch (target)
        {
            case StatType.Health:
                stats.maxHealth = Mathf.RoundToInt(value);
                break;

            case StatType.AttackDamage:
                stats.attackDamage = Mathf.RoundToInt(value);
                break;

            case StatType.AttackSpeed:
                stats.attackSpeed = value;
                break;

            case StatType.AttackRange:
                stats.attackRange = value;
                break;

            case StatType.Armor:
                stats.armor = Mathf.RoundToInt(value);
                break;

            case StatType.CritChance:
                stats.critChance = value;
                break;

            case StatType.CritDamage:
                stats.critDamage = value;
                break;
        }
    }

    private float CalculateDelta(float baseValue, EffectOperation operation, float magnitude)
    {
        return operation switch
        {
            EffectOperation.Add => magnitude,
            EffectOperation.Subtract => -magnitude,
            EffectOperation.Multiply => (baseValue * magnitude) - baseValue,
            _ => throw new ArgumentOutOfRangeException("No operation found in CalculateDelta")
        };
    }

    private float GetBaseStat(StatType target, UnitStatsDefinition baseStats)
    {
        return target switch
        {
            StatType.Health => baseStats.maxHealth,
            StatType.AttackDamage => baseStats.attackDamage,
            StatType.AttackSpeed => baseStats.attackSpeed,
            StatType.AttackRange => baseStats.attackRange,
            StatType.Armor => baseStats.armor,
            StatType.CritChance => baseStats.critChance,
            StatType.CritDamage => baseStats.critDamage,
            _ => throw new ArgumentOutOfRangeException("No EffectTarget found in GetBaseStat")
        };
    }

    public FinalStats CalculateEnemyStats(int waveIndex, FinalStats finalStats, bool isRanged = false)
    {
        int wave = waveIndex + 1;
        int hpPercent = Mathf.RoundToInt(2f * Mathf.Pow(wave, 0.85f));
        int dmgPercent = Mathf.RoundToInt(2f * Mathf.Pow(wave, 0.75f));

        float hpMultiplier = 1f + (hpPercent * 0.01f);
        float dmgMultiplier = 1f + (dmgPercent * 0.01f);

        ApplyEnemyEffect(StatType.Health, EffectOperation.Multiply, hpMultiplier, ref finalStats);
        ApplyEnemyEffect(StatType.AttackDamage, EffectOperation.Multiply, dmgMultiplier, ref finalStats);

        if (isRanged)
        {
            finalStats.attackRange *= EnemyRangedRangeMultiplier;
        }

        return finalStats;
    }

    private void ApplyEnemyEffect(StatType target, EffectOperation operation,
        float value, ref FinalStats stats)
    {
        float current = stats.Stats.Get(target);

        float result = operation switch
        {
            EffectOperation.Add => current + value,
            EffectOperation.Subtract => current - value,
            EffectOperation.Multiply => current * value,
            EffectOperation.Set => value,
            _ => current
        };

        stats.Stats.Set(target, result);
    }
}
