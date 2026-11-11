using System.Collections.Generic;
using NUnit.Framework;

public class ResearchRulesTests
{
    private const long Now = 1_700_000_000;

    private ResearchTree tree;
    private PlayerResearchState state;

    [SetUp]
    public void SetUp()
    {
        tree = new ResearchTree
        {
            ScalingPresets = new Dictionary<string, ScalingPreset>
            {
                ["double"] = new ScalingPreset
                {
                    Stages = new[]
                    {
                        new ScalingStage { StageNumber = 1, MinLevel = 1, MaxLevel = 3, MultiplierPerLevel = 2f },
                        new ScalingStage { StageNumber = 2, MinLevel = 4, MaxLevel = 10, MultiplierPerLevel = 1.5f },
                    }
                }
            },
            ResearchByCategory = new Dictionary<string, List<ResearchDefinition>>
            {
                ["unit"] = new List<ResearchDefinition>
                {
                    Research("unit_health", ResearchCategory.Unit, maxLevel: 5),
                    Research("unit_damage", ResearchCategory.Unit, maxLevel: 5,
                        new ResearchPrerequisite { RequiredResearchId = "tower_range", RequiredLevel = 2 }),
                },
                ["tower"] = new List<ResearchDefinition>
                {
                    Research("tower_range", ResearchCategory.Tower, maxLevel: 3),
                },
                ["castle_from_a_newer_client"] = new List<ResearchDefinition>(),
            }
        };

        tree.NormalizeCategoryKeys();
        tree.ApplyScalingPresets();
        state = new PlayerResearchState();
    }

    private static ResearchDefinition Research(string id, ResearchCategory category, int maxLevel, params ResearchPrerequisite[] prerequisites) =>
        new ResearchDefinition
        {
            Id = id,
            Name = id,
            Category = category,
            MaxLevel = maxLevel,
            CostScaling = new ResearchScaling { BaseValue = 100, Preset = "double" },
            TimeScaling = new ResearchScaling { BaseValue = 60, Preset = "double" },
            Prerequisites = prerequisites
        };

    private ResearchStartCheck Check(string id, int cinders = 100_000) => ResearchRules.CanStart(tree, state, id, cinders);

    [Test]
    public void ScalingSkipsLevelOneAndMultipliesPerLevelAndStage()
    {
        var research = tree.GetResearchById("unit_health");

        Assert.That(ResearchRules.GetCost(research, 1), Is.EqualTo(100));
        Assert.That(ResearchRules.GetCost(research, 3), Is.EqualTo(400));       // 100 * 2 * 2
        Assert.That(ResearchRules.GetCost(research, 5), Is.EqualTo(900));       // 400 * 1.5 * 1.5
        Assert.That(ResearchRules.GetDurationSeconds(research, 2), Is.EqualTo(120));
    }

    [Test]
    public void UnknownCategoriesAreSkipped()
    {
        Assert.That(tree.GetCategories(), Is.EquivalentTo(new[] { ResearchCategory.Unit, ResearchCategory.Tower }));
    }

    [Test]
    public void MissingPresetIsAnError()
    {
        tree.GetResearchById("unit_health").CostScaling.Preset = "missing";

        Assert.That(() => tree.ApplyScalingPresets(), Throws.InvalidOperationException);
    }

    [Test]
    public void CanStartWithExactlyEnoughCinders()
    {
        Assert.That(Check("unit_health", 100), Is.EqualTo(ResearchStartCheck.Ok));
        Assert.That(Check("unit_health", 99), Is.EqualTo(ResearchStartCheck.NotEnoughCurrency));
    }

    [Test]
    public void UnknownResearchIsRejected()
    {
        Assert.That(Check("unit_speed"), Is.EqualTo(ResearchStartCheck.UnknownResearch));
    }

    [Test]
    public void OneResearchPerCategory()
    {
        ResearchRules.Start(tree, state, "unit_health", Now);

        Assert.That(Check("unit_health"), Is.EqualTo(ResearchStartCheck.CategoryBusy));
        Assert.That(Check("tower_range"), Is.EqualTo(ResearchStartCheck.Ok));
    }

    [Test]
    public void MaxLevelCantBeResearched()
    {
        state.CompletedResearch["tower_range"] = 3;

        Assert.That(Check("tower_range"), Is.EqualTo(ResearchStartCheck.MaxedOut));
    }

    [Test]
    public void PrerequisiteNeedsTheRequiredLevel()
    {
        state.CompletedResearch["tower_range"] = 1;
        Assert.That(Check("unit_damage"), Is.EqualTo(ResearchStartCheck.PrerequisitesNotMet));

        state.CompletedResearch["tower_range"] = 2;
        Assert.That(Check("unit_damage"), Is.EqualTo(ResearchStartCheck.Ok));
    }

    [Test]
    public void StartTargetsTheNextLevelAtTheGivenTime()
    {
        state.CompletedResearch["unit_health"] = 1;

        var active = ResearchRules.Start(tree, state, "unit_health", Now);

        Assert.That(active.TargetLevel, Is.EqualTo(2));
        Assert.That(active.StartTime, Is.EqualTo(Now));
        Assert.That(state.ActiveResearch, Does.Contain(active));
    }

    [Test]
    public void IsDoneOnceTheDurationHasPassed()
    {
        var active = ResearchRules.Start(tree, state, "unit_health", Now);   // level 1: 60 s

        Assert.That(ResearchRules.IsDone(tree, active, Now + 59), Is.False);
        Assert.That(ResearchRules.IsDone(tree, active, Now + 60), Is.True);
    }

    [Test]
    public void CompleteSetsTheLevelAndFreesTheCategory()
    {
        var active = ResearchRules.Start(tree, state, "unit_health", Now);

        ResearchRules.Complete(state, active);

        Assert.That(ResearchRules.GetLevel(state, "unit_health"), Is.EqualTo(1));
        Assert.That(ResearchRules.GetActiveInCategory(state, ResearchCategory.Unit), Is.Null);
    }
}
