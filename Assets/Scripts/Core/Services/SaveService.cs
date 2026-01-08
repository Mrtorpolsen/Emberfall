using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class SaveService : GameService<SaveService>
{
    public SaveGame Current { get; private set; }

    private string savePath;

    protected override void Awake()
    {
        base.Awake();
    }

    public void CreateSave()
    {
        Current = new SaveGame();
        Save();
    }

    public async Task Load()
    {
        savePath = Path.Combine(Application.persistentDataPath, $"{IdentityService.Instance.Current.GetPlayerId()}_save.json");
        Debug.LogWarning(savePath);
        if (!File.Exists(savePath))
        {
            CreateSave();
            return;
        }

        string json = await Task.Run(() => File.ReadAllText(savePath)); // async file read
        Current = JsonUtility.FromJson<SaveGame>(json);
    }

    public void Save()
    {
        string json = JsonUtility.ToJson(Current, true);
        File.WriteAllText(savePath, json);
    }
}
