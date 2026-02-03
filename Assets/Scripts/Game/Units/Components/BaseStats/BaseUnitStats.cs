using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UnitMetadata))]
public abstract class BaseUnitStats : MonoBehaviour, IUnit, ITargetable
{
    [Header("Reference")]
    [SerializeField] protected GameObject unit;
    [SerializeField] protected FloatingHealthBar healthBar;

    [Header("Attributes")]
    [SerializeField] protected float cost;
    [SerializeField] protected int maxHealth;
    [SerializeField] protected int currentHealth;
    [SerializeField] protected int attackDamage;
    [SerializeField] protected float attackSpeed;
    [SerializeField] protected float attackRange;
    [SerializeField] protected float hitRadius;
    [SerializeField] protected float movementSpeed;
    [SerializeField] protected int armor;
    [SerializeField] protected float critChance;
    [SerializeField] protected float critMultiplier;
    [SerializeField] protected ThreatLevel unitPrio;
    [SerializeField] protected bool isTargetable;

    // IUnit
    public float AttackRange => attackRange;
    public int AttackDamage => attackDamage;
    public float AttackSpeed => attackSpeed;
    public float MovementSpeed => movementSpeed;
    public float CritChance => critChance;
    public float CritMultiplier => critMultiplier;
    public int MaxHealth => maxHealth;
    public int Armor => armor;

    // ITargetable
    public GameObject GameObject => gameObject;
    public Transform Transform => (this != null) ? transform : null;
    public float HitRadius => hitRadius;
    public bool IsAlive => currentHealth > 0;
    public virtual ThreatLevel UnitPrio => unitPrio;
    public bool IsTargetable => isTargetable;

    // UnitMetadata
    public Team Team => metadata.Team;
    public float Cost => cost;


    protected UnitMetadata metadata;

    protected virtual void Awake()
    {
        metadata = GetComponent<UnitMetadata>();
        currentHealth = maxHealth;
        healthBar = GetComponentInChildren<FloatingHealthBar>();
    }

    protected virtual void Start()
    {
        healthBar?.UpdateHealthBar(currentHealth, maxHealth);
    }

    public virtual void TakeDamage(int amount)
    {
        currentHealth -= ApplyArmorReduction(amount);
        healthBar?.UpdateHealthBar(currentHealth, maxHealth);
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private int ApplyArmorReduction(int dmg)
    {
        return (dmg - armor);
    }

    public virtual void Die()
    {
        Destroy(unit != null ? unit : gameObject);
    }

    public virtual void ApplyFinalStats(FinalStats stats)
    {
        maxHealth = stats.health;
        currentHealth = stats.health;

        attackDamage = stats.attackDamage;
        attackSpeed = stats.attackSpeed;
        attackRange = stats.attackRange;
        movementSpeed = stats.movementSpeed;
        hitRadius = stats.hitRadius;
        cost = stats.cost;
        armor = stats.armor;
        critChance = stats.critChance;
        critMultiplier = stats.critMultiplier;

        healthBar?.UpdateHealthBar(currentHealth, maxHealth);
    }

    public int GetAttackDamage()
    {
        int dmg = attackDamage;

        if(critChance > 0)
        {
            if(RollCrit())
            {
                dmg = Mathf.RoundToInt(dmg * critMultiplier);
                ShowCritFeedback(dmg);
            }
        }

        return dmg;
    }

    protected bool RollCrit()
    {
        return Random.value <= critChance;
    }

    protected virtual void ShowCritFeedback(int dmg)
    {
        // Incase i wanna add something to the UI later
        Debug.Log($"{name} landed a CRIT for {dmg} damage!");
    }


    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
