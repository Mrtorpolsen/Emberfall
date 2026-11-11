using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

public class TalentRulesTests
{
    private const string Unit = "fighter";

    private TalentTree tree;
    private PlayerTalentState state;

    [SetUp]
    public void SetUp()
    {
        tree = new TalentTree
        {
            CostPresets = new Dictionary<CostPreset, TalentCostPreset>
            {
                [CostPreset.Common] = new TalentCostPreset
                {
                    Tiers = new Dictionary<string, TalentCostModel>
                    {
                        ["1"] = new TalentCostModel { BaseCost = 25, CostMultiplier = 1.35f },
                        ["2"] = new TalentCostModel { BaseCost = 40, CostMultiplier = 1.35f },
                    }
                }
            },
            TalentData = new Dictionary<string, TalentData>
            {
                ["stat_health"] = Data(TalentType.StatModifier, 5),
                ["stat_damage"] = Data(TalentType.StatModifier, 5),
                ["unlock_unit_x"] = Data(TalentType.UnitUnlock, 1),
                ["unlock_ability_y"] = Data(TalentType.AbilityUnlock, 1),
                ["no_buy"] = Data(TalentType.StatModifier, 0),
            },
            ArchetypeOverrides = new Dictionary<Archetype, Dictionary<string, TalentOverride>>
            {
                [Archetype.Default] = new[] { "stat_health", "stat_damage", "unlock_unit_x", "unlock_ability_y", "no_buy" }
                    .ToDictionary(id => id, id => new TalentOverride { Name = id, IconId = id })
            },
            UnitDefinitions = new Dictionary<string, UnitDefinition>
            {
                [Unit] = new UnitDefinition
                {
                    CostPreset = CostPreset.Common,
                    Archetype = Archetype.Melee,
                    Talents = new List<TalentNodeData>
                    {
                        Node("stat_health", 1),
                        Node("stat_health", 1),
                        Node("unlock_unit_x", 1),
                        Node("unlock_ability_y", 1),
                        Node("no_buy", 1),
                        Node("stat_health", 2, new TalentPrerequisite { RequiredTier = 1, RequiredPointsInTier = 3 }),
                        Node("stat_damage", 2, new TalentPrerequisite { RequiredUpgradeId = "stat_health_T1" }),
                    }
                }
            }
        };

        tree.BuildTalentsByUnit();
        state = new PlayerTalentState();
    }

    private static TalentData Data(TalentType type, int maxPurchases) =>
        new TalentData { Type = type, Purchase = new TalentPurchaseModel { MaxPurchases = maxPurchases } };

    private static TalentNodeData Node(string definitionId, int tier, params TalentPrerequisite[] prerequisites) =>
        new TalentNodeData { DefinitionId = definitionId, Tier = tier, Prerequisites = prerequisites };

    private void Buy(string talentId, int times = 1)
    {
        for (int i = 0; i < times; i++)
        {
            TalentRules.ApplyPurchase(state, Unit, talentId, 25);
        }
    }

    private TalentPurchaseCheck Check(string talentId, int cinders = 1000) =>
        TalentRules.CanPurchase(tree, state, Unit, talentId, cinders);

    [Test]
    public void BuildsTalentIds()
    {
        var ids = tree.GetTalentsByClass(Unit).Select(t => t.Id);

        Assert.That(ids, Is.EqualTo(new[]
        {
            "stat_health_T1", "stat_health_T1_1", "unlock_unit_x", "unlock_ability_y", "no_buy_T1",
            "stat_health_T2", "stat_damage_T2"
        }));
    }

    [Test]
    public void CostIsFlatPerTier()
    {
        var talent = tree.GetTalentById(Unit, "stat_health_T1");
        int before = talent.GetCurrentCost();

        Buy("stat_health_T1", 3);

        Assert.That(before, Is.EqualTo(25));
        Assert.That(talent.GetCurrentCost(), Is.EqualTo(25));
    }

    [Test]
    public void CanBuyWithExactlyEnoughCinders()
    {
        Assert.That(Check("stat_health_T1", 25), Is.EqualTo(TalentPurchaseCheck.Ok));
        Assert.That(Check("stat_health_T1", 24), Is.EqualTo(TalentPurchaseCheck.NotEnoughCurrency));
    }

    [Test]
    public void UnknownTalentIsRejected()
    {
        Assert.That(Check("stat_speed_T1"), Is.EqualTo(TalentPurchaseCheck.UnknownTalent));
        Assert.That(TalentRules.CanPurchase(tree, state, "archer", "stat_health_T1", 1000), Is.EqualTo(TalentPurchaseCheck.UnknownTalent));
    }

    [Test]
    public void OnlyStatAndUnitUnlockTalentsAreSold()
    {
        Assert.That(Check("unlock_unit_x"), Is.EqualTo(TalentPurchaseCheck.Ok));
        Assert.That(Check("unlock_ability_y"), Is.EqualTo(TalentPurchaseCheck.NotPurchasable));
    }

    [Test]
    public void MaxPurchasesLimitsLevels()
    {
        Buy("stat_health_T1", 4);
        Assert.That(Check("stat_health_T1"), Is.EqualTo(TalentPurchaseCheck.Ok));

        Buy("stat_health_T1");
        Assert.That(Check("stat_health_T1"), Is.EqualTo(TalentPurchaseCheck.MaxedOut));
    }

    [Test]
    public void ZeroMaxPurchasesCantBeBought()
    {
        Assert.That(Check("no_buy_T1"), Is.EqualTo(TalentPurchaseCheck.MaxedOut));
    }

    [Test]
    public void TierPrerequisiteCountsLevelsAcrossTheTier()
    {
        Buy("stat_health_T1");
        Buy("stat_health_T1_1");
        Assert.That(Check("stat_health_T2"), Is.EqualTo(TalentPurchaseCheck.PrerequisitesNotMet));

        Buy("stat_health_T1_1");
        Assert.That(Check("stat_health_T2"), Is.EqualTo(TalentPurchaseCheck.Ok));
    }

    [Test]
    public void TierPointsIgnoreTalentsNoLongerInTheTree()
    {
        Buy("removed_talent_T1", 5);

        Assert.That(TalentRules.GetPointsInTier(tree, state, Unit, 1), Is.EqualTo(0));
    }

    [Test]
    public void UpgradePrerequisiteNeedsThatTalentOnTheSameUnit()
    {
        Assert.That(Check("stat_damage_T2"), Is.EqualTo(TalentPurchaseCheck.PrerequisitesNotMet));

        TalentRules.ApplyPurchase(state, "archer", "stat_health_T1", 25);
        Assert.That(Check("stat_damage_T2"), Is.EqualTo(TalentPurchaseCheck.PrerequisitesNotMet));

        Buy("stat_health_T1");
        Assert.That(Check("stat_damage_T2"), Is.EqualTo(TalentPurchaseCheck.Ok));
    }

    [Test]
    public void ApplyPurchaseAddsALevelAndRecordsTheCinders()
    {
        TalentRules.ApplyPurchase(state, Unit, "stat_health_T1", 25);
        TalentRules.ApplyPurchase(state, Unit, "stat_health_T1", 25);

        Assert.That(TalentRules.GetOwned(state, Unit, "stat_health_T1"), Is.EqualTo(2));
        Assert.That(state.CurrencySpent[CurrencyTypes.Cinders], Is.EqualTo(50));
    }
}
