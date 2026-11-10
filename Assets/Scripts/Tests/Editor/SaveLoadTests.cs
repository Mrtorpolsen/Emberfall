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

        save.Progress.Talents.Purchases["fighter"] = new UnitSaveData
        {
            PurchasedTalents = new Dictionary<string, int> { { "stat_health_1_T1", 2 } }
        };
        save.Progress.Talents.CurrencySpent[CurrencyTypes.Cinders] = 120;
        save.Progress.Talents.CurrencySpent[CurrencyTypes.Embers] = 3;

        save.Progress.Research.CompletedResearch["unit_damage_1"] = 4;
        var active = new ActiveResearch(ResearchCategory.Tower, "tower_range_1", 2, "Tower Range", 1_700_000_000);
        save.Progress.Research.ActiveResearch.Add(active);

        save.Progress.Currency.Cinders = 345;
        save.Progress.Currency.Embers = 7;

        save.Client.Loadouts.Presets.Add(new SavedLoadout
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
        save.Client.Loadouts.ActiveLoadoutId = 3;
        save.Client.Loadouts.NextLoadoutId = 4;

        save.Progress.HasReceivedLoginGift = true;
        save.Client.HasCompletedTutorial = true;

        save.PendingRuns.Add(new PendingRun
        {
            RunId = "run-1",
            Difficulty = DifficultyLevel.Hard,
            Minutes = 12,
            EndedAtUtc = 1_700_000_500
        });

        return save;
    }

    private static void AssertMatchesFullSave(SaveGame save)
    {
        Assert.AreEqual(CurrentSaveVersion.CURRENT_SAVE_VERSION, save.Version);

        Assert.AreEqual(2, save.Progress.Talents.Purchases["fighter"].PurchasedTalents["stat_health_1_T1"]);
        Assert.AreEqual(120, save.Progress.Talents.CurrencySpent[CurrencyTypes.Cinders]);
        Assert.AreEqual(3, save.Progress.Talents.CurrencySpent[CurrencyTypes.Embers]);

        Assert.AreEqual(4, save.Progress.Research.CompletedResearch["unit_damage_1"]);
        Assert.AreEqual(1, save.Progress.Research.ActiveResearch.Count);
        ActiveResearch active = save.Progress.Research.ActiveResearch[0];
        Assert.AreEqual(ResearchCategory.Tower, active.ResearchCategory);
        Assert.AreEqual("tower_range_1", active.ResearchId);
        Assert.AreEqual(2, active.TargetLevel);
        Assert.AreEqual("Tower Range", active.ResearchName);
        // The constructor stamps "now"; the saved start time must win
        Assert.AreEqual(1_700_000_000, active.StartTime);

        Assert.AreEqual(345, save.Progress.Currency.Cinders);
        Assert.AreEqual(7, save.Progress.Currency.Embers);

        Assert.AreEqual(1, save.Client.Loadouts.Presets.Count);
        SavedLoadout preset = save.Client.Loadouts.Presets[0];
        Assert.AreEqual(3, preset.Id);
        Assert.AreEqual("Rush", preset.DisplayName);
        CollectionAssert.AreEqual(new[] { "fighter", "archer", null, null }, preset.State.UnitLoadout);
        CollectionAssert.AreEqual(new[] { "tower", null, null }, preset.State.TowerLoadout);
        CollectionAssert.AreEqual(new[] { "heal", null }, preset.State.AbilityLoadout);
        Assert.AreEqual(3, save.Client.Loadouts.ActiveLoadoutId);
        Assert.AreEqual(4, save.Client.Loadouts.NextLoadoutId);

        Assert.IsTrue(save.Progress.HasReceivedLoginGift);
        Assert.IsTrue(save.Client.HasCompletedTutorial);
        Assert.AreEqual(ServerProgress.CurrentVersion, save.Progress.Version);

        Assert.AreEqual(1, save.PendingRuns.Count);
        PendingRun run = save.PendingRuns[0];
        Assert.AreEqual("run-1", run.RunId);
        Assert.AreEqual(DifficultyLevel.Hard, run.Difficulty);
        Assert.AreEqual(12, run.Minutes);
        Assert.AreEqual(1_700_000_500, run.EndedAtUtc);
    }

    private static void AssertAllSectionsPresent(SaveGame save)
    {
        Assert.IsNotNull(save.Progress);
        Assert.IsNotNull(save.Progress.Talents);
        Assert.IsNotNull(save.Progress.Talents.Purchases);
        Assert.IsNotNull(save.Progress.Talents.CurrencySpent);
        Assert.IsNotNull(save.Progress.Research);
        Assert.IsNotNull(save.Progress.Research.CompletedResearch);
        Assert.IsNotNull(save.Progress.Research.ActiveResearch);
        Assert.IsNotNull(save.Progress.Currency);
        Assert.IsNotNull(save.Client);
        Assert.IsNotNull(save.Client.Loadouts);
        Assert.IsNotNull(save.PendingRuns);
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
        first.Progress.Currency.Cinders = 1;

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
        string json = $@"{{ ""Version"": {CurrentSaveVersion.CURRENT_SAVE_VERSION}, ""Progress"": {{ ""Talents"": null, ""Research"": {{ ""ActiveResearch"": null }} }}, ""Client"": null }}";

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
        Assert.AreEqual(10, result.Save.Progress.Currency.Cinders);
        AssertAllSectionsPresent(result.Save);
    }

    [Test]
    public void Deserialize_InvalidJson_IsCorrupt()
    {
        SaveLoadResult result = SaveSerializer.Deserialize("{ \"Version\": 5, \"Progress\": ");

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
        string json = $@"{{ ""Version"": {CurrentSaveVersion.CURRENT_SAVE_VERSION}, ""Progress"": {{ ""Currency"": ""lots"" }} }}";

        SaveLoadResult result = SaveSerializer.Deserialize(json);

        Assert.AreEqual(SaveLoadStatus.Failed, result.Status);
        Assert.IsNull(result.Save);
    }
}
