using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class TalentService : MonoBehaviour
{
    public static TalentService Instance { get; private set; }
    public TalentTree playerTalentTree;

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
            var tree = JsonConvert.DeserializeObject<TalentTree>(jsonAsset.text);
            playerTalentTree = tree;

            BuildTalentsByUnit(playerTalentTree);

            ProvideUnlocked();
        }
        finally
        {
            Addressables.Release(handle);
        }
    }

    private void BuildTalentsByUnit(TalentTree talentTree)
    {
        unlockTalents.Clear();
        talentTree.TalentsByUnit.Clear();

        foreach (var unitEntry in talentTree.UnitDefinitions)
        {
            string unitName = unitEntry.Key;
            UnitDefinition unitDef = unitEntry.Value;

            var tempList = new List<Talent>();

            var idCounts = new Dictionary<string, int>();

            foreach (var talentNode in unitDef.Talents)
            {
                TalentOverride archetypeOverride = talentTree.GetArchetypeOverride(unitDef.Archetype, talentNode.DefinitionId);
                TalentData talentData = talentTree.GetTalentData(talentNode.DefinitionId);


                string talentId = talentNode.DefinitionId;

                if (talentData.Type == TalentType.AbilityUnlock ||
                    talentData.Type == TalentType.UnitUnlock ||
                    talentData.Type == TalentType.TowerUnlock)
                {
                    unlockTalents.Add((unitName, talentNode.DefinitionId));
                } 
                else
                {
                    //For talents with the same definition ID and tier, we need to create unique IDs for each instance of the talent. We can do this by appending a count to the base ID.
                    string baseId = $"{talentNode.DefinitionId}_T{talentNode.Tier}";

                    if (!idCounts.TryAdd(baseId, 0))
                    {
                        idCounts[baseId]++;
                    }

                    talentId = idCounts[baseId] == 0 ? baseId : $"{baseId}_{idCounts[baseId]}";
                }

                var talent = new Talent
                {
                    Id = talentId,
                    IconId = archetypeOverride.IconId,
                    Name = archetypeOverride.Name,
                    Description = talentData.Description,
                    Category = talentNode.Category,
                    Type = talentData.Type,
                    Tier = talentNode.Tier,
                    Effects = talentData.Effects,
                    Unlocks = talentData.Unlocks,
                    Purchase = talentData.Purchase,
                    Prerequisites = talentNode.Prerequisites,
                    Cost = talentTree.GetCostModel(unitDef.CostPreset, talentNode.Tier)
                };

                tempList.Add(talent);
            }

            talentTree.TalentsByUnit.Add(unitName, tempList);
        }
    }

    public void AddTalent(string unitName, string talentId)
    {
        var purchases = SaveService.Instance.Current.Talents.Purchases;

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
            unitPurchases.PurchasedTalents[talentId] = count + 1;
        }
        else
        {
            unitPurchases.PurchasedTalents[talentId] = 1;
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

    public int GetPurchasedTalent(string unitName, string talentId)
    {
        if (SaveService.Instance.Current.Talents.Purchases.TryGetValue(unitName, out var unitPurchase) &&
            unitPurchase.PurchasedTalents.TryGetValue(talentId, out var count))
        {
            return count;
        }

        return 0;
    }
}
