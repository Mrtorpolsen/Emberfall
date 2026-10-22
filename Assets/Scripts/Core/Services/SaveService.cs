using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class SaveService : GlobalSystem<SaveService>
{
    public SaveGame Current { get; private set; }

    private string savePath;

    // Saves are written one at a time; each request is numbered so an older snapshot never overwrites a newer one
    private readonly SemaphoreSlim saveLock = new(1, 1);
    private int requestedSaveSequence;
    private int writtenSaveSequence;

    public event Func<Task> OnSaveLoaded;

    protected override void Awake()
    {
        base.Awake();
    }

    public void InitializeForPlayer(string playerId)
    {
        savePath = Path.Combine(
            Application.persistentDataPath,
            $"{playerId}_save.json"
        );
    }

    public async Task CreateNewSave()
    {
        Current = new SaveGame
        {
            Version = CurrentSaveVersion.CURRENT_SAVE_VERSION
        };

        ValidateSave();
        await SaveAsync();
        await InvokeOnSaveLoaded();
    }

    public async Task Load()
    {
        if (!ValidateSavePath())
            return;

        if (!File.Exists(savePath))
        {
            Debug.LogWarning("No save file found, creating new save file");
            await CreateNewSave();
            return;
        }

        string json = await Task.Run(() => File.ReadAllText(savePath));

        JObject root;
        int version;

        try
        {
            root = JObject.Parse(json);
            version = root.Value<int>("Version");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to parse save file, creating new save: {e.Message}");
            await BackupSave(json, "corrupt");
            await CreateNewSave();
            return;
        }

        if (version < SaveMigrator.OldestSupportedVersion)
        {
            Debug.LogError($"Save version {version} is too old to migrate, creating new save");
            await BackupSave(json, $"unsupported-v{version}");
            await CreateNewSave();
            return;
        }

        bool migrated = version < CurrentSaveVersion.CURRENT_SAVE_VERSION;
        SaveGame loaded;

        try
        {
            loaded = migrated
                ? SaveMigrator.Migrate(root)
                : JsonConvert.DeserializeObject<SaveGame>(json);
        }
        catch (Exception e)
        {
            // Valid JSON that doesn't fit SaveGame or fails to migrate; keep a copy so it can be recovered
            Debug.LogError($"Failed to load save version {version}, creating new save: {e}");
            await BackupSave(json, $"failed-v{version}");
            await CreateNewSave();
            return;
        }

        Current = loaded;
        ValidateSave();

        if (migrated)
        {
            await SaveAsync();
        }

        await InvokeOnSaveLoaded();
    }

    public Task SaveAsync()
    {
        // Serializing on the main thread at call time is the snapshot
        string json = JsonConvert.SerializeObject(Current);
        return SaveInternal(json, ++requestedSaveSequence);
    }

    public void Save()
    {
        _ = SaveAsync().ContinueWith(t =>
        {
            if (t.Exception != null)
                Debug.LogError(t.Exception);
        });
    }

    private async Task SaveInternal(string json, int sequence)
    {
        await saveLock.WaitAsync();

        try
        {
            // A newer snapshot was already written; writing this one would roll the save back
            if (sequence <= writtenSaveSequence)
                return;

            await Task.Run(() => WriteAtomic(savePath, json));
            writtenSaveSequence = sequence;
        }
        finally
        {
            saveLock.Release();
        }
    }

    // Write to a temp file, then swap it in, so a crash or kill mid-write can't leave a truncated save
    private static void WriteAtomic(string path, string contents)
    {
        string tempPath = path + ".tmp";

        File.WriteAllText(tempPath, contents);

        if (File.Exists(path))
        {
            File.Replace(tempPath, path, null);
        }
        else
        {
            File.Move(tempPath, path);
        }
    }

    // Copies a save we can't use next to the real one before it gets replaced, e.g. <id>_save.json.corrupt-20261022-143000
    private async Task BackupSave(string json, string reason)
    {
        string backupPath = $"{savePath}.{reason}-{DateTime.Now:yyyyMMdd-HHmmss}";

        await Task.Run(() =>
        {
            File.WriteAllText(backupPath, json);
        });
    }

    private bool ValidateSavePath()
    {
        if (string.IsNullOrEmpty(savePath))
        {
            Debug.LogError("SaveService not initialized with player ID");
            return false;
        }

        return true;
    }

    private async Task InvokeOnSaveLoaded()
    {
        if (OnSaveLoaded == null)
            return;

        var handlers = OnSaveLoaded.GetInvocationList();
        var tasks = new List<Task>();

        foreach (Func<Task> handler in handlers)
        {
            tasks.Add((Task)handler());
        }

        await Task.WhenAll(tasks);
    }

    private void ValidateSave()
    {
        Current ??= new SaveGame();

        Current.Talents ??= new PlayerTalentState();
        Current.Talents.Purchases ??= new Dictionary<string, UnitSaveData>();
        Current.Talents.CurrencySpent ??= new Dictionary<CurrencyTypes, int>();

        Current.Research ??= new PlayerResearchState();
        Current.Research.CompletedResearch ??= new Dictionary<string, int>();
        Current.Research.ActiveResearch ??= new List<ActiveResearch>();

        Current.Currency ??= new CurrencyData();

        Current.Loadouts ??= new PlayerLoadoutCollection();
        Current.Flags ??= new PlayerFlagsState();
    }
}
