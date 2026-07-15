using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class TalentTree
{
    [JsonProperty("costPresets")]
    public Dictionary<CostPreset, TalentCostPreset> CostPresets { get; set; }

    [JsonProperty("unitDefinitions")]
    public Dictionary<string, UnitDefinition> UnitDefinitions { get; set; }

    [JsonProperty("talentData")]
    public Dictionary<string, TalentData> TalentData { get; set; }

    [JsonProperty("archetypes")]
    public Dictionary<Archetype, Dictionary<string, TalentOverride>> ArchetypeOverrides { get; set; }

    public Dictionary<string, List<Talent>> TalentsByUnit { get; set; } = new();

    public List<Talent> GetTalentsByClass(string unitName)
    {
        if (!TalentsByUnit.TryGetValue(unitName, out List<Talent> talents))
        {
            throw new System.Exception($"Class {unitName} not found in talent tree");
        }
        return talents; 
    }

    public Talent GetTalentById(string className, string talentId)
    {
        return GetTalentsByClass(className).Find(unit => unit.Id == talentId);
    }

    public TalentCostModel GetCostModel(CostPreset presetName, int tier)
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

    public TalentOverride GetArchetypeOverride(Archetype archetype, string talentId)
    {
        TalentOverride talentOverride;

        if (ArchetypeOverrides.TryGetValue(archetype, out var overrides) &&
            overrides.TryGetValue(talentId, out talentOverride))
        {
            return talentOverride;
        }

        if (ArchetypeOverrides.TryGetValue(Archetype.Default, out var defaultOverrides) &&
            defaultOverrides.TryGetValue(talentId, out talentOverride))
        {
            return talentOverride;
        }

        throw new System.Exception($"Override for {talentId} not found in archetype {archetype} or default");
    }

    public TalentData GetTalentData(string talentId)
    {
        if (!TalentData.TryGetValue(talentId, out TalentData data))
        {
            throw new System.Exception($"Talent data for {talentId} not found");
        }
        return data;
    }
}

public class Talent
{
    public string Id;                        // unique id
    public string IconId;
    public string Name;
    public string Description;

    public TalentCategory Category;         // UnitUpgrade, TowerUpgrade, GlobalUpgrade
    public TalentType Type;                 // StatModifier, AbilityUnlock, UnitUnlock, Income, etc.
    public int Tier;

    public List<StatEffect> Effects;      // one upgrade may have multiple effects
    public List<Unlock> Unlocks;          // Can be units, abilities or unit skills

    public TalentPurchaseModel Purchase;    // handles max, infinite, etc.

    public TalentPrerequisite[] Prerequisites;

    public TalentCostModel Cost;            // built from presets on load

    public int GetCurrentCost()
    {
        float baseCost = Cost.BaseCost;      // e.g. 100
        float multiplier = Cost.CostMultiplier; // e.g. 1.5
        int purchased = Purchase.Purchased;
        
        return Mathf.FloorToInt(baseCost + (baseCost * multiplier * purchased));
    }
}

public class UnitDefinition
{
    public string IconId;
    public CostPreset CostPreset;
    public Archetype Archetype;
    public List<UnitTags> Tags;
    public List<TalentNodeData> Talents;
}

public class TalentData
{
    public string Description;
    public TalentType Type;
    public List<StatEffect> Effects;
    public List<Unlock> Unlocks;
    public TalentPurchaseModel Purchase;
}

public class TalentNodeData
{
    public string DefinitionId;
    public TalentCategory Category;
    public int Tier;
    public TalentPrerequisite[] Prerequisites;
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

public enum Archetype
{
    Default,
    Melee,
    Ranged,
}

public enum CostPreset
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}

public enum UnitTags
{
    None = 0, //Nothing for now
}

public class TalentCostPreset
{
    public Dictionary<string, TalentCostModel> Tiers;
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

public class TalentOverride
{
    public string Name;
    public string IconId;
}
