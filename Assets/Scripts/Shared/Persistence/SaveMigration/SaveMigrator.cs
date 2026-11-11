using System;
using Newtonsoft.Json.Linq;
using UnityEngine;

// Upgrades old saves one version at a time (3 -> 4 -> 5 ...), so each bump only needs a single MigrateNToM step.
// When bumping the version: keep the old shape as SaveGameN, add a case below, and make MigrateNToM return version N+1.
public static class SaveMigrator
{
    public const int OldestSupportedVersion = 3;

    public static SaveGame Migrate(JObject root)
    {
        int version = root.Value<int>("Version");

        while (version < CurrentSaveVersion.CURRENT_SAVE_VERSION)
        {
            root = version switch
            {
                3 => JObject.FromObject(Migrate3To4(root.ToObject<SaveGame3>())),
                4 => JObject.FromObject(Migrate4To5(root.ToObject<SaveGame4>())),
                _ => throw new Exception($"No migration found for version {version}")
            };

            version++;
        }

        return root.ToObject<SaveGame>();
    }

    private static SaveGame4 Migrate3To4(SaveGame3 oldSave)
    {
        Debug.Log("running Migrate3To4");
        SaveGame4 newSave = new SaveGame4
        {
            Version = 4,
            Talents = oldSave.Talents,
            Research = oldSave.Research,
            Currency = oldSave.Currency,
            Loadouts = oldSave.Loadouts
        };

        int cindersSpent = oldSave.Talents.CurrencySpent.TryGetValue(
            CurrencyTypes.Cinders, out int cinders)
            ? cinders
            : 0;

        int embersSpent = oldSave.Talents.CurrencySpent.TryGetValue(
            CurrencyTypes.Embers, out int embers)
            ? embers
            : 0;

        newSave.Currency.Cinders += cindersSpent;
        newSave.Currency.Embers += embersSpent;

        newSave.Flags.HasReceivedLoginGift = oldSave.HasReceivedLoginGift;

        return newSave;
    }

    // Splits the save by owner for cloud save: progress (server), client state (game), no pending runs yet
    private static SaveGame Migrate4To5(SaveGame4 oldSave)
    {
        Debug.Log("running Migrate4To5");
        return new SaveGame
        {
            Version = 5,
            Progress = new ServerProgress
            {
                Currency = oldSave.Currency,
                Talents = oldSave.Talents,
                Research = oldSave.Research,
                HasReceivedLoginGift = oldSave.Flags?.HasReceivedLoginGift ?? false
            },
            Client = new ClientState
            {
                Loadouts = oldSave.Loadouts,
                HasCompletedTutorial = oldSave.Flags?.HasCompletedTutorial ?? false
            }
        };
    }
}
