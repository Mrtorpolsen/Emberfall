using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class SaveService : MonoBehaviour
{
    public static SaveService main;

    public SaveGame Current { get; private set; }

    private string savePath;

    private void Awake()
    {
        if (main != null)
        {
            Destroy(gameObject);
            return;
        }

        main = this;
        DontDestroyOnLoad(gameObject);
        
        savePath = Path.Combine(Application.persistentDataPath, $"{IdentityService.main.Current.GetPlayerId()}_save.json");
    }

    public void CreateSave()
    {
        Current = new SaveGame();
        Save();
    }

    public async void Load()
    {
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
