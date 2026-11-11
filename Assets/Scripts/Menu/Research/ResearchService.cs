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

    // Research IDs with a CompleteResearch call in flight, so the timer and a reconnect don't both send one
    private readonly HashSet<string> completing = new();

    // Retry delays while the server says NotDone (the device clock is ahead) or can't be reached
    private const float FirstRetrySeconds = 10f;
    private const float MaxRetrySeconds = 300f;

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

    private void Start()
    {
        // IdentityService is created in Bootstrap, before this object's scene (Login) loads
        if (IdentityService.Instance != null)
        {
            IdentityService.Instance.Reconnected += HandleReconnected;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this && IdentityService.Instance != null)
        {
            IdentityService.Instance.Reconnected -= HandleReconnected;
        }
    }

    // Research that finished while offline completes once the server can be reached
    private void HandleReconnected()
    {
        if (playerResearchTree != null)
        {
            RestartActiveResearchTimers();
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

    // Starts the next level on the server (it checks, charges and times it; docs/cloud-save.md).
    // False (nothing spent) when it can't start.
    public async Task<bool> TryStartResearchAsync(string id)
    {
        var state = SaveService.Instance.Current.Progress.Research;

        var check = ResearchRules.CanStart(playerResearchTree, state, id, CurrencyManager.Instance.Get(CurrencyTypes.Cinders));

        if (check != ResearchStartCheck.Ok)
        {
            Debug.LogWarning($"Can't start research {id}: {check}");
            return false;
        }

        if (!ServerActions.IsOnline)
        {
            Toast.Show("Connect to the internet to start research.");
            return false;
        }

        int fromLevel = ResearchRules.GetLevel(state, id);

        var response = await ServerActions.SendAsync(module =>
            module.StartResearch(id, fromLevel, playerResearchTree.DefinitionsVersion));

        if (response == null)
        {
            Toast.Show("Couldn't reach the server. Try again.");
            return false;
        }

        if (!string.IsNullOrEmpty(response.Error))
        {
            Toast.Show(ServerActions.Describe(response.Error));
            return false;
        }

        var active = SaveService.Instance.Current.Progress.Research.ActiveResearch.Find(a => a.ResearchId == id);

        if (active == null)
            return false;

        OnResearchStarted?.Invoke(active.ResearchCategory);
        StartTimer(active);

        return true;
    }

    private void StartTimer(ActiveResearch active)
    {
        if (researchCoroutines.TryGetValue(active.ResearchId, out var running))
        {
            StopCoroutine(running);
        }

        researchCoroutines[active.ResearchId] = StartCoroutine(ResearchTimerRoutine(active));
    }

    private IEnumerator ResearchTimerRoutine(ActiveResearch active)
    {
        long endTime = ResearchRules.GetEndTime(playerResearchTree, active);

        while (ServerClock.Now < endTime)
        {
            long remaining = endTime - ServerClock.Now;

            OnResearchTimeUpdated?.Invoke(active.ResearchCategory, remaining);

            yield return new WaitForSeconds(1f);
        }

        // Final update to 0
        OnResearchTimeUpdated?.Invoke(active.ResearchCategory, 0);

        researchCoroutines.Remove(active.ResearchId);
        _ = CompleteOnServerAsync(active.ResearchId, active.ResearchCategory);
    }

    // Asks the server to complete the research. It checks the time on its own clock, so when this device's clock
    // is ahead (NotDone) or the server can't be reached, it asks again later. Offline it waits for a reconnect.
    private async Task CompleteOnServerAsync(string researchId, ResearchCategory category)
    {
        if (!completing.Add(researchId))
            return;

        try
        {
            float retrySeconds = FirstRetrySeconds;

            while (ServerActions.IsOnline)
            {
                var response = await ServerActions.SendAsync(module => module.CompleteResearch(researchId));

                if (this == null)
                    return;

                if (response != null && response.Error != ProgressResponse.NotDone)
                {
                    if (string.IsNullOrEmpty(response.Error))
                    {
                        PostUnlock(researchId, category);
                        OnResearchCompleted?.Invoke(category);
                    }

                    return;
                }

                await Task.Delay(TimeSpan.FromSeconds(retrySeconds));
                retrySeconds = Math.Min(retrySeconds * 2, MaxRetrySeconds);

                if (this == null)
                    return;
            }
        }
        finally
        {
            completing.Remove(researchId);
        }
    }

    // Finished research is completed on the server; the rest gets a timer
    private void RestartActiveResearchTimers()
    {
        long now = ServerClock.Now;

        foreach (var active in SaveService.Instance.Current.Progress.Research.ActiveResearch.ToList())
        {
            if (ResearchRules.IsDone(playerResearchTree, active, now))
            {
                _ = CompleteOnServerAsync(active.ResearchId, active.ResearchCategory);
            }
            else
            {
                StartTimer(active);
            }
        }
    }

    public long GetRemainingSeconds(ActiveResearch active)
    {
        long endTime = ResearchRules.GetEndTime(playerResearchTree, active);
        long now = ServerClock.Now;

        return Math.Max(0, endTime - now);
    }

    public float GetProgress(ActiveResearch active)
    {
        var def = playerResearchTree.GetResearchById(active.ResearchId);
        int duration = ResearchRules.GetDurationSeconds(def, active.TargetLevel);

        long now = ServerClock.Now;
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

    private void PostUnlock(string researchId, ResearchCategory category)
    {
        if (category == ResearchCategory.GlobalAbility)
        {
            if (UnlockService.Instance != null)
            {
                UnlockService.Instance.Unlock(researchId);
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
}