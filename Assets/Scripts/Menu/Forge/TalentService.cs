using Newtonsoft.Json;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class TalentService : MonoBehaviour
{
    public static TalentService Instance { get; private set; }
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

            BuildTalentsByUnit(playerTalentTree);
        }
        finally
        {
            Addressables.Release(handle);
        }
    }

    private void BuildTalentsByUnit(TalentTree talentTree)
    {
        foreach (var unitEntry in talentTree.UnitDefinitions)
        {
            string unitName = unitEntry.Key;
            UnitDefinition unitDef = unitEntry.Value;

            var tempList = new List<Talent>();

            var idCounts = new Dictionary<string, int>();

            foreach (var talentNode in unitDef.Talents)
            {
                var archetypeOverride = talentTree.GetArchetypeOverride(unitDef.Archetype, talentNode.DefinitionId);
                var talentData = talentTree.GetTalentData(talentNode.DefinitionId);

                //For talents with the same definition ID and tier, we need to create unique IDs for each instance of the talent. We can do this by appending a count to the base ID.
                var baseId = $"{talentNode.DefinitionId}_T{talentNode.Tier}";

                if (!idCounts.TryAdd(baseId, 0))
                {
                    idCounts[baseId]++;
                }

                var talentId = idCounts[baseId] == 0 ? baseId : $"{baseId}_{idCounts[baseId]}";

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
