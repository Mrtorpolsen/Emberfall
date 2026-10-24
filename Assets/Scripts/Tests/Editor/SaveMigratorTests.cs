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
        Assert.AreEqual(150, save.Currency.Cinders);
        Assert.AreEqual(6, save.Currency.Embers);

        Assert.IsTrue(save.Flags.HasReceivedLoginGift);
        Assert.AreEqual(2, save.Talents.Purchases["fighter"].PurchasedTalents["stat_health_1_T1"]);
        Assert.AreEqual(3, save.Research.CompletedResearch["unit_damage_1"]);
    }

    [Test]
    public void Migrate_CurrentVersion_IsUnchanged()
    {
        var current = new SaveGame { Version = CurrentSaveVersion.CURRENT_SAVE_VERSION };
        current.Currency.Cinders = 42;

        SaveGame save = SaveMigrator.Migrate(JObject.FromObject(current));

        Assert.AreEqual(CurrentSaveVersion.CURRENT_SAVE_VERSION, save.Version);
        Assert.AreEqual(42, save.Currency.Cinders);
    }
}
