using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UnitMetadata))]
public abstract class BaseUnitStats : MonoBehaviour, IUnit, ITargetable
{
    [Header("Reference")]
    [SerializeField] protected GameObject unit;
    [SerializeField] protected FloatingHealthBar healthBar;
    [SerializeField] private UnitStatsDefinition stats;

    protected int currentHealth;
    protected UnitMetadata metadata;

    // IUnit
    public float AttackRange => stats.attackRange;
    public int AttackDamage => stats.attackDamage;
    public float AttackSpeed => stats.attackSpeed;
    public float MovementSpeed => stats.movementSpeed;
    public float CritChance => stats.critChance;
    public float CritMultiplier => stats.critMultiplier;
    public int MaxHealth => stats.maxHealth;
    public int Armor => stats.armor;

    // ITargetable
    public GameObject GameObject => gameObject;
    public Transform Transform => (this != null) ? transform : null;
    public float HitRadius => stats.hitRadius;
    public bool IsAlive => currentHealth > 0;
    public virtual ThreatLevel UnitPrio => stats.unitPrio;
    public bool IsTargetable => stats.isTargetable;

    // UnitMetadata
    public Team Team => metadata.Team;
    public float Cost => stats.cost;

    //Debug
#if UNITY_EDITOR
    [Header("Deubg Stats")]
    [SerializeField] private float debugCost;

    [SerializeField] private int debugMaxHealth;
    [SerializeField] private int debugCurrentHealth;
    [SerializeField] private int debugArmor;

    [SerializeField] private int debugAttackDamage;
    [SerializeField] private float debugAttackSpeed;
    [SerializeField] private float debugAttackRange;
    [SerializeField] private float debugHitRadius;

    [SerializeField] private float debugMovementSpeed;

    [SerializeField] private float debugCritChance;
    [SerializeField] private float debugCritMultiplier;

    [SerializeField] private ThreatLevel debugUnitPrio;
    [SerializeField] private bool debugIsTargetable;
#endif

    protected virtual void Awake()
    {
        metadata = GetComponent<UnitMetadata>();
        currentHealth = stats.maxHealth;
        healthBar = GetComponentInChildren<FloatingHealthBar>();

        if (stats == null)
        {
            Debug.LogError($"{name} has no UnitStatsDefinition assigned.", this);
        }
#if UNITY_EDITOR
        SyncDebugStats();
#endif
    }

    protected virtual void Start()
    {
        healthBar?.UpdateHealthBar(currentHealth, stats.maxHealth);
    }

    public virtual void TakeDamage(int amount)
    {
        currentHealth -= ApplyArmorReduction(amount);

#if UNITY_EDITOR
        SyncDebugStats();
#endif

        healthBar?.UpdateHealthBar(currentHealth, stats.maxHealth);
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private int ApplyArmorReduction(int dmg)
    {
        return (dmg - stats.armor);
    }

    public virtual void Die()
    {
        Destroy(unit != null ? unit : gameObject);
    }

    public virtual void ApplyFinalStats(FinalStats finalStats)
    {
        stats.maxHealth = finalStats.health;
        currentHealth = finalStats.health;

        stats.attackDamage = finalStats.attackDamage;
        stats.attackSpeed = finalStats.attackSpeed;
        stats.attackRange = finalStats.attackRange;
        stats.movementSpeed = finalStats.movementSpeed;
        stats.hitRadius = finalStats.hitRadius;
        stats.cost = finalStats.cost;
        stats.armor = finalStats.armor;
        stats.critChance = finalStats.critChance;
        stats.critMultiplier = finalStats.critMultiplier;

        healthBar?.UpdateHealthBar(currentHealth, stats.maxHealth);
    }

    public int GetAttackDamage()
    {
        int dmg = stats.attackDamage;

        if(stats.critChance > 0)
        {
            if(RollCrit())
            {
                dmg = Mathf.RoundToInt(dmg * stats.critMultiplier);
                ShowCritFeedback(dmg);
            }
        }

        return dmg;
    }

    protected bool RollCrit()
    {
        return Random.value <= stats.critChance;
    }

    protected virtual void ShowCritFeedback(int dmg)
    {
        // Incase i wanna add something to the UI later
        Debug.Log($"{name} landed a CRIT for {dmg} damage!");
    }


    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stats.attackRange);
    }
#if UNITY_EDITOR
    private void SyncDebugStats()
    {
        if (stats == null) return;

        debugCost = stats.cost;

        debugMaxHealth = stats.maxHealth;
        debugCurrentHealth = currentHealth;
        debugArmor = stats.armor;

        debugAttackDamage = stats.attackDamage;
        debugAttackSpeed = stats.attackSpeed;
        debugAttackRange = stats.attackRange;
        debugHitRadius = stats.hitRadius;

        debugMovementSpeed = stats.movementSpeed;

        debugCritChance = stats.critChance;
        debugCritMultiplier = stats.critMultiplier;

        debugUnitPrio = stats.unitPrio;
        debugIsTargetable = stats.isTargetable;
    }
#endif
}
