using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class ResearchService : MonoBehaviour
{
    public static ResearchService Instance { get; private set; }
    public ResearchTree playerResearchTree { get; private set; }

    public event Action<ResearchCategory> OnResearchStarted;
    public event Action<ResearchCategory> OnResearchCompleted;
    public event Action<ResearchCategory, long> OnResearchTimeUpdated;

    private readonly Dictionary<string, Coroutine> researchCoroutines = new();

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
            // The server's definitions when they're newer: those are the costs it will charge
            bool useServer = DefinitionsSelector.PreferServer(jsonAsset.text, ServerDefinitions.ResearchJson);
            string json = useServer ? ServerDefinitions.ResearchJson : jsonAsset.text;
            Debug.Log($"Research definitions: {(useServer ? "server" : "bundled")} copy");
            var tree = JsonConvert.DeserializeObject<ResearchTree>(json);
            tree.NormalizeCategoryKeys();
            tree.ApplyScalingPresets();
            playerResearchTree = tree;

            if (SaveService.Instance.Current.Progress.Research.ActiveResearch.Count > 0)
            {
                RestartActiveResearchTimers();
            }

            ProvideUnlocked();
        }
        finally
        {
            Addressables.Release(handle);
        }
    }

    // Checks the rules, pays and starts the next level. False (nothing spent) when it can't start.
    public async Task<bool> TryStartResearchAsync(string id)
    {
        var state = SaveService.Instance.Current.Progress.Research;

        var check = ResearchRules.CanStart(playerResearchTree, state, id, CurrencyManager.Instance.Get(CurrencyTypes.Cinders));

        if (check != ResearchStartCheck.Ok)
        {
            Debug.LogWarning($"Can't start research {id}: {check}");
            return false;
        }

        ResearchDefinition research = playerResearchTree.GetResearchById(id);
        int cost = ResearchRules.GetCost(research, ResearchRules.GetLevel(state, id) + 1);

        if (!CurrencyManager.Instance.Spend(CurrencyTypes.Cinders, cost))
        {
            Debug.LogWarning("Failed to spend currency / start research");
            return false;
        }

        OnResearchStarted?.Invoke(research.Category);

        ActiveResearch researchToStart = ResearchRules.Start(playerResearchTree, state, id, DateTimeOffset.UtcNow.ToUnixTimeSeconds());

        var coroutine = StartCoroutine(ResearchTimerRoutine(researchToStart));
        researchCoroutines[researchToStart.ResearchId] = coroutine;

        await SaveService.Instance.SaveAsync();
        return true;
    }

    private void CompleteResearch(ActiveResearch active)
    {
        CompleteResearchInternal(active);
        SaveService.Instance.Save();
    }

    private void CompleteResearchInternal(ActiveResearch active)
    {
        ResearchRules.Complete(SaveService.Instance.Current.Progress.Research, active);

        PostUnlock(active);

        OnResearchCompleted?.Invoke(active.ResearchCategory);
    }

    private IEnumerator ResearchTimerRoutine(ActiveResearch active)
    {
        long endTime = ResearchRules.GetEndTime(playerResearchTree, active);

        while (DateTimeOffset.UtcNow.ToUnixTimeSeconds() < endTime)
        {
            long remaining = endTime - DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            OnResearchTimeUpdated?.Invoke(active.ResearchCategory, remaining);

            yield return new WaitForSeconds(1f);
        }

        // Final update to 0
        OnResearchTimeUpdated?.Invoke(active.ResearchCategory, 0);

        CompleteResearch(active);
        researchCoroutines.Remove(active.ResearchId);
    }

    private async void RestartActiveResearchTimers()
    {
        //To batch expired while offline, and save at once
        var expired = new List<ActiveResearch>();

        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        foreach (var active in SaveService.Instance.Current.Progress.Research.ActiveResearch.ToList())
        {
            if (ResearchRules.IsDone(playerResearchTree, active, now))
            {
                expired.Add(active);
            }
            else
            {
                var coroutine = StartCoroutine(ResearchTimerRoutine(active));
                researchCoroutines[active.ResearchId] = coroutine;
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
        long endTime = ResearchRules.GetEndTime(playerResearchTree, active);
        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        return Math.Max(0, endTime - now);
    }

    public float GetProgress(ActiveResearch active)
    {
        var def = playerResearchTree.GetResearchById(active.ResearchId);
        int duration = ResearchRules.GetDurationSeconds(def, active.TargetLevel);

        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        float elapsed = now - active.StartTime;

        return Mathf.Clamp01(elapsed / duration);
    }

    public ActiveResearch IsActiveCategory(ResearchCategory category)
    {
        var state = SaveService.Instance.Current?.Progress.Research;
        return state == null ? null : ResearchRules.GetActiveInCategory(state, category);
    }

    public int GetCurrentResearchLevel(string researchId)
    {
        return ResearchRules.GetLevel(SaveService.Instance.Current.Progress.Research, researchId);
    }

    private void PostUnlock(ActiveResearch active)
    {
        if (active.ResearchCategory == ResearchCategory.GlobalAbility)
        {
            if (UnlockService.Instance != null)
            {
                UnlockService.Instance.Unlock(active.ResearchId);
            }
            else
            {
                Debug.LogError("UnlockService is null. Cannot provide global ability unlock.");
            }
        }
    }

    private void ProvideUnlocked()
    {
        if (UnlockService.Instance == null)
        {
            Debug.LogError("UnlockService is null. Cannot provide talent unlocks.");
            return;
        }

        foreach (var research in playerResearchTree.GetResearchByCategory(ResearchCategory.GlobalAbility))
        {
            if (SaveService.Instance.Current.Progress.Research.CompletedResearch.TryGetValue(research.Id, out int level) && level > 0)
            {
                UnlockService.Instance.Unlock(research.Id);
            }
        }
    }

    public void SetTutorialResearchState()
    {
        var researchId = "unit_statsmodifier_health";

        if (researchCoroutines.TryGetValue(researchId, out var coroutine))
        {
            StopCoroutine(coroutine);
            researchCoroutines.Remove(researchId);
        }

        var active = SaveService.Instance.Current.Progress.Research.ActiveResearch
            .Find(x => x.ResearchId == researchId);

        if (active != null)
        {
            SaveService.Instance.Current.Progress.Research.ActiveResearch.Remove(active);
        }

        SaveService.Instance.Current.Progress.Research.CompletedResearch[researchId] = 0;
    }
}