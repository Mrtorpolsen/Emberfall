[System.Serializable]
public class AppliedEffect
{
    public EffectTarget Target;      // Health, Damage, AttackSpeed, AbilityName, Income, etc.
    public EffectOperation Operation; // Add, Subtract, Multiply, Set
    public float Value;               // 1.05 or +5 etc.
}

public enum EffectTarget
{
    Health,
    AttackDamage,
    AttackSpeed,
    AttackRange,
    Armor,
    CritChance,
    CritDamage,
    SplashRadius,
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
    //Global
    Restoration,
    Fortify
}

public enum EffectOperation
{
    Add,
    Subtract,
    Multiply,
    Set
}
