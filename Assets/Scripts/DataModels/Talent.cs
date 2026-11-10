#nullable disable // Unity code is nullable-oblivious; the Cloud Code module compiles this file with nullable on
using Newtonsoft.Json;
using System.Collections.Generic;

// Talent definitions (Talents.json). Shared with the Cloud Code module (CloudCode/EmberfallModule), which
// validates purchases on the server, so keep this plain C#: no UnityEngine.

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

    // Builds TalentsByUnit from the unit definitions. Purchases are saved under these IDs, so the client and
    // the server must derive them the same way: unlock talents use their DefinitionId, every other talent
    // "<DefinitionId>_T<tier>", with "_1", "_2", ... appended for repeats of the same definition and tier.
    public void BuildTalentsByUnit()
    {
        TalentsByUnit.Clear();

        foreach (var unitEntry in UnitDefinitions)
        {
            string unitName = unitEntry.Key;
            UnitDefinition unitDef = unitEntry.Value;

            var talents = new List<Talent>();
            var idCounts = new Dictionary<string, int>();

            foreach (var talentNode in unitDef.Talents)
            {
                TalentOverride archetypeOverride = GetArchetypeOverride(unitDef.Archetype, talentNode.DefinitionId);
                TalentData talentData = GetTalentData(talentNode.DefinitionId);

                string talentId = talentNode.DefinitionId;

                if (!Talent.IsUnlock(talentData.Type))
                {
                    string baseId = $"{talentNode.DefinitionId}_T{talentNode.Tier}";

                    if (!idCounts.TryAdd(baseId, 0))
                    {
                        idCounts[baseId]++;
                    }

                    talentId = idCounts[baseId] == 0 ? baseId : $"{baseId}_{idCounts[baseId]}";
                }

                talents.Add(new Talent
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
                    Cost = GetCostModel(unitDef.CostPreset, talentNode.Tier)
                });
            }

            TalentsByUnit.Add(unitName, talents);
        }
    }

    public List<Talent> GetTalentsByClass(string unitName)
    {
        if (!TalentsByUnit.TryGetValue(unitName, out List<Talent> talents))
        {
            throw new System.Exception($"Class {unitName} not found in talent tree");
        }
        return talents;
    }

    // Null if the class or talent no longer exists, e.g. a save with purchases for a removed unit
    public Talent GetTalentById(string className, string talentId)
    {
        if (!TalentsByUnit.TryGetValue(className, out List<Talent> talents))
            return null;

        return talents.Find(talent => talent.Id == talentId);
    }

    public TalentCostModel GetCostModel(CostPreset presetName, int tier)
    {
        if (!CostPresets.TryGetValue(presetName, out TalentCostPreset preset))
        {
            throw new System.Exception($"Cost preset {presetName} not found");
        }

        if (!preset.Tiers.TryGetValue(tier.ToString(), out TalentCostModel costModel))
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
    public string Id;                        // comp id of DefinitionId + Tier
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

    public bool IsUnlockType => IsUnlock(Type);

    public static bool IsUnlock(TalentType type) =>
        type == TalentType.UnitUnlock ||
        type == TalentType.AbilityUnlock ||
        type == TalentType.TowerUnlock;

    // Flat: every level of a talent costs its tier's BaseCost (CostMultiplier is not used)
    public int GetCurrentCost()
    {
        return (int)System.Math.Floor(Cost.BaseCost);
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
    public float CostMultiplier; // Not used: talent costs are flat per tier
}

public class TalentPurchaseModel
{
    public int MaxPurchases;  // 0 = can't be bought
}

public class TalentPrerequisite
{
    public string RequiredUpgradeId;

    public int RequiredTier;
    public int RequiredPointsInTier;

    public string RequiredAchievementId; // e.g "KillBoss5"
}

public class TalentOverride
{
    public string Name;
    public string IconId;
}
