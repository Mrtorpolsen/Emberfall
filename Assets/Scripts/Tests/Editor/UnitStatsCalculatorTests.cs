using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class UnitStatsCalculatorTests
{
    [Test]
    public void ApplyTalents_AdditiveCritTalent_StacksCorrectly()
    {
        var calculator = new UnitStatsCalculator();

        FinalStats stats = new FinalStats
        {
            critChance = 0f
        };

        var talents = new List<AppliedTalent>
        {
            new AppliedTalent
            {
                Purchased = 3,
                Effects = new List<TalentEffect>
                {
                    new TalentEffect
                    {
                        Target = EffectTarget.CritChance,
                        Operation = EffectOperation.Add,
                        Value = 0.1f
                    }
                }
            }
        };

        calculator.ApplyTalents(ref stats, talents);
        // 0 + 0.1 + 0.1 + 0.1
        Assert.AreEqual(0.3f, stats.critChance);
    }

    [Test]
    public void ApplyTalents_MultiplicativeHealthTalent_StacksCorrectly()
    {
        var calculator = new UnitStatsCalculator();

        FinalStats stats = new FinalStats
        {
            health = 200
        };

        var talents = new List<AppliedTalent>
        {
            new AppliedTalent
            {
                Purchased = 5,
                Effects = new List<TalentEffect>
                {
                    new TalentEffect
                    {
                        Target = EffectTarget.Health,
                        Operation = EffectOperation.Multiply,
                        Value = 1.02f
                    }
                }
            },

            new AppliedTalent
            {
                Purchased = 3,
                Effects = new List<TalentEffect>
                {
                    new TalentEffect
                    {
                        Target = EffectTarget.Health,
                        Operation = EffectOperation.Multiply,
                        Value = 1.03f
                    }
                }
            }
        };

        calculator.ApplyTalents(ref stats, talents);

        // 200 * (1.02^5) * (1.03^3)
        Assert.AreEqual(
            Mathf.RoundToInt(200 * Mathf.Pow(1.02f, 5) * Mathf.Pow(1.03f, 3)),
            stats.health
        );
    }

    [Test]
    public void ApplyTalents_MultiplyAttackDamageTalent_StacksCorrectly()
    {
        var calculator = new UnitStatsCalculator();

        FinalStats stats = new FinalStats
        {
            attackDamage = 20
        };

        var talents = new List<AppliedTalent>
        {
            new AppliedTalent
            {
                Purchased = 5,
                Effects = new List<TalentEffect>
                {
                    new TalentEffect
                    {
                        Target = EffectTarget.AttackDamage,
                        Operation = EffectOperation.Multiply,
                        Value = 1.05f
                    }
                }
            }
        };

        calculator.ApplyTalents(ref stats, talents);

        // 20 * (1,05^5)
        Assert.AreEqual(
            Mathf.RoundToInt(20 * Mathf.Pow(1.05f, 5)),
            stats.attackDamage
        );
    }

    [Test]
    public void CalculateEnemyStats_Wave0_AppliesCorrectScaling()
    {
        //zero based
        int waveIndex = 0;

        var calculator = new UnitStatsCalculator();

        var baseStats = new FinalStats
        {
            health = 100,
            attackDamage = 10,
            attackSpeed = 1f,  // should remain unchanged
            critChance = 0.05f // should remain unchanged
        };

        var result = calculator.CalculateEnemyStats(waveIndex, baseStats);

        float actualWave = waveIndex + 1;

        // Expected HP multiplier
        float expectedHpMultiplier = 1f + Mathf.RoundToInt(2f * Mathf.Pow(actualWave, 0.85f)) * 0.01f;
        float expectedDmgMultiplier = 1f + Mathf.RoundToInt(2f * Mathf.Pow(actualWave, 0.75f)) * 0.01f;

        Assert.AreEqual(Mathf.RoundToInt(100 * expectedHpMultiplier), result.health);
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
            health = 500,
            attackDamage = 50,
        };

        var result = calculator.CalculateEnemyStats(waveIndex, baseStats);

        float actualWave = waveIndex + 1;

        float expectedHpMultiplier = 1f + Mathf.RoundToInt(2f * Mathf.Pow(actualWave, 0.85f)) * 0.01f;
        float expectedDmgMultiplier = 1f + Mathf.RoundToInt(2f * Mathf.Pow(actualWave, 0.75f)) * 0.01f;

        Assert.AreEqual(Mathf.RoundToInt(500 * expectedHpMultiplier), result.health);
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

    [Test]
    public void ApplyTalents_FloatAttackRange_IsNotRounded()
    {
        var calculator = new UnitStatsCalculator();

        FinalStats stats = new FinalStats
        {
            attackRange = 0.2f
        };

        var talents = new List<AppliedTalent>
    {
        new AppliedTalent
        {
            Purchased = 5,
            Effects = new List<TalentEffect>
            {
                new TalentEffect
                {
                    Target = EffectTarget.AttackRange,
                    Operation = EffectOperation.Add,
                    Value = 0.15f
                }
            }
        }
    };

        calculator.ApplyTalents(ref stats, talents);

        // 0.2 + (0.15 * 5) = 0.95f
        Assert.AreEqual(0.95f, stats.attackRange, 0.0001f);
    }

    [Test]
    public void ApplyTalents_Health_IsRounded()
    {
        var calculator = new UnitStatsCalculator();

        FinalStats stats = new FinalStats
        {
            health = 100
        };

        var talents = new List<AppliedTalent>
    {
        new AppliedTalent
        {
            Purchased = 3,
            Effects = new List<TalentEffect>
            {
                new TalentEffect
                {
                    Target = EffectTarget.Health,
                    Operation = EffectOperation.Multiply,
                    Value = 1.07f
                }
            }
        }
    };

        calculator.ApplyTalents(ref stats, talents);

        // 100 * (1.07^3) = 122.5043 rounded to 123
        Assert.AreEqual(Mathf.RoundToInt(100 * Mathf.Pow(1.07f, 3)), stats.health);
    }

    [Test]
    public void ApplyTalents_NoTalents_PlayerStatsRemainUnchanged()
    {
        var calculator = new UnitStatsCalculator();

        var stats = new FinalStats
        {
            health = 100,
            attackDamage = 20,
            critChance = 0.05f,
            attackSpeed = 1f
        };

        // Simulate empty save: no purchased talents
        var playerTalents = new List<AppliedTalent>();

        calculator.ApplyTalents(ref stats, playerTalents);

        Assert.AreEqual(100, stats.health);
        Assert.AreEqual(20, stats.attackDamage);
        Assert.AreEqual(0.05f, stats.critChance, 0.0001f);
        Assert.AreEqual(1f, stats.attackSpeed);
    }
}
