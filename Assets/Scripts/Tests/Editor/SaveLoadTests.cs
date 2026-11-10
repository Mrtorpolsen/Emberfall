using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveLoadTests
{
    private string tempDirectory;

    [SetUp]
    public void SetUp()
    {
        tempDirectory = Path.Combine(Application.temporaryCachePath, "SaveLoadTests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDirectory);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(tempDirectory))
            Directory.Delete(tempDirectory, true);
    }

    // Every section set to a non-default value, so a field that doesn't survive JSON shows up
    private static SaveGame CreateFullSave()
    {
        var save = new SaveGame { Version = CurrentSaveVersion.CURRENT_SAVE_VERSION };

        save.Talents.Purchases["fighter"] = new UnitSaveData
        {
            PurchasedTalents = new Dictionary<string, int> { { "stat_health_1_T1", 2 } }
        };
        save.Talents.CurrencySpent[CurrencyTypes.Cinders] = 120;
        save.Talents.CurrencySpent[CurrencyTypes.Embers] = 3;

        save.Research.CompletedResearch["unit_damage_1"] = 4;
        var active = new ActiveResearch(ResearchCategory.Tower, "tower_range_1", 2, "Tower Range", 1_700_000_000);
        save.Research.ActiveResearch.Add(active);

        save.Currency.Cinders = 345;
        save.Currency.Embers = 7;

        save.Loadouts.Presets.Add(new SavedLoadout
        {
            Id = 3,
            DisplayName = "Rush",
            State = new PlayerLoadoutState
            {
                UnitLoadout = new[] { "fighter", "archer", null, null },
                TowerLoadout = new[] { "tower", null, null },
                AbilityLoadout = new[] { "heal", null }
            }
        });
        save.Loadouts.ActiveLoadoutId = 3;
        save.Loadouts.NextLoadoutId = 4;

        save.Flags.HasReceivedLoginGift = true;
        save.Flags.HasCompletedTutorial = true;

        return save;
    }

    private static void AssertMatchesFullSave(SaveGame save)
    {
        Assert.AreEqual(CurrentSaveVersion.CURRENT_SAVE_VERSION, save.Version);

        Assert.AreEqual(2, save.Talents.Purchases["fighter"].PurchasedTalents["stat_health_1_T1"]);
        Assert.AreEqual(120, save.Talents.CurrencySpent[CurrencyTypes.Cinders]);
        Assert.AreEqual(3, save.Talents.CurrencySpent[CurrencyTypes.Embers]);

        Assert.AreEqual(4, save.Research.CompletedResearch["unit_damage_1"]);
        Assert.AreEqual(1, save.Research.ActiveResearch.Count);
        ActiveResearch active = save.Research.ActiveResearch[0];
        Assert.AreEqual(ResearchCategory.Tower, active.ResearchCategory);
        Assert.AreEqual("tower_range_1", active.ResearchId);
        Assert.AreEqual(2, active.TargetLevel);
        Assert.AreEqual("Tower Range", active.ResearchName);
        // The constructor stamps "now"; the saved start time must win
        Assert.AreEqual(1_700_000_000, active.StartTime);

        Assert.AreEqual(345, save.Currency.Cinders);
        Assert.AreEqual(7, save.Currency.Embers);

        Assert.AreEqual(1, save.Loadouts.Presets.Count);
        SavedLoadout preset = save.Loadouts.Presets[0];
        Assert.AreEqual(3, preset.Id);
        Assert.AreEqual("Rush", preset.DisplayName);
        CollectionAssert.AreEqual(new[] { "fighter", "archer", null, null }, preset.State.UnitLoadout);
        CollectionAssert.AreEqual(new[] { "tower", null, null }, preset.State.TowerLoadout);
        CollectionAssert.AreEqual(new[] { "heal", null }, preset.State.AbilityLoadout);
        Assert.AreEqual(3, save.Loadouts.ActiveLoadoutId);
        Assert.AreEqual(4, save.Loadouts.NextLoadoutId);

        Assert.IsTrue(save.Flags.HasReceivedLoginGift);
        Assert.IsTrue(save.Flags.HasCompletedTutorial);
    }

    private static void AssertAllSectionsPresent(SaveGame save)
    {
        Assert.IsNotNull(save.Talents);
        Assert.IsNotNull(save.Talents.Purchases);
        Assert.IsNotNull(save.Talents.CurrencySpent);
        Assert.IsNotNull(save.Research);
        Assert.IsNotNull(save.Research.CompletedResearch);
        Assert.IsNotNull(save.Research.ActiveResearch);
        Assert.IsNotNull(save.Currency);
        Assert.IsNotNull(save.Loadouts);
        Assert.IsNotNull(save.Flags);
    }

    [Test]
    public void RoundTrip_KeepsEverySection()
    {
        string json = SaveSerializer.Serialize(CreateFullSave());

        SaveLoadResult result = SaveSerializer.Deserialize(json);

        Assert.AreEqual(SaveLoadStatus.Loaded, result.Status);
        AssertMatchesFullSave(result.Save);
    }

    [Test]
    public void RoundTrip_ThroughFile_KeepsLatestSave()
    {
        string path = Path.Combine(tempDirectory, "player_save.json");

        var first = new SaveGame { Version = CurrentSaveVersion.CURRENT_SAVE_VERSION };
        first.Currency.Cinders = 1;

        // First write creates the file, the second replaces it
        SaveService.WriteAtomic(path, SaveSerializer.Serialize(first));
        SaveService.WriteAtomic(path, SaveSerializer.Serialize(CreateFullSave()));

        SaveLoadResult result = SaveSerializer.Deserialize(File.ReadAllText(path));

        Assert.AreEqual(SaveLoadStatus.Loaded, result.Status);
        AssertMatchesFullSave(result.Save);
        Assert.IsFalse(File.Exists(path + ".tmp"), "Temp file should be swapped in, not left behind");
    }

    [Test]
    public void Deserialize_MissingSections_FillsDefaults()
    {
        string json = $@"{{ ""Version"": {CurrentSaveVersion.CURRENT_SAVE_VERSION}, ""Talents"": null, ""Research"": {{ ""ActiveResearch"": null }} }}";

        SaveLoadResult result = SaveSerializer.Deserialize(json);

        Assert.AreEqual(SaveLoadStatus.Loaded, result.Status);
        AssertAllSectionsPresent(result.Save);
    }

    [Test]
    public void Deserialize_OldSupportedVersion_IsMigrated()
    {
        string json = $@"{{ ""Version"": {SaveMigrator.OldestSupportedVersion}, ""Currency"": {{ ""Cinders"": 10 }} }}";

        SaveLoadResult result = SaveSerializer.Deserialize(json);

        Assert.AreEqual(SaveLoadStatus.Migrated, result.Status);
        Assert.AreEqual(CurrentSaveVersion.CURRENT_SAVE_VERSION, result.Save.Version);
        Assert.AreEqual(10, result.Save.Currency.Cinders);
        AssertAllSectionsPresent(result.Save);
    }

    [Test]
    public void Deserialize_InvalidJson_IsCorrupt()
    {
        SaveLoadResult result = SaveSerializer.Deserialize("{ \"Version\": 4, \"Currency\": ");

        Assert.AreEqual(SaveLoadStatus.Corrupt, result.Status);
        Assert.IsNull(result.Save);
    }

    [Test]
    public void Deserialize_TooOldVersion_IsUnsupported()
    {
        int tooOld = SaveMigrator.OldestSupportedVersion - 1;

        SaveLoadResult result = SaveSerializer.Deserialize($@"{{ ""Version"": {tooOld} }}");

        Assert.AreEqual(SaveLoadStatus.Unsupported, result.Status);
        Assert.AreEqual(tooOld, result.Version);
        Assert.IsNull(result.Save);
    }

    [Test]
    public void Deserialize_WrongShape_IsFailed()
    {
        string json = $@"{{ ""Version"": {CurrentSaveVersion.CURRENT_SAVE_VERSION}, ""Currency"": ""lots"" }}";

        SaveLoadResult result = SaveSerializer.Deserialize(json);

        Assert.AreEqual(SaveLoadStatus.Failed, result.Status);
        Assert.IsNull(result.Save);
    }
}
