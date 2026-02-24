using Newtonsoft.Json;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class TalentManager : MonoBehaviour
{
    public static TalentManager Instance;
    public TalentTree playerTalentTree;

    private const string TALENTS_ADDRESSABLE = "Talents";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnEnable()
    {
        if (SaveService.Instance != null)
        {
            SaveService.Instance.OnSaveLoaded += HandleSaveLoadedAsync;
        }
    }

    private void OnDisable()
    {
        if (SaveService.Instance != null)
        {
            SaveService.Instance.OnSaveLoaded -= HandleSaveLoadedAsync;
        }
    }

    private Task HandleSaveLoadedAsync()
    {
        return LoadPlayerTalentsAsync();
    }

    public async Task LoadPlayerTalentsAsync()
    {
        var handle = Addressables.LoadAssetAsync<TextAsset>(TALENTS_ADDRESSABLE);
        TextAsset jsonAsset = await handle.Task;

        try
        {
            var tree = JsonConvert.DeserializeObject<TalentTree>(jsonAsset.text);
            playerTalentTree = tree;
        }
        finally
        {
            Addressables.Release(handle);
        }
    }

    public void SaveTalent(string id)
    {
        if (SaveService.Instance.Current.Talents.Purchases.TryGetValue(id, out int purchased))
        {
            SaveService.Instance.Current.Talents.Purchases[id] = purchased + 1;
        }
        else
        {
            SaveService.Instance.Current.Talents.Purchases.Add(id, 1);
        }
    }

    public int GetPurchasedTalent(string id)
    {
        return SaveService.Instance.Current.Talents.Purchases.TryGetValue(id, out int count) ? count : 0;
    }
}
