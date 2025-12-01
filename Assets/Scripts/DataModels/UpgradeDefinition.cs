using System.Collections.Generic;

public class UpgradeDefinition
{
    public string Id;                        // unique id
    public string Name;
    public string Description;

    public UpgradeCategory Category;         // UnitUpgrade, TowerUpgrade, GlobalUpgrade
    public UpgradeType Type;                 // StatModifier, AbilityUnlock, UnitUnlock, Income, etc.

    public string TargetId;                  // "Fighter", "Ranger", "Tower_Archer", null for globals

    public List<UpgradeEffect> Effects;      // one upgrade may have multiple effects

    public UpgradeCostModel Cost;            // dynamic cost scaling
    public UpgradePurchaseModel Purchase;    // handles max, infinite, etc.

    public List<UpgradePrerequisite> Prerequisites;
}

public enum UpgradeCategory
{
    Unit,
    Tower,
    Global
}

public enum UpgradeType
{
    StatModifier,       // + HP, + Damage, multipliers, flat
    AbilityUnlock,      // unlock crit, splash, poison
    UnitUnlock,         // unlock new unit
    TowerUnlock,        // unlock new tower
    Income,             // increase income tick
}

public class UpgradeEffect
{
    public EffectTarget Target;      // Health, Damage, AttackSpeed, AbilityName, Income, etc.
    public EffectOperation Operation; // Add, Multiply, Set
    public float Value;               // 1.05 or +5 etc.
}

public enum EffectTarget
{
    Health,
    Damage,
    AttackSpeed,
    CritChance,
    CritDamage,
    SplashRadius,
    Income,
    Ability,       // e.g "Poison"
    UnlockUnit,    // special case but fits well
    UnlockTower,
}

public enum EffectOperation
{
    Add,
    Multiply,
    Set
}

public class UpgradeCostModel
{
    public float BaseCost;
    public float CostMultiplier; // e.g 1.5
}

public class UpgradePurchaseModel
{
    public int Purchased;
    public int MaxPurchases;  // 0 = infinite
}

public class UpgradePrerequisite
{
    public string RequiredUpgradeId; 

    public int RequiredTier;
    public int RequiredPointsInTier; 

    public string RequiredAchievementId; // e.g “KillBoss5”
}
