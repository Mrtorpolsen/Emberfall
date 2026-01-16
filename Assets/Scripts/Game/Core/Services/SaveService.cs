using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class SaveService : GlobalSystem<SaveService>
{
    public SaveGame Current { get; private set; }

    private string savePath;

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
    }

    public void Save()
    {
        if (!ValidateSavePath())
            return;

        string json = JsonConvert.SerializeObject(Current);
        File.WriteAllText(savePath, json);
    }

    public void AddToSave(string id)
    {
        if (Current.Talents.Purchases.TryGetValue(id, out int purchased))
        {
            Current.Talents.Purchases[id] = purchased + 1;
        }
        else
        {
            Current.Talents.Purchases.Add(id, 1);
        }
    }

    public int GetPurchases(string id)
    {
        return Current.Talents.Purchases.TryGetValue(id, out int count) ? count : 0;
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
