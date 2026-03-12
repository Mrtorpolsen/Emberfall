using Newtonsoft.Json;
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

    public async Task CreateSave()
    {
        Current = new SaveGame();
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
            await CreateSave();
            return;
        }

        string json = await Task.Run(() => File.ReadAllText(savePath)); // async file read
        Current = JsonConvert.DeserializeObject<SaveGame>(json);

        ValidateSave();

        await InvokeOnSaveLoaded();
    }

    public async Task SaveAsync()
    {
        if (!ValidateSavePath())
            return;

        string json = JsonConvert.SerializeObject(Current);
        await Task.Run(() => File.WriteAllText(savePath, json));
    }

    public void Save()
    {
        _ = SaveAsync().ContinueWith(t =>
        {
            if (t.Exception != null)
                Debug.LogError(t.Exception);
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
        // Ensure the root save exists
        Current ??= new SaveGame();

        // Talents
        Current.Talents ??= new PlayerTalentState();
        Current.Talents.Purchases ??= new Dictionary<string, int>();
        Current.Talents.CurrencySpent ??= new Dictionary<CurrencyTypes, int>();

        // Research
        Current.Research ??= new PlayerResearchState();
        Current.Research.CompletedResearch ??= new Dictionary<string, int>();
        Current.Research.ActiveResearches ??= new List<ActiveResearch>();

        // Currency
        Current.Currency ??= new CurrencyData();
    }
}
