using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using static StatsBootstrapper;

public class UnitStatsManagerTests
{
    [Test]
    public void GetResearchStatModifiers_GetResearch_GetsAll()
    {
        var go = new GameObject("UnitStatsManager");
        var manager = go.AddComponent<UnitStatsManager>();

        manager.StatsBootstrapper = new StatsBootstrapperMock();

        var result = manager.GetResearchStatModifiers();

        Assert.IsTrue(result.ContainsKey(ResearchCategory.Unit));
        Assert.IsTrue(result.ContainsKey(ResearchCategory.Tower));

        // Check number of research in each category
        Assert.AreEqual(2, result[ResearchCategory.Unit].Count);
        Assert.AreEqual(1, result[ResearchCategory.Tower].Count);

        // Check individual research
        var unitResearch1 = result[ResearchCategory.Unit][0];
        Assert.AreEqual(95, unitResearch1.Stacks);
        Assert.AreEqual(EffectTarget.AttackDamage, unitResearch1.Effects[0].Target);
        Assert.AreEqual(EffectOperation.Multiply, unitResearch1.Effects[0].Operation);
        Assert.AreEqual(1.02f, unitResearch1.Effects[0].Value);

        var unitResearch2 = result[ResearchCategory.Unit][1];
        Assert.AreEqual(10, unitResearch2.Stacks);
        Assert.AreEqual(EffectTarget.Health, unitResearch2.Effects[0].Target);
        Assert.AreEqual(EffectOperation.Add, unitResearch2.Effects[0].Operation);
        Assert.AreEqual(5, unitResearch2.Effects[0].Value);

        var towerResearch = result[ResearchCategory.Tower][0];
        Assert.AreEqual(35, towerResearch.Stacks);
        Assert.AreEqual(EffectTarget.AttackDamage, towerResearch.Effects[0].Target);
        Assert.AreEqual(EffectOperation.Multiply, towerResearch.Effects[0].Operation);
        Assert.AreEqual(1.02f, towerResearch.Effects[0].Value);
    }

    [Test]
    public void CalculateAllFinalStats_ApplyToAllInCategory_AppliedToAll()
    {
        var go = new GameObject("UnitStatsManager");
        var manager = go.AddComponent<UnitStatsManager>();

        manager.StatsBootstrapper = new StatsBootstrapperMock();
        manager.UnitStatsCalculator = new UnitStatsCalculator();

        manager.StatsBootstrapper.ClearTalents();

        var fighter = ScriptableObject.CreateInstance<UnitStatsDefinition>();
        fighter.name = "fighter";
        fighter.maxHealth = 200;
        fighter.attackDamage = 20;
        fighter.category = ResearchCategory.Unit;

        manager.UnitStatsByUnitKey["fighter"] = fighter;

        var ranger = ScriptableObject.CreateInstance<UnitStatsDefinition>();
        ranger.name = "ranger";
        ranger.maxHealth = 100;
        ranger.attackDamage = 15;
        ranger.category = ResearchCategory.Unit;

        manager.UnitStatsByUnitKey["ranger"] = ranger;

        var tower = ScriptableObject.CreateInstance<UnitStatsDefinition>();
        tower.name = "tower";
        tower.maxHealth = 100;
        tower.attackDamage = 45;
        tower.category = ResearchCategory.Tower;

        manager.UnitStatsByUnitKey["tower"] = tower;

        manager.CalculateAllFinalStats();

        var fighterStats = manager.FinalStatsByUnit["fighter"];
        var rangerStats = manager.FinalStatsByUnit["ranger"];
        var towerStats = manager.FinalStatsByUnit["tower"];

        // magnitude formula = 1 + ((effect value) - 1) * (stacks)
        // base stat value * magnitude
        // 20 * (1 + 0.02*95) = 20 * 2.9 = 58
        Assert.AreEqual(
            Mathf.RoundToInt(20 * (1 + (1.02f - 1) * 95)),
            fighterStats.attackDamage
        );

        // base stat value + ((stacks) * (effect value))
        // 200 + 50 = 250
        Assert.AreEqual(200 + (10 * 5), fighterStats.maxHealth);

        // 15 * 2.9 = 43
        Assert.AreEqual(
            Mathf.RoundToInt(15 * (1 + (1.02f - 1) * 95)),
            rangerStats.attackDamage
        );
        // 100 + 50 = 150
        Assert.AreEqual(100 + (10 * 5), rangerStats.maxHealth);

        // 45 * (1 + 0.02*35) = 45 * 1.7 = 77
        Assert.AreEqual(
            Mathf.RoundToInt(45 * (1 + (1.02f - 1) * 35)),
            towerStats.attackDamage
        );
    }

    private class StatsBootstrapperMock : StatsBootstrapper
    {
        public StatsBootstrapperMock()
        {
            // Initialize internal dictionaries
            var talentsByUnit = new Dictionary<string, List<TalentsToApply>>
            {
                { 
                    "fighter", new List<TalentsToApply>
                    {
                        new TalentsToApply
                        {
                            purchased = 3,
                            effects = new List<AppliedEffect>
                            {
                                new AppliedEffect
                                {
                                    Target = EffectTarget.Health,
                                    Operation = EffectOperation.Multiply,
                                    Value = 1.05f
                                }
                            }
                        },
                        new TalentsToApply
                        {
                            purchased = 2,
                            effects = new List<AppliedEffect>
                            {
                                new AppliedEffect
                                {
                                    Target = EffectTarget.AttackDamage,
                                    Operation = EffectOperation.Multiply,
                                    Value = 1.05f
                                }
                            }
                        }
                    }
                },
                { 
                    "ranger", new List<TalentsToApply>
                    {
                        new TalentsToApply
                        {
                            purchased = 2,
                            effects = new List<AppliedEffect>
                            {
                                new AppliedEffect
                                {
                                    Target = EffectTarget.AttackDamage,
                                    Operation = EffectOperation.Multiply,
                                    Value = 1.05f
                                }
                            }
                        }
                    }
                }
            };

            var researchByCategory = new Dictionary<ResearchCategory, List<ResearchToApply>>
            {
                { 
                    ResearchCategory.Unit, new List<ResearchToApply>
                    {
                        new ResearchToApply
                        {
                            purchased = 95,
                            effects = new List<AppliedEffect>
                            {
                                new AppliedEffect
                                {
                                    Target = EffectTarget.AttackDamage,
                                    Operation = EffectOperation.Multiply,
                                    Value = 1.02f
                                }
                            }
                        },
                        new ResearchToApply
                        {
                            purchased = 10,
                            effects = new List<AppliedEffect>
                            {
                                new AppliedEffect
                                {
                                    Target = EffectTarget.Health,
                                    Operation = EffectOperation.Add,
                                    Value = 5
                                }
                            }
                        }
                    }

                },
                { 
                    ResearchCategory.Tower, new List<ResearchToApply>
                    {
                        new ResearchToApply
                        {
                            purchased = 35,
                            effects = new List<AppliedEffect>
                            {
                                new AppliedEffect
                                {
                                    Target = EffectTarget.AttackDamage,
                                    Operation = EffectOperation.Multiply,
                                    Value = 1.02f
                                }
                            }
                        }
                    }
                }
            };

            // Using reflection to set the private fields, since they are private in StatsBootstrapper
            var talentsField = typeof(StatsBootstrapper).GetField("talentsByUnit", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var researchField = typeof(StatsBootstrapper).GetField("researchByCategory", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            talentsField.SetValue(this, talentsByUnit);
            researchField.SetValue(this, researchByCategory);
        }
    }

}
