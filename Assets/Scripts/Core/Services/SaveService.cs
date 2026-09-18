using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class SaveService : GlobalSystem<SaveService>
{
    public SaveGame Current { get; private set; }

    private string savePath;

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

        try
        {
            root = JObject.Parse(json);
        }
        catch (JsonReaderException e)
        {
            Debug.LogError($"Failed to parse save file: {e.Message}");
            Debug.LogError("Creating new save.");
            await SaveCorrupt(json);
            await CreateNewSave();
            return;
        }

        int version = root.Value<int>("Version");

        if (version < 3)
        {
            Debug.LogError("Save file too old to migrate, creating new save");
            await CreateNewSave();
            return;
        }

        if (version < CurrentSaveVersion.CURRENT_SAVE_VERSION)
        {
            Current = Migrate(root, version);

            ValidateSave();
            await SaveAsync();
        }
        else
        {
            Current = JsonConvert.DeserializeObject<SaveGame>(json);
            ValidateSave();
        }

        await InvokeOnSaveLoaded();
    }

    public Task SaveAsync()
    {
        var snapshot = DeepClone(Current);
        return SaveInternal(snapshot);
    }

    public void Save()
    {
        _ = SaveAsync().ContinueWith(t =>
        {
            if (t.Exception != null)
                Debug.LogError(t.Exception);
        });
    }

    private async Task SaveInternal(SaveGame snapshot)
    {
        string json = JsonConvert.SerializeObject(snapshot);

        await Task.Run(() =>
        {
            File.WriteAllText(savePath, json);
        });
    }

    private async Task SaveCorrupt(string json)
    {
        string corruptSavePath = savePath + ".corrupt" + "-" + DateTime.Now.ToString();

        await Task.Run(() =>
        {
            File.WriteAllText(corruptSavePath, json);
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

    public static T DeepClone<T>(T obj)
    {
        var json = JsonConvert.SerializeObject(obj);
        return JsonConvert.DeserializeObject<T>(json);
    }

    private SaveGame Migrate(JObject root, int version)
    {
        return version switch
        {
            3 => Migrate3To4(root.ToObject<SaveGame3>()),
            _ => throw new Exception($"No migration found for version {version}")
        };
    }

    private SaveGame Migrate3To4(SaveGame3 oldSave)
    {
        Debug.Log("running Migrate3To4");
        SaveGame newSave = new SaveGame
        {
            Version = CurrentSaveVersion.CURRENT_SAVE_VERSION,
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
}
