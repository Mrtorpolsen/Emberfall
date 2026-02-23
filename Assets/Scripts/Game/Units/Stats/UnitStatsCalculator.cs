using System.Collections.Generic;
using UnityEngine;

public class UnitStatsCalculator
{
    public void ApplyModifiers(ref FinalStats stats, IEnumerable<AppliedStatModifier> modifiers)
    {
        foreach (var modifier in modifiers)
        {
            foreach (var effect in modifier.Effects)
            {
                float magnitude = effect.Operation switch
                {
                    EffectOperation.Add => effect.Value * modifier.Stacks,
                    EffectOperation.Multiply => Mathf.Pow(effect.Value, modifier.Stacks),
                    EffectOperation.Set => effect.Value,
                    _ => effect.Value
                };
                ApplyEffect(effect.Target, effect.Operation, magnitude, ref stats);
            }
        }
    }

    public FinalStats CalculateEnemyStats(float waveIndex, FinalStats finalStats)
    {
        float wave = waveIndex + 1;
        int hpPercent = Mathf.RoundToInt(2f * Mathf.Pow(wave, 0.85f));
        int dmgPercent = Mathf.RoundToInt(2f * Mathf.Pow(wave, 0.75f));

        float hpMultiplier = 1f + (hpPercent * 0.01f);
        float dmgMultiplier = 1f + (dmgPercent * 0.01f);

        ApplyEffect(EffectTarget.Health, EffectOperation.Multiply, hpMultiplier, ref finalStats);
        ApplyEffect(EffectTarget.AttackDamage, EffectOperation.Multiply, dmgMultiplier, ref finalStats);

        return finalStats;
    }

    private void ApplyEffect(EffectTarget target, EffectOperation operation,
        float value, ref FinalStats stats)
    {
        switch (target)
        {
            case EffectTarget.Health:
                Apply(ref stats.maxHealth, operation, value);
                break;

            case EffectTarget.AttackDamage:
                Apply(ref stats.attackDamage, operation, value);
                break;

            case EffectTarget.AttackSpeed:
                Apply(ref stats.attackSpeed, operation, value);
                break;

            case EffectTarget.AttackRange:
                Apply(ref stats.attackRange, operation, value);
                break;

            case EffectTarget.Armor:
                Apply(ref stats.armor, operation, value);
                break;

            case EffectTarget.CritChance:
                Apply(ref stats.critChance, operation, value);
                break;

            case EffectTarget.CritDamage:
                Apply(ref stats.critMultiplier, operation, value);
                break;
        }
    }

    private void Apply(ref int stat, EffectOperation operation, float value)
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

    private void Apply(ref float stat, EffectOperation operation, float value)
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
