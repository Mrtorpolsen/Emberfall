using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class UnitStatsCalculatorTests
{
    private static StatBlock Block(params (StatType stat, float value)[] values)
    {
        var block = new StatBlock();
        foreach (var (stat, value) in values)
        {
            block.Set(stat, value);
        }
        return block;
    }

    [Test]
    public void ApplyModifier_AdditiveCritTalent_StacksCorrectly()
    {
        var calculator = new UnitStatsCalculator();

        StatBlock stats = Block((StatType.CritChance, 0f));

        UnitStatsDefinition baseStats = ScriptableObject.CreateInstance<UnitStatsDefinition>();
        baseStats.critChance = 0f;

        var talents = new List<AppliedStatModifier>
        {
            new AppliedStatModifier
            {
                Stacks = 3,
                Effects = new List<StatEffect>
                {
                    new StatEffect
                    {
                        Target = StatType.CritChance,
                        Operation = EffectOperation.Add,
                        Value = 0.1f
                    }
                }
            }
        };

        calculator.ApplyModifiers(stats, baseStats, talents);
        // 0 + 0.1 + 0.1 + 0.1
        Assert.AreEqual(0.3f, stats.Get(StatType.CritChance));
    }

    [Test]
    public void ApplyModifier_AdditiveHealthTalent_StacksCorrectly()
    {
        var calculator = new UnitStatsCalculator();

        StatBlock stats = Block((StatType.Health, 200));

        UnitStatsDefinition baseStats = ScriptableObject.CreateInstance<UnitStatsDefinition>();
        baseStats.maxHealth = 200;

        var talents = new List<AppliedStatModifier>
        {
            new AppliedStatModifier
            {
                Stacks = 5,
                Effects = new List<StatEffect>
                {
                    new StatEffect
                    {
                        Target = StatType.Health,
                        Operation = EffectOperation.Multiply,
                        Value = 1.02f
                    }
                }
            },

            new AppliedStatModifier
            {
                Stacks = 3,
                Effects = new List<StatEffect>
                {
                    new StatEffect
                    {
                        Target = StatType.Health,
                        Operation = EffectOperation.Multiply,
                        Value = 1.03f
                    }
                }
            }
        };

        calculator.ApplyModifiers(stats, baseStats, talents);

        // delta 1 = 1 + (1.02 - 1) * 5 = 1.10
        // 200 * 1.1 - 200 = 20

        // delta 2 = 1 + (1.03 - 1) * 3 = 1.09
        // 200 * 1.09 - 200 = 18

        // 200 + 18 + 20 = 238
        Assert.AreEqual(
            Mathf.RoundToInt(
                200 +
                (200 * (1 + (1.02f - 1) * 5) - 200) +   // first delta
                (200 * (1 + (1.03f - 1) * 3) - 200)     // second delta
            ),
            stats.GetInt(StatType.Health)
        );
    }

    [Test]
    public void ApplyModifier_MultiplyAttackDamageTalent_StacksCorrectly()
    {
        var calculator = new UnitStatsCalculator();

        StatBlock stats = Block((StatType.AttackDamage, 20));

        UnitStatsDefinition baseStats = ScriptableObject.CreateInstance<UnitStatsDefinition>();
        baseStats.attackDamage = 20;

        var talents = new List<AppliedStatModifier>
        {
            new AppliedStatModifier
            {
                Stacks = 5,
                Effects = new List<StatEffect>
                {
                    new StatEffect
                    {
                        Target = StatType.AttackDamage,
                        Operation = EffectOperation.Multiply,
                        Value = 1.05f
                    }
                }
            }
        };

        calculator.ApplyModifiers(stats, baseStats, talents);

        // magnitude = 1 + (1.05 - 1) * 5 = 1.25
        // 20 * 1.25 = 25
        Assert.AreEqual(
            Mathf.RoundToInt(20 * (1 + (1.05f - 1) * 5)),
            stats.GetInt(StatType.AttackDamage)
        );
    }

    [Test]
    public void ApplyModifier_FloatAttackRange_IsNotRounded()
    {
        var calculator = new UnitStatsCalculator();

        StatBlock stats = Block((StatType.AttackRange, 0.2f));

        UnitStatsDefinition baseStats = ScriptableObject.CreateInstance<UnitStatsDefinition>();
        baseStats.attackRange = 0.2f;

        var talents = new List<AppliedStatModifier>
    {
        new AppliedStatModifier
        {
            Stacks = 5,
            Effects = new List<StatEffect>
            {
                new StatEffect
                {
                    Target = StatType.AttackRange,
                    Operation = EffectOperation.Add,
                    Value = 0.15f
                }
            }
        }
    };

        calculator.ApplyModifiers(stats, baseStats, talents);

        // 0.2 + (0.15 * 5) = 0.95f
        Assert.AreEqual(0.95f, stats.Get(StatType.AttackRange), 0.0001f);
    }

    [Test]
    public void ApplyModifier_Health_IsRounded()
    {
        var calculator = new UnitStatsCalculator();

        StatBlock stats = Block((StatType.Health, 100));

        UnitStatsDefinition baseStats = ScriptableObject.CreateInstance<UnitStatsDefinition>();
        baseStats.maxHealth = 100;

        var talents = new List<AppliedStatModifier>
    {
        new AppliedStatModifier
        {
            Stacks = 3,
            Effects = new List<StatEffect>
            {
                new StatEffect
                {
                    Target = StatType.Health,
                    Operation = EffectOperation.Multiply,
                    Value = 1.075f
                }
            }
        }
    };

        calculator.ApplyModifiers(stats, baseStats, talents);

        // magnitude = 1 + (1.075 - 1) * 3 = 1.225
        // 100 * 1.225 = 122.5 rounded to 123
        Assert.AreEqual(Mathf.RoundToInt(100 * (1 + (1.075f - 1) * 3)), stats.GetInt(StatType.Health));
    }

    [Test]
    public void ApplyModifier_Modifiers_PlayerStatsRemainUnchanged()
    {
        var calculator = new UnitStatsCalculator();

        var stats = Block((StatType.Health, 100), (StatType.AttackDamage, 20), (StatType.CritChance, 0.05f), (StatType.AttackSpeed, 1f));

        UnitStatsDefinition baseStats = ScriptableObject.CreateInstance<UnitStatsDefinition>();
        baseStats.maxHealth = 100;
        baseStats.attackDamage = 20;
        baseStats.critChance = 0.05f;
        baseStats.attackSpeed = 1f;

        // Simulate empty save: no purchased talents
        var playerTalents = new List<AppliedStatModifier>();

        calculator.ApplyModifiers(stats, baseStats, playerTalents);

        Assert.AreEqual(100, stats.GetInt(StatType.Health));
        Assert.AreEqual(20, stats.GetInt(StatType.AttackDamage));
        Assert.AreEqual(0.05f, stats.Get(StatType.CritChance), 0.0001f);
        Assert.AreEqual(1f, stats.Get(StatType.AttackSpeed));
    }



    [Test]
    public void CalculateEnemyStats_Wave0_AppliesCorrectScaling()
    {
        //zero based
        int waveIndex = 0;

        var calculator = new UnitStatsCalculator();

        var baseStats = Block((StatType.Health, 100), (StatType.AttackDamage, 10), (StatType.AttackSpeed, 1f), (StatType.CritChance, 0.05f));

        var result = calculator.CalculateEnemyStats(waveIndex, baseStats);

        float actualWave = waveIndex + 1;

        // Expected HP multiplier
        float expectedHpMultiplier = 1f + Mathf.RoundToInt(2f * Mathf.Pow(actualWave, 0.85f)) * 0.01f;
        float expectedDmgMultiplier = 1f + Mathf.RoundToInt(2f * Mathf.Pow(actualWave, 0.75f)) * 0.01f;

        Assert.AreEqual(Mathf.RoundToInt(100 * expectedHpMultiplier), result.GetInt(StatType.Health));
        Assert.AreEqual(Mathf.RoundToInt(10 * expectedDmgMultiplier), result.GetInt(StatType.AttackDamage));
    }

    [Test]
    public void CalculateEnemyStats_Wave20_AppliesCorrectScaling()
    {
        //zero based
        int waveIndex = 19;

        var calculator = new UnitStatsCalculator();

        var baseStats = Block((StatType.Health, 500), (StatType.AttackDamage, 50));

        var result = calculator.CalculateEnemyStats(waveIndex, baseStats);

        float actualWave = waveIndex + 1;

        float expectedHpMultiplier = 1f + Mathf.RoundToInt(2f * Mathf.Pow(actualWave, 0.85f)) * 0.01f;
        float expectedDmgMultiplier = 1f + Mathf.RoundToInt(2f * Mathf.Pow(actualWave, 0.75f)) * 0.01f;

        Assert.AreEqual(Mathf.RoundToInt(500 * expectedHpMultiplier), result.GetInt(StatType.Health));
        Assert.AreEqual(Mathf.RoundToInt(50 * expectedDmgMultiplier), result.GetInt(StatType.AttackDamage));
    }

    [Test]
    public void CalculateEnemyStats_DoesNotAffectOtherStats()
    {
        var calculator = new UnitStatsCalculator();

        var baseStats = Block((StatType.AttackSpeed, 0.7f), (StatType.CritChance, 0.05f), (StatType.AttackRange, 2f), (StatType.Armor, 5));

        var result = calculator.CalculateEnemyStats(10, baseStats);

        Assert.AreEqual(0.7f, result.Get(StatType.AttackSpeed));
        Assert.AreEqual(0.05f, result.Get(StatType.CritChance), 0.0001f);
        Assert.AreEqual(2f, result.Get(StatType.AttackRange));
        Assert.AreEqual(5, result.GetInt(StatType.Armor));
    }

    [Test]
    public void ApplyModifier_SubtractTalent_ReducesStat()
    {
        var calculator = new UnitStatsCalculator();

        StatBlock stats = Block((StatType.Armor, 10));

        UnitStatsDefinition baseStats = ScriptableObject.CreateInstance<UnitStatsDefinition>();
        baseStats.armor = 10;

        var talents = new List<AppliedStatModifier>
        {
            new AppliedStatModifier
            {
                Stacks = 2,
                Effects = new List<StatEffect>
                {
                    new StatEffect
                    {
                        Target = StatType.Armor,
                        Operation = EffectOperation.Subtract,
                        Value = 3f
                    }
                }
            }
        };

        calculator.ApplyModifiers(stats, baseStats, talents);
        // 10 - 3 - 3
        Assert.AreEqual(4, stats.GetInt(StatType.Armor));
    }

    [Test]
    public void CalculateEnemyStats_Ranged_ReducesAttackRange()
    {
        var calculator = new UnitStatsCalculator();

        var baseStats = Block((StatType.AttackRange, 5f));

        var result = calculator.CalculateEnemyStats(0, baseStats, isRanged: true);

        Assert.AreEqual(4f, result.Get(StatType.AttackRange), 0.0001f);
    }

    [Test]
    public void CalculateEnemyStats_Melee_KeepsAttackRange()
    {
        var calculator = new UnitStatsCalculator();

        var baseStats = Block((StatType.AttackRange, 0.25f));

        var result = calculator.CalculateEnemyStats(0, baseStats);

        Assert.AreEqual(0.25f, result.Get(StatType.AttackRange), 0.0001f);
    }

    [Test]
    public void ApplyModifier_MovementSpeedTalent_IsApplied()
    {
        var calculator = new UnitStatsCalculator();

        StatBlock stats = Block((StatType.MovementSpeed, 2f));

        UnitStatsDefinition baseStats = ScriptableObject.CreateInstance<UnitStatsDefinition>();
        baseStats.movementSpeed = 2f;

        var talents = new List<AppliedStatModifier>
        {
            new AppliedStatModifier
            {
                Stacks = 2,
                Effects = new List<StatEffect>
                {
                    new StatEffect
                    {
                        Target = StatType.MovementSpeed,
                        Operation = EffectOperation.Multiply,
                        Value = 1.05f
                    }
                }
            }
        };

        calculator.ApplyModifiers(stats, baseStats, talents);
        // 2 * (1 + 0.05 * 2)
        Assert.AreEqual(2.2f, stats.Get(StatType.MovementSpeed), 0.0001f);
    }
}
