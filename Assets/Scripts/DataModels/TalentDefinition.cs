using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class TalentTree
{
    [JsonProperty("talents")]
    public Dictionary<string, TalentClassDefinition> TalentsByClass { get; set; }

    [JsonProperty("costPresets")]
    public Dictionary<string, TalentCostPreset> CostPresets { get; set; }

    public List<TalentDefinition> GetTalentsByClass(string className)
    {
        if (!TalentsByClass.TryGetValue(className, out TalentClassDefinition classDef))
        {
            throw new System.Exception($"Class {className} not found in talent tree");
        }
        return classDef.Talents;
    }

    public TalentDefinition GetTalentById(string id)
    {
        string className = id.Split("_")[0];
        return GetTalentsByClass(className).Find(unit => unit.Id == id);
    }

    public TalentCostModel GetCostModel(string presetName, int tier)
    {
        if(!CostPresets.TryGetValue(presetName, out TalentCostPreset preset))
        {
            throw new System.Exception($"Cost preset {presetName} not found");
        }

        if(!preset.Tiers.TryGetValue(tier.ToString(), out TalentCostModel costModel))
        {
            throw new System.Exception($"Tier {tier} not found in preset {presetName}");
        }

        return costModel;
    }
}

public class TalentClassDefinition
{
    public string CostPreset;
    public List<TalentDefinition> Talents;
}

public class TalentDefinition
{
    public string Id;                        // unique id
    public string IconId;
    public string Name;
    public string Description;

    public TalentCategory Category;         // UnitUpgrade, TowerUpgrade, GlobalUpgrade
    public TalentType Type;                 // StatModifier, AbilityUnlock, UnitUnlock, Income, etc.
    public int Tier;

    public StatEffect[] Effects;      // one upgrade may have multiple effects
    public Unlock[] Unlocks;          // Can be units, abilities or unit skills

    public TalentPurchaseModel Purchase;    // handles max, infinite, etc.

    public TalentPrerequisite[] Prerequisites;

    [JsonIgnore]
    public TalentCostModel Cost;            // built from presets on load

    public int GetCurrentCost()
    {
        float baseCost = Cost.BaseCost;      // e.g. 100
        float multiplier = Cost.CostMultiplier; // e.g. 1.5
        int purchased = Purchase.Purchased;
        
        return Mathf.FloorToInt(baseCost + (baseCost * multiplier * purchased));
    }
}

public enum TalentCategory
{
    Unit,
    Tower,
    Global
}

public enum TalentType
{
    StatModifier,       // + HP, + Damage, multipliers, flat
    AbilityUnlock,      // unlock crit, splash, poison
    UnitUnlock,         // unlock new unit
    TowerUnlock,        // unlock new tower
    Income,             // increase income tick
}
public class TalentCostPreset
{
    public Dictionary<string, TalentCostModel> Tiers { get; set; }
}

public class TalentCostModel
{
    public float BaseCost;
    public float CostMultiplier; // e.g 1.5
}

public class TalentPurchaseModel
{
    public int Purchased;
    public int MaxPurchases;  // 0 = infinite
}

public class TalentPrerequisite
{
    public string RequiredUpgradeId; 

    public int RequiredTier;
    public int RequiredPointsInTier; 

    public string RequiredAchievementId; // e.g “KillBoss5”
}
