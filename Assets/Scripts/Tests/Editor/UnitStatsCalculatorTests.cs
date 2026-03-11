using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class UnitStatsCalculatorTests
{
    [Test]
    public void ApplyModifier_AdditiveCritTalent_StacksCorrectly()
    {
        var calculator = new UnitStatsCalculator();

        FinalStats stats = new FinalStats
        {
            critChance = 0f
        };

        UnitStatsDefinition baseStats = ScriptableObject.CreateInstance<UnitStatsDefinition>();
        baseStats.critChance = 0f;

        var talents = new List<AppliedStatModifier>
        {
            new AppliedStatModifier
            {
                Stacks = 3,
                Effects = new List<AppliedEffect>
                {
                    new AppliedEffect
                    {
                        Target = EffectTarget.CritChance,
                        Operation = EffectOperation.Add,
                        Value = 0.1f
                    }
                }
            }
        };

        calculator.ApplyModifiers(ref stats, baseStats, talents);
        // 0 + 0.1 + 0.1 + 0.1
        Assert.AreEqual(0.3f, stats.critChance);
    }

    [Test]
    public void ApplyModifier_AdditiveHealthTalent_StacksCorrectly()
    {
        var calculator = new UnitStatsCalculator();

        FinalStats stats = new FinalStats
        {
            maxHealth = 200
        };

        UnitStatsDefinition baseStats = ScriptableObject.CreateInstance<UnitStatsDefinition>();
        baseStats.maxHealth = 200;

        var talents = new List<AppliedStatModifier>
        {
            new AppliedStatModifier
            {
                Stacks = 5,
                Effects = new List<AppliedEffect>
                {
                    new AppliedEffect
                    {
                        Target = EffectTarget.Health,
                        Operation = EffectOperation.Multiply,
                        Value = 1.02f
                    }
                }
            },

            new AppliedStatModifier
            {
                Stacks = 3,
                Effects = new List<AppliedEffect>
                {
                    new AppliedEffect
                    {
                        Target = EffectTarget.Health,
                        Operation = EffectOperation.Multiply,
                        Value = 1.03f
                    }
                }
            }
        };

        calculator.ApplyModifiers(ref stats, baseStats, talents);

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
            stats.maxHealth
        );
    }

    [Test]
    public void ApplyModifier_MultiplyAttackDamageTalent_StacksCorrectly()
    {
        var calculator = new UnitStatsCalculator();

        FinalStats stats = new FinalStats
        {
            attackDamage = 20
        };

        UnitStatsDefinition baseStats = ScriptableObject.CreateInstance<UnitStatsDefinition>();
        baseStats.attackDamage = 20;

        var talents = new List<AppliedStatModifier>
        {
            new AppliedStatModifier
            {
                Stacks = 5,
                Effects = new List<AppliedEffect>
                {
                    new AppliedEffect
                    {
                        Target = EffectTarget.AttackDamage,
                        Operation = EffectOperation.Multiply,
                        Value = 1.05f
                    }
                }
            }
        };

        calculator.ApplyModifiers(ref stats, baseStats, talents);

        // magnitude = 1 + (1.05 - 1) * 5 = 1.25
        // 20 * 1.25 = 25
        Assert.AreEqual(
            Mathf.RoundToInt(20 * (1 + (1.05f - 1) * 5)),
            stats.attackDamage
        );
    }

    [Test]
    public void ApplyModifier_FloatAttackRange_IsNotRounded()
    {
        var calculator = new UnitStatsCalculator();

        FinalStats stats = new FinalStats
        {
            attackRange = 0.2f
        };

        UnitStatsDefinition baseStats = ScriptableObject.CreateInstance<UnitStatsDefinition>();
        baseStats.attackRange = 0.2f;

        var talents = new List<AppliedStatModifier>
    {
        new AppliedStatModifier
        {
            Stacks = 5,
            Effects = new List<AppliedEffect>
            {
                new AppliedEffect
                {
                    Target = EffectTarget.AttackRange,
                    Operation = EffectOperation.Add,
                    Value = 0.15f
                }
            }
        }
    };

        calculator.ApplyModifiers(ref stats, baseStats, talents);

        // 0.2 + (0.15 * 5) = 0.95f
        Assert.AreEqual(0.95f, stats.attackRange, 0.0001f);
    }

    [Test]
    public void ApplyModifier_Health_IsRounded()
    {
        var calculator = new UnitStatsCalculator();

        FinalStats stats = new FinalStats
        {
            maxHealth = 100
        };

        UnitStatsDefinition baseStats = ScriptableObject.CreateInstance<UnitStatsDefinition>();
        baseStats.maxHealth = 100;

        var talents = new List<AppliedStatModifier>
    {
        new AppliedStatModifier
        {
            Stacks = 3,
            Effects = new List<AppliedEffect>
            {
                new AppliedEffect
                {
                    Target = EffectTarget.Health,
                    Operation = EffectOperation.Multiply,
                    Value = 1.075f
                }
            }
        }
    };

        calculator.ApplyModifiers(ref stats, baseStats, talents);

        // magnitude = 1 + (1.075 - 1) * 3 = 1.225
        // 100 * 1.225 = 122.5 rounded to 123
        Assert.AreEqual(Mathf.RoundToInt(100 * (1 + (1.075f - 1) * 3)), stats.maxHealth);
    }

    [Test]
    public void ApplyModifier_Modifiers_PlayerStatsRemainUnchanged()
    {
        var calculator = new UnitStatsCalculator();

        var stats = new FinalStats
        {
            maxHealth = 100,
            attackDamage = 20,
            critChance = 0.05f,
            attackSpeed = 1f
        };

        UnitStatsDefinition baseStats = ScriptableObject.CreateInstance<UnitStatsDefinition>();
        baseStats.maxHealth = 100;
        baseStats.attackDamage = 20;
        baseStats.critChance = 0.05f;
        baseStats.attackSpeed = 1f;

        // Simulate empty save: no purchased talents
        var playerTalents = new List<AppliedStatModifier>();

        calculator.ApplyModifiers(ref stats, baseStats, playerTalents);

        Assert.AreEqual(100, stats.maxHealth);
        Assert.AreEqual(20, stats.attackDamage);
        Assert.AreEqual(0.05f, stats.critChance, 0.0001f);
        Assert.AreEqual(1f, stats.attackSpeed);
    }



    [Test]
    public void CalculateEnemyStats_Wave0_AppliesCorrectScaling()
    {
        //zero based
        int waveIndex = 0;

        var calculator = new UnitStatsCalculator();

        var baseStats = new FinalStats
        {
            maxHealth = 100,
            attackDamage = 10,
            attackSpeed = 1f,  // should remain unchanged
            critChance = 0.05f // should remain unchanged
        };

        var result = calculator.CalculateEnemyStats(waveIndex, baseStats);

        float actualWave = waveIndex + 1;

        // Expected HP multiplier
        float expectedHpMultiplier = 1f + Mathf.RoundToInt(2f * Mathf.Pow(actualWave, 0.85f)) * 0.01f;
        float expectedDmgMultiplier = 1f + Mathf.RoundToInt(2f * Mathf.Pow(actualWave, 0.75f)) * 0.01f;

        Assert.AreEqual(Mathf.RoundToInt(100 * expectedHpMultiplier), result.maxHealth);
        Assert.AreEqual(Mathf.RoundToInt(10 * expectedDmgMultiplier), result.attackDamage);
    }

    [Test]
    public void CalculateEnemyStats_Wave20_AppliesCorrectScaling()
    {
        //zero based
        int waveIndex = 19;

        var calculator = new UnitStatsCalculator();

        var baseStats = new FinalStats
        {
            maxHealth = 500,
            attackDamage = 50,
        };

        var result = calculator.CalculateEnemyStats(waveIndex, baseStats);

        float actualWave = waveIndex + 1;

        float expectedHpMultiplier = 1f + Mathf.RoundToInt(2f * Mathf.Pow(actualWave, 0.85f)) * 0.01f;
        float expectedDmgMultiplier = 1f + Mathf.RoundToInt(2f * Mathf.Pow(actualWave, 0.75f)) * 0.01f;

        Assert.AreEqual(Mathf.RoundToInt(500 * expectedHpMultiplier), result.maxHealth);
        Assert.AreEqual(Mathf.RoundToInt(50 * expectedDmgMultiplier), result.attackDamage);
    }

    [Test]
    public void CalculateEnemyStats_DoesNotAffectOtherStats()
    {
        var calculator = new UnitStatsCalculator();

        var baseStats = new FinalStats
        {
            attackSpeed = 0.7f,
            critChance = 0.05f,
            attackRange = 2f,
            armor = 5
        };

        var result = calculator.CalculateEnemyStats(10, baseStats);

        Assert.AreEqual(0.7f, result.attackSpeed);
        Assert.AreEqual(0.05f, result.critChance, 0.0001f);
        Assert.AreEqual(2f, result.attackRange);
        Assert.AreEqual(5, result.armor);
    }
}
