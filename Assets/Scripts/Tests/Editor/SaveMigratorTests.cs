using Newtonsoft.Json.Linq;
using NUnit.Framework;

public class SaveMigratorTests
{
    private const string Version3Save = @"{
        ""Version"": 3,
        ""Talents"": {
            ""Purchases"": { ""fighter"": { ""PurchasedTalents"": { ""stat_health_1_T1"": 2 } } },
            ""CurrencySpent"": { ""Cinders"": 100, ""Embers"": 5 }
        },
        ""Research"": { ""CompletedResearch"": { ""unit_damage_1"": 3 }, ""ActiveResearch"": [] },
        ""Currency"": { ""Cinders"": 50, ""Embers"": 1 },
        ""Unlocks"": { ""UnlockedIds"": [ ""unlock_unit_berserker"" ] },
        ""Loadouts"": {},
        ""HasReceivedLoginGift"": true
    }";

    [Test]
    public void Migrate_FromVersion3_ReachesCurrentVersion()
    {
        SaveGame save = SaveMigrator.Migrate(JObject.Parse(Version3Save));

        Assert.AreEqual(CurrentSaveVersion.CURRENT_SAVE_VERSION, save.Version);
    }

    [Test]
    public void Migrate_FromVersion3_RefundsTalentCurrencyAndKeepsProgress()
    {
        SaveGame save = SaveMigrator.Migrate(JObject.Parse(Version3Save));

        // 3 -> 4 refunds currency spent on talents
        Assert.AreEqual(150, save.Progress.Currency.Cinders);
        Assert.AreEqual(6, save.Progress.Currency.Embers);

        Assert.IsTrue(save.Progress.HasReceivedLoginGift);
        Assert.AreEqual(2, save.Progress.Talents.Purchases["fighter"].PurchasedTalents["stat_health_1_T1"]);
        Assert.AreEqual(3, save.Progress.Research.CompletedResearch["unit_damage_1"]);
    }

    private const string Version4Save = @"{
        ""Version"": 4,
        ""Talents"": {
            ""Purchases"": { ""fighter"": { ""PurchasedTalents"": { ""stat_health_1_T1"": 2 } } },
            ""CurrencySpent"": { ""Cinders"": 100 }
        },
        ""Research"": {
            ""CompletedResearch"": { ""unit_damage_1"": 3 },
            ""ActiveResearch"": [ { ""ResearchCategory"": ""Tower"", ""ResearchId"": ""tower_range_1"", ""TargetLevel"": 2, ""StartTime"": 1700000000, ""ResearchName"": ""Tower Range"" } ]
        },
        ""Currency"": { ""Cinders"": 50, ""Embers"": 1 },
        ""Loadouts"": {
            ""Presets"": [ { ""Id"": 2, ""DisplayName"": ""Rush"", ""State"": { ""UnitLoadout"": [ ""fighter"", null, null, null ] } } ],
            ""ActiveLoadoutId"": 2,
            ""NextLoadoutId"": 3
        },
        ""Flags"": { ""HasReceivedLoginGift"": true, ""HasCompletedTutorial"": true }
    }";

    [Test]
    public void Migrate_FromVersion4_SplitsProgressFromClientState()
    {
        SaveGame save = SaveMigrator.Migrate(JObject.Parse(Version4Save));

        Assert.AreEqual(CurrentSaveVersion.CURRENT_SAVE_VERSION, save.Version);

        // Unlike 3 -> 4, nothing is refunded
        Assert.AreEqual(50, save.Progress.Currency.Cinders);
        Assert.AreEqual(1, save.Progress.Currency.Embers);
        Assert.AreEqual(100, save.Progress.Talents.CurrencySpent[CurrencyTypes.Cinders]);
        Assert.AreEqual(2, save.Progress.Talents.Purchases["fighter"].PurchasedTalents["stat_health_1_T1"]);
        Assert.AreEqual(3, save.Progress.Research.CompletedResearch["unit_damage_1"]);
        Assert.AreEqual(1, save.Progress.Research.ActiveResearch.Count);
        Assert.AreEqual(1_700_000_000, save.Progress.Research.ActiveResearch[0].StartTime);
        Assert.IsTrue(save.Progress.HasReceivedLoginGift);
        Assert.AreEqual(ServerProgress.CurrentVersion, save.Progress.Version);

        Assert.AreEqual(2, save.Client.Loadouts.ActiveLoadoutId);
        Assert.AreEqual("Rush", save.Client.Loadouts.Presets[0].DisplayName);
        Assert.IsTrue(save.Client.HasCompletedTutorial);

        Assert.That(save.PendingRuns, Is.Empty);
    }

    [Test]
    public void Migrate_FromVersion4WithoutFlags_DefaultsThemToFalse()
    {
        SaveGame save = SaveMigrator.Migrate(JObject.Parse(@"{ ""Version"": 4, ""Currency"": { ""Cinders"": 5 } }"));

        Assert.IsFalse(save.Progress.HasReceivedLoginGift);
        Assert.IsFalse(save.Client.HasCompletedTutorial);
        Assert.AreEqual(5, save.Progress.Currency.Cinders);
    }

    [Test]
    public void Migrate_CurrentVersion_IsUnchanged()
    {
        var current = new SaveGame { Version = CurrentSaveVersion.CURRENT_SAVE_VERSION };
        current.Progress.Currency.Cinders = 42;

        SaveGame save = SaveMigrator.Migrate(JObject.FromObject(current));

        Assert.AreEqual(CurrentSaveVersion.CURRENT_SAVE_VERSION, save.Version);
        Assert.AreEqual(42, save.Progress.Currency.Cinders);
    }
}
