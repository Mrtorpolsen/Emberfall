[System.Serializable]
public class StatEffect
{
    public StatType Target;      // Health, Damage, AttackSpeed, AbilityName, Income, etc.
    public EffectOperation Operation; // Add, Subtract, Multiply, Set
    public float Value;               // 1.05 or +5 etc.
}

public enum StatType
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
}

public enum EffectOperation
{
    Add,
    Subtract,
    Multiply,
    Set
}
