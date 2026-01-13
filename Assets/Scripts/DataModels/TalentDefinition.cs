using System.Collections.Generic;
using Newtonsoft.Json;

public class TalentTree
{
    [JsonProperty("talents")]
    public Dictionary<string, List<TalentDefinition>> TalentsByClass { get; set; }

    public List<TalentDefinition> GetTalentsByClass(string className)
    {
        TalentsByClass.TryGetValue(className.ToLower(), out var talents);
        return talents;
    }
    public TalentDefinition GetTalentById(string id)
    {
        string className = id.Split("_")[0];

        return GetTalentsByClass(className).Find(unit => unit.Id == id);
    }
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

    public List<TalentEffect> Effects;      // one upgrade may have multiple effects

    public TalentCostModel Cost;            // dynamic cost scaling
    public TalentPurchaseModel Purchase;    // handles max, infinite, etc.

    public List<TalentPrerequisite> Prerequisites;

    public float GetCurrentCost()
    {
        float baseCost = Cost.BaseCost;      // e.g. 100
        float multiplier = Cost.CostMultiplier; // e.g. 1.5
        int purchased = Purchase.Purchased;

        return baseCost + (baseCost * multiplier * purchased);
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

public class TalentEffect
{
    public EffectTarget Target;      // Health, Damage, AttackSpeed, AbilityName, Income, etc.
    public EffectOperation Operation; // Add, Multiply, Set
    public float Value;               // 1.05 or +5 etc.
}

public enum EffectTarget
{
    Health,
    Damage,
    Armor,
    AttackSpeed,
    CritChance,
    CritDamage,
    SplashRadius,
    Range,
    Income,
    //Ability
    ShieldSlam,
    Multishot,
    LanceStrike,
    //Units
    Berserker,
    Guardian,
    Sniper,
    Trapper,
    Templar,
    Dragoon,
}

public enum EffectOperation
{
    Add,
    Multiply,
    Set
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
