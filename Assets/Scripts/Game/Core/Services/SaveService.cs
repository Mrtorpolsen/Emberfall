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

    public void CreateSave()
    {
        Current = new SaveGame();
        ValidateSave();
        Save();
    }

    public async Task Load()
    {
        if (!ValidateSavePath())
            return;

        if (!File.Exists(savePath))
        {
            Debug.LogWarning("No save file found, creating new save file");
            CreateSave();
            return;
        }

        string json = await Task.Run(() => File.ReadAllText(savePath)); // async file read
        Current = JsonConvert.DeserializeObject<SaveGame>(json);

        ValidateSave();

        if (OnSaveLoaded != null)
        {
            var handlers = OnSaveLoaded.GetInvocationList();
            var tasks = new List<Task>();

            foreach (Func<Task> handler in handlers)
            {
                tasks.Add(handler());
            }

            await Task.WhenAll(tasks);
        }
    }

    public void Save()
    {
        if (!ValidateSavePath())
            return;

        string json = JsonConvert.SerializeObject(Current);
        File.WriteAllText(savePath, json);
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

    private void ValidateSave()
    {
        Current ??= new SaveGame();
        Current.Talents ??= new PlayerTalentState();
        Current.Talents.Purchases ??= new Dictionary<string, int>();
    }
}
