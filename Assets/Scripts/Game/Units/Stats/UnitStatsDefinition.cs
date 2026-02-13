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

    [Header("Movement")]
    public float movementSpeed;

    [Header("Crit")]
    [Range(0f, 1f)] public float critChance;
    public float critMultiplier;

    [Header("Targeting")]
    public ThreatLevel unitPrio;
    public bool isTargetable;
}
