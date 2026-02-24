using Newtonsoft.Json;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class ResearchManager : MonoBehaviour
{
    public static ResearchManager Instance;
    public ResearchTree playerResearchTree;

    private const string RESEARCH_ADDRESSABLE = "Research";

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
        return LoadPlayerResearchAsync();
    }

    private async Task LoadPlayerResearchAsync()
    {
        var handle = Addressables.LoadAssetAsync<TextAsset>(RESEARCH_ADDRESSABLE);
        TextAsset jsonAsset = await handle.Task;

        try
        {
            var tree = JsonConvert.DeserializeObject<ResearchTree>(jsonAsset.text);
            playerResearchTree = tree;
        }
        finally
        {
            Addressables.Release(handle);
        }
    }
}
