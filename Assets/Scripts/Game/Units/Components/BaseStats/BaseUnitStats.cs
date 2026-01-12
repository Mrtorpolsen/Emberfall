using UnityEngine;

public abstract class BaseUnitStats : UnitStats, IUnit, ITargetable
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

    public override Team Team { get; set; }
    public override float Cost => cost;

    // IUnit
    public float AttackRange => attackRange;
    public int AttackDamage => attackDamage;
    public float AttackSpeed => attackSpeed;
    public float MovementSpeed => movementSpeed;

    // ITargetable
    public GameObject GameObject => gameObject;
    public Transform Transform => (this != null) ? transform : null;
    public float HitRadius => hitRadius;
    public bool IsAlive => currentHealth > 0;

    protected virtual void Awake()
    {
        currentHealth = maxHealth;
        healthBar = GetComponentInChildren<FloatingHealthBar>();
    }

    protected virtual void Start()
    {
        healthBar?.UpdateHealthBar(currentHealth, maxHealth);
    }

    public virtual void TakeDamage(int amount)
    {
        currentHealth -= amount;
        healthBar?.UpdateHealthBar(currentHealth, maxHealth);
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public virtual void Die()
    {
        Destroy(unit != null ? unit : gameObject);
    }

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
