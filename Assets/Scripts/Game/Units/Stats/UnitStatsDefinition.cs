using UnityEngine;

[CreateAssetMenu(menuName = "Units/Unit Stats")]
public class UnitStatsDefinition : ScriptableObject
{
    [Header("Economy")]
    public float cost;

    [Header("Health")]
    public int maxHealth;
    public int armor;

    [Header("Combat")]
    public int attackDamage;
    public float attackSpeed;
    public float attackRange;
    public float hitRadius;
    public float splashRadius;

    [Header("Movement")]
    public float movementSpeed;
    public int mass;

    [Header("Crit")]
    [Range(0f, 1f)] public float critChance;
    public float critDamage;

    [Header("Targeting")]
    public ThreatLevel unitPrio;
    public bool isTargetable;

    [Header("Category")]
    public ResearchCategory category;

    [Header("Healthbar")]
    public Vector3 healthbarScale;

    public StatBlock ToStatBlock()
    {
        var block = new StatBlock();
        block.Set(StatType.Health, maxHealth);
        block.Set(StatType.AttackDamage, attackDamage);
        block.Set(StatType.AttackSpeed, attackSpeed);
        block.Set(StatType.AttackRange, attackRange);
        block.Set(StatType.Armor, armor);
        block.Set(StatType.CritChance, critChance);
        block.Set(StatType.CritDamage, critDamage);
        block.Set(StatType.SplashRadius, splashRadius);
        block.Set(StatType.MovementSpeed, movementSpeed);
        block.Set(StatType.Mass, mass);
        block.Set(StatType.HitRadius, hitRadius);
        block.Set(StatType.Cost, cost);
        return block;
    }
}
