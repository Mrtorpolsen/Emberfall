using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Android.Gradle.Manifest;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class ResearchService : MonoBehaviour
{
    public static ResearchService Instance;
    public ResearchTree playerResearchTree;

    public event Action<ResearchCategory> OnResearchStarted;
    public event Action<ResearchCategory> OnResearchCompleted;
    public event Action<ResearchCategory, long> OnResearchTimeUpdated;

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

            if (SaveService.Instance.Current.Research.ActiveResearches.Count > 0)
            {
                RestartActiveResearchTimers();
            }
        }
        finally
        {
            Addressables.Release(handle);
        }
    }

    public async Task StartResearch(string id)
    {
        //Checks if category already being researched
        ResearchDefinition research = playerResearchTree.GetResearchById(id);

        if (SaveService.Instance.Current.Research.ActiveResearches
            .Find(activeResearch => activeResearch.ResearchCategory == research.Category) != null)
        {
            Debug.LogWarning("Category already being researched");
            return;
        }

        int currentLevel = 0;

        if (SaveService.Instance.Current.Research.CompletedResearch.TryGetValue(id, out var level))
        {
            currentLevel = level;
        }

        if (currentLevel == research.MaxLevel)
        {
            throw new InvalidOperationException("Current research maxed");
        }

        OnResearchStarted?.Invoke(research.Category);

        ActiveResearch researchToStart = new ActiveResearch(research.Category, research.Id, (currentLevel + 1), research.Name);

        SaveService.Instance.Current.Research.ActiveResearches.Add(researchToStart);

        StartCoroutine(ResearchTimerRoutine(researchToStart, research));

        await SaveService.Instance.SaveAsync();
    }

    private void CompleteResearch(ActiveResearch active)
    {
        SaveService.Instance.Current.Research.ActiveResearches.Remove(active);
        SaveService.Instance.Current.Research.CompletedResearch[active.ResearchId] = active.TargetLevel;
        SaveService.Instance.Save();

        OnResearchCompleted?.Invoke(active.ResearchCategory);
    }

    private void CompleteResearchInternal(ActiveResearch active)
    {
        SaveService.Instance.Current.Research.ActiveResearches.Remove(active);
        SaveService.Instance.Current.Research.CompletedResearch[active.ResearchId] = active.TargetLevel;

        OnResearchCompleted?.Invoke(active.ResearchCategory);
    }

    private IEnumerator ResearchTimerRoutine(ActiveResearch active, ResearchDefinition research)
    {
        int duration = research.TimeScaling.GetAmountForNextLevelLinear(active.TargetLevel);
        long endTime = active.StartTime + duration;

        while (DateTimeOffset.UtcNow.ToUnixTimeSeconds() < endTime)
        {
            long remaining = endTime - DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            OnResearchTimeUpdated?.Invoke(active.ResearchCategory, remaining);

            yield return new WaitForSeconds(1f);
        }

        // Final update to 0
        OnResearchTimeUpdated?.Invoke(active.ResearchCategory, 0);

        CompleteResearch(active);
    }

    private async void RestartActiveResearchTimers()
    {
        //To batch expired while offline, and save at once
        var expired = new List<ActiveResearch>();

        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        foreach (var active in SaveService.Instance.Current.Research.ActiveResearches.ToList())
        {
            ResearchDefinition def = playerResearchTree.GetResearchById(active.ResearchId);

            int duration = def.TimeScaling.GetAmountForNextLevelLinear(active.TargetLevel);
            long endTime = active.StartTime + duration;

            if (now >= endTime)
            {
                expired.Add(active);
            }
            else
            {
                StartCoroutine(ResearchTimerRoutine(active, def));
            }
        }

        if (expired.Count > 0)
        {
            foreach (var active in expired)
            {
                CompleteResearchInternal(active);
            }

            await SaveService.Instance.SaveAsync();
        }
    }

    public long GetRemainingSeconds(ActiveResearch active)
    {
        ResearchDefinition def = playerResearchTree.GetResearchById(active.ResearchId);
        int duration = def.TimeScaling.GetAmountForNextLevelLinear(active.TargetLevel);

        long endTime = active.StartTime + duration;
        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        return Math.Max(0, endTime - now);
    }

    public float GetProgress(ActiveResearch active)
    {
        var def = playerResearchTree.GetResearchById(active.ResearchId);
        int duration = def.TimeScaling.GetAmountForNextLevelLinear(active.TargetLevel);

        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        float elapsed = now - active.StartTime;

        return Mathf.Clamp01(elapsed / duration);
    }

    public ActiveResearch IsActiveCategory(ResearchCategory category)
    {
        return SaveService.Instance.Current?.Research.ActiveResearches
            .Find(r => r.ResearchCategory == category);
    }

    public int GetCurrentResearchLevel(string researchId)
    {
        if (SaveService.Instance.Current.Research.CompletedResearch
            .TryGetValue(researchId, out int level))
        {
            return level;
        }

        return 0;
    }
}