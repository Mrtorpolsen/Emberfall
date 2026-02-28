using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class ResearchService : MonoBehaviour
{
    public static ResearchService Instance;
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
            tree.NormalizeCategoryKeys();
            playerResearchTree = tree;
        }
        finally
        {
            Addressables.Release(handle);
        }
    }

    public async void StartResearch(string id)
    {
        //Checks if category already being researched
        ResearchDefinition researchDef = playerResearchTree.GetResearchById(id);

        if (SaveService.Instance.Current.Research.ActiveResearches
            .Find(research => research.ResearchCategory == researchDef.Category) != null)
        {
            Debug.LogWarning("Category already being researched");
            return;
        }

        int currentStacks = 0;

        if (SaveService.Instance.Current.Research.CompletedResearch.TryGetValue(id, out var stacks))
        {
            currentStacks = stacks;
        }

        ActiveResearch researchToStart = new ActiveResearch(researchDef.Category, researchDef.Id, (currentStacks + 1));

        SaveService.Instance.Current.Research.ActiveResearches.Add(researchToStart);
        await SaveService.Instance.SaveAsync();
    }

    public void SaveResearch()
    {

    }

    public void ResearchComplete()
    {

    }

    public ActiveResearch IsActiveCategory(ResearchCategory category)
    {
        return SaveService.Instance.Current?.Research.ActiveResearches
            .Find(r => r.ResearchCategory == category);
    }
}
