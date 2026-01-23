using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class UnitStatsCalculatorTests
{
    [Test]
    public void ApplyTalents_AdditiveHealthTalent_StacksCorrectly()
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
                        Operation = EffectOperation.Add,
                        Value = 10
                    }
                }
            }
        };

        calculator.ApplyTalents(ref stats, talents);

        Assert.AreEqual(130, stats.health);
    }

}
