using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

public enum SaveLoadStatus
{
    Loaded,
    Migrated,
    // Not valid JSON / no readable version
    Corrupt,
    // Older than SaveMigrator.OldestSupportedVersion
    Unsupported,
    // Valid JSON that doesn't fit SaveGame or fails to migrate
    Failed
}

public readonly struct SaveLoadResult
{
    public readonly SaveLoadStatus Status;
    public readonly SaveGame Save;
    public readonly int Version;
    public readonly string Error;

    public SaveLoadResult(SaveLoadStatus status, SaveGame save, int version, string error)
    {
        Status = status;
        Save = save;
        Version = version;
        Error = error;
    }

    public bool Succeeded => Status == SaveLoadStatus.Loaded || Status == SaveLoadStatus.Migrated;
}

// Save JSON <-> SaveGame, kept out of SaveService so it can be tested without files or a MonoBehaviour
public static class SaveSerializer
{
    public static string Serialize(SaveGame save) => JsonConvert.SerializeObject(save);

    public static SaveLoadResult Deserialize(string json)
    {
        JObject root;
        int version;

        try
        {
            root = JObject.Parse(json);
            version = root.Value<int>("Version");
        }
        catch (Exception e)
        {
            return new SaveLoadResult(SaveLoadStatus.Corrupt, null, 0, e.Message);
        }

        if (version < SaveMigrator.OldestSupportedVersion)
            return new SaveLoadResult(SaveLoadStatus.Unsupported, null, version, null);

        bool migrated = version < CurrentSaveVersion.CURRENT_SAVE_VERSION;

        try
        {
            SaveGame save = migrated
                ? SaveMigrator.Migrate(root)
                : root.ToObject<SaveGame>();

            EnsureDefaults(save);
            return new SaveLoadResult(migrated ? SaveLoadStatus.Migrated : SaveLoadStatus.Loaded, save, version, null);
        }
        catch (Exception e)
        {
            return new SaveLoadResult(SaveLoadStatus.Failed, null, version, e.ToString());
        }
    }

    // Fills sections missing from older or hand-edited saves, so callers never null-check them
    public static SaveGame EnsureDefaults(SaveGame save)
    {
        save ??= new SaveGame();

        save.Progress ??= new ServerProgress();
        save.Progress.EnsureDefaults();

        save.Client ??= new ClientState();
        save.Client.Loadouts ??= new PlayerLoadoutCollection();

        save.PendingRuns ??= new List<PendingRun>();

        return save;
    }
}
