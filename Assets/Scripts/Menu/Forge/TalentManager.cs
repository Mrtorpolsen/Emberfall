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

}
