using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class TalentService : MonoBehaviour
{
    public static TalentService Instance { get; private set; }
    [System.NonSerialized] public TalentTree playerTalentTree;

    private const string TALENTS_ADDRESSABLE = "Talents";

    private List<(string unitId, string talentId)> unlockTalents = new List<(string unitId, string talentId)>();

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
            // The server's definitions when they're newer: those are the costs it will charge
            bool useServer = DefinitionsSelector.PreferServer(jsonAsset.text, ServerDefinitions.TalentsJson);
            string json = useServer ? ServerDefinitions.TalentsJson : jsonAsset.text;
            Debug.Log($"Talents definitions: {(useServer ? "server" : "bundled")} copy");
            var tree = JsonConvert.DeserializeObject<TalentTree>(json);
            playerTalentTree = tree;

            playerTalentTree.BuildTalentsByUnit();
            CollectUnlockTalents();

            ProvideUnlocked();
        }
        finally
        {
            Addressables.Release(handle);
        }
    }

    private void CollectUnlockTalents()
    {
        unlockTalents.Clear();

        foreach (var unitEntry in playerTalentTree.TalentsByUnit)
        {
            foreach (var talent in unitEntry.Value)
            {
                if (talent.IsUnlockType)
                {
                    unlockTalents.Add((unitEntry.Key, talent.Id));
                }
            }
        }
    }

    public void SetTutorialTalentState()
    {
        string unitName = "fighter";
        string talentId = "stat_health_1_T1";
        var purchases = SaveService.Instance.Current.Progress.Talents.Purchases;

        if (!purchases.TryGetValue(unitName, out var unitPurchases))
        {
            unitPurchases = new UnitSaveData
            {
                PurchasedTalents = new Dictionary<string, int>()
            };

            purchases[unitName] = unitPurchases;
        }

        if (unitPurchases.PurchasedTalents.TryGetValue(talentId, out var count))
        {
            unitPurchases.PurchasedTalents[talentId] = 0;
        }
    }

    private void ProvideUnlocked()
    {
        if (UnlockService.Instance == null)
        {
            Debug.LogError("UnlockService is null. Cannot provide talent unlocks.");
            return;
        }

        foreach (var talent in unlockTalents)
        {
            if (GetPurchasedTalent(talent.unitId, talent.talentId) > 0)
            {
                UnlockService.Instance.Unlock(talent.talentId);
            }
        }
    }

    // Gives back all currency spent on talents and re-locks what talents unlocked, removing those from loadouts
    // The server gives the Cinders back and clears the talents (docs/cloud-save.md). False when it couldn't.
    public async Task<bool> RefundAllAsync()
    {
        if (!ServerActions.IsOnline)
        {
            Toast.Show("Connect to the internet to refund talents.");
            return false;
        }

        var response = await ServerActions.SendAsync(module => module.RefundTalents());

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

        // Only talent unlocks: research and default unlocks stay
        foreach (var talent in unlockTalents)
        {
            UnlockService.Instance.Lock(talent.talentId);
        }

        LoadoutService.Instance.RemoveLockedDefinitions();

        await SaveService.Instance.SaveAsync();
        return true;
    }

    public bool CanAfford(int cost)
    {
        return cost <= CurrencyManager.Instance.Get(CurrencyTypes.Cinders);
    }

    // Buys the next level on the server (it checks and charges; docs/cloud-save.md) and shows the progress it returns
    public async Task<TalentPurchaseResult> TryPurchaseAsync(string unitName, Talent talent)
    {
        int max = talent.Purchase.MaxPurchases;

        //Repull live state
        int owned = GetPurchasedTalent(unitName, talent.Id);

        var check = TalentRules.CanPurchase(playerTalentTree, SaveService.Instance.Current.Progress.Talents, unitName, talent.Id,
            CurrencyManager.Instance.Get(CurrencyTypes.Cinders));

        if (check != TalentPurchaseCheck.Ok)
        {
            Debug.LogWarning($"Can't buy talent {talent.Id} for {unitName}: {check}");
            return TalentPurchaseResult.Failed(owned);
        }

        if (!ServerActions.IsOnline)
        {
            Toast.Show("Connect to the internet to buy talents.");
            return TalentPurchaseResult.Failed(owned);
        }

        var response = await ServerActions.SendAsync(module =>
            module.BuyTalent(unitName, talent.Id, owned, playerTalentTree.DefinitionsVersion));

        if (response == null)
        {
            Toast.Show("Couldn't reach the server. Try again.");
            return TalentPurchaseResult.Failed(owned);
        }

        owned = GetPurchasedTalent(unitName, talent.Id);

        if (!string.IsNullOrEmpty(response.Error))
        {
            Toast.Show(ServerActions.Describe(response.Error));
            return TalentPurchaseResult.Failed(owned);
        }

        if (talent.IsUnlockType)
        {
            UnlockService.Instance.Unlock(talent.Id);
        }

        bool canBuyMore = owned < max && CanAfford(talent.GetCurrentCost());

        return new TalentPurchaseResult(true, owned, canBuyMore);
    }

    public int GetPurchasedTalent(string unitName, string talentId)
    {
        return TalentRules.GetOwned(SaveService.Instance.Current.Progress.Talents, unitName, talentId);
    }

    public bool ArePrerequisitesMet(string unitName, Talent talent)
    {
        return TalentRules.ArePrerequisitesMet(playerTalentTree, SaveService.Instance.Current.Progress.Talents, unitName, talent.Prerequisites);
    }
}

public readonly struct TalentPurchaseResult
{
    public readonly bool Success;
    public readonly int Owned;
    public readonly bool CanBuyMore;

    public TalentPurchaseResult(bool success, int owned, bool canBuyMore)
    {
        Success = success;
        Owned = owned;
        CanBuyMore = canBuyMore;
    }

    public static TalentPurchaseResult Failed(int owned) => new TalentPurchaseResult(false, owned, false);
}
