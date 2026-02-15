using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UnitMetadata))]
public abstract class BaseUnitStats : MonoBehaviour, IUnit, ITargetable
{
    [Header("Reference")]
    [SerializeField] protected GameObject unit;
    [SerializeField] protected FloatingHealthBar healthBar;
    [SerializeField] private UnitStatsDefinition baseStats;

    protected UnitStatsDefinition BaseStats => baseStats;

    protected int currentHealth;
    protected UnitMetadata metadata;
    private RuntimeStats runtimeStats;

    // IUnit
    public float AttackRange => runtimeStats.attackRange;
    public int AttackDamage => runtimeStats.attackDamage;
    public float AttackSpeed => runtimeStats.attackSpeed;
    public float MovementSpeed => runtimeStats.movementSpeed;
    public float CritChance => runtimeStats.critChance;
    public float CritMultiplier => runtimeStats.critMultiplier;
    public int MaxHealth => runtimeStats.maxHealth;
    public int Armor => runtimeStats.armor;

    // ITargetable
    public GameObject GameObject => gameObject;
    public Transform Transform => (this != null) ? transform : null;
    public float HitRadius => runtimeStats.hitRadius;
    public bool IsAlive => currentHealth > 0;
    public virtual ThreatLevel UnitPrio => runtimeStats.unitPrio;
    public bool IsTargetable => runtimeStats.isTargetable;

    // UnitMetadata
    public Team Team => metadata.Team;
    public float Cost => baseStats.cost;

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
        runtimeStats = new RuntimeStats
        {
            maxHealth = baseStats.maxHealth,
            attackDamage = baseStats.attackDamage,
            armor = baseStats.armor,
            attackSpeed = baseStats.attackSpeed,
            movementSpeed = baseStats.movementSpeed,
            attackRange = baseStats.attackRange,
            cost = baseStats.cost,
            hitRadius = baseStats.hitRadius,
            critChance = baseStats.critChance,
            critMultiplier = baseStats.critMultiplier,
            unitPrio = baseStats.unitPrio,
            isTargetable = baseStats.isTargetable
        };

        metadata = GetComponent<UnitMetadata>();
        currentHealth = runtimeStats.maxHealth;
        healthBar = GetComponentInChildren<FloatingHealthBar>();

        if (runtimeStats == null)
        {
            Debug.LogError($"{name} has no runtimeStats assigned.", this);
        }
#if UNITY_EDITOR
        SyncDebugStats();
#endif
    }

    protected virtual void Start()
    {
        healthBar?.UpdateHealthBar(currentHealth, runtimeStats.maxHealth);
    }

    public virtual void TakeDamage(int amount)
    {
        currentHealth -= ApplyArmorReduction(amount);

#if UNITY_EDITOR
        SyncDebugStats();
#endif

        healthBar?.UpdateHealthBar(currentHealth, runtimeStats.maxHealth);
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private int ApplyArmorReduction(int dmg)
    {
        return Mathf.Max(1, dmg - runtimeStats.armor);
    }

    public virtual void Die()
    {
        Destroy(unit != null ? unit : gameObject);
    }

    public virtual void ApplyFinalStats(FinalStats finalStats)
    {
        runtimeStats.maxHealth = finalStats.maxHealth;
        currentHealth = finalStats.maxHealth;

        runtimeStats.attackDamage = finalStats.attackDamage;
        runtimeStats.attackSpeed = finalStats.attackSpeed;
        runtimeStats.attackRange = finalStats.attackRange;
        runtimeStats.movementSpeed = finalStats.movementSpeed;
        runtimeStats.hitRadius = finalStats.hitRadius;
        runtimeStats.cost = finalStats.cost;
        runtimeStats.armor = finalStats.armor;
        runtimeStats.critChance = finalStats.critChance;
        runtimeStats.critMultiplier = finalStats.critMultiplier;

        healthBar?.UpdateHealthBar(currentHealth, runtimeStats.maxHealth);

#if UNITY_EDITOR
        SyncDebugStats();
#endif
    }

    public int GetAttackDamage()
    {
        int dmg = runtimeStats.attackDamage;

        if(runtimeStats.critChance > 0)
        {
            if(RollCrit())
            {
                dmg = Mathf.RoundToInt(dmg * runtimeStats.critMultiplier);
                ShowCritFeedback(dmg);
            }
        }

        return dmg;
    }

    protected bool RollCrit()
    {
        return Random.value <= runtimeStats.critChance;
    }

    protected virtual void ShowCritFeedback(int dmg)
    {
        // Incase i wanna add something to the UI later
        Debug.Log($"{name} landed a CRIT for {dmg} damage!");
    }


    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, baseStats.attackRange);
    }

#if !UNITY_EDITOR
    //TODO create world bounds instead.
    private void OnBecameInvisible()
    {
        Destroy(gameObject);
    }
#endif

#if UNITY_EDITOR
    private void SyncDebugStats()
    {
        if (runtimeStats == null) return;

        debugCost = runtimeStats.cost;

        debugMaxHealth = runtimeStats.maxHealth;
        debugCurrentHealth = currentHealth;
        debugArmor = runtimeStats.armor;

        debugAttackDamage = runtimeStats.attackDamage;
        debugAttackSpeed = runtimeStats.attackSpeed;
        debugAttackRange = runtimeStats.attackRange;
        debugHitRadius = runtimeStats.hitRadius;

        debugMovementSpeed = runtimeStats.movementSpeed;

        debugCritChance = runtimeStats.critChance;
        debugCritMultiplier = runtimeStats.critMultiplier;

        debugUnitPrio = runtimeStats.unitPrio;
        debugIsTargetable = runtimeStats.isTargetable;
    }
#endif
}
