// Stats live in a StatBlock; the named properties are kept until callers move to Stats.Get/Set.
public class FinalStats
{
    public StatBlock Stats { get; }

    public FinalStats() : this(new StatBlock()) { }

    public FinalStats(StatBlock stats)
    {
        Stats = stats;
    }

    public int maxHealth { get => Stats.GetInt(StatType.Health); set => Stats.Set(StatType.Health, value); }
    public int attackDamage { get => Stats.GetInt(StatType.AttackDamage); set => Stats.Set(StatType.AttackDamage, value); }
    public int armor { get => Stats.GetInt(StatType.Armor); set => Stats.Set(StatType.Armor, value); }
    public float attackSpeed { get => Stats.Get(StatType.AttackSpeed); set => Stats.Set(StatType.AttackSpeed, value); }
    public float movementSpeed { get => Stats.Get(StatType.MovementSpeed); set => Stats.Set(StatType.MovementSpeed, value); }
    public int mass { get => Stats.GetInt(StatType.Mass); set => Stats.Set(StatType.Mass, value); }
    public float attackRange { get => Stats.Get(StatType.AttackRange); set => Stats.Set(StatType.AttackRange, value); }
    public float critChance { get => Stats.Get(StatType.CritChance); set => Stats.Set(StatType.CritChance, value); }
    public float critDamage { get => Stats.Get(StatType.CritDamage); set => Stats.Set(StatType.CritDamage, value); }
    public float splashRadius { get => Stats.Get(StatType.SplashRadius); set => Stats.Set(StatType.SplashRadius, value); }

    // Not modifiable stats (not in StatType)
    public float cost;
    public float hitRadius;
}
