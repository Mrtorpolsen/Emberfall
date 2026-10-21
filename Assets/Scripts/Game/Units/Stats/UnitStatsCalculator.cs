using System.Collections.Generic;
using UnityEngine;

public class UnitStatsCalculator
{
    // Keeps enemy ranged units from outranging the player's ranged units once scaling kicks in
    private const float EnemyRangedRangeMultiplier = 0.8f;

    // Applies all progression modifiers in one pass so percentages add up against the definition value
    public void ApplyModifiers(ref FinalStats stats, UnitStatsDefinition baseStats, IEnumerable<AppliedStatModifier> modifiers)
    {
        var modifiersByStat = new Dictionary<StatType, List<StatModifier>>();

        foreach (AppliedStatModifier modifier in modifiers)
        {
            foreach (StatEffect effect in modifier.Effects)
            {
                if (!modifiersByStat.TryGetValue(effect.Target, out var list))
                {
                    list = new List<StatModifier>();
                    modifiersByStat[effect.Target] = list;
                }

                list.Add(StatModifier.FromEffect(effect, modifier.Stacks));
            }
        }

        StatBlock baseBlock = baseStats.ToStatBlock();

        foreach (var kvp in modifiersByStat)
        {
            stats.Stats.Set(kvp.Key, StatPipeline.Calculate(baseBlock.Get(kvp.Key), kvp.Value));
        }
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
