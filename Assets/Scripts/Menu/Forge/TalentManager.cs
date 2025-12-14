using Newtonsoft.Json;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class TalentManager : MonoBehaviour
{
    public static TalentManager main;
    public TalentTree playerTalentTree;

    private const string TALENTS_ADDRESSABLE = "Talents";

    private void Awake()
    {
        if (main != null && main != this)
        {
            Destroy(gameObject);
            return;
        }

        main = this;
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
