using UnityEngine;
public class FighterStats : UnitStats, IUnit, ITargetable
{
    [Header("Reference")]
    [SerializeField] private GameObject unit;
    [SerializeField] FloatingHealthBar healthBar;


    [Header("Attributes")]
    [SerializeField] private float cost = 50;
    [SerializeField] private int maxHealth = 200;
    [SerializeField] private int currentHealth;
    [SerializeField] private int attackDamage = 20;
    [SerializeField] private float attackSpeed = 1f;
    [SerializeField] private float attackRange = 0.25f;
    [SerializeField] private float hitRadius = 0.135f;
    [SerializeField] private float movementSpeed = 2f;


    public override Team Team { get; set; }
    public override float Cost => cost;
    public float AttackRange => attackRange;

    public int AttackDamage => attackDamage;

    public float AttackSpeed => attackSpeed;

    public float MovementSpeed => movementSpeed;

    public GameObject GameObject => gameObject;

    public Transform Transform => (this != null) ? transform : null;

    public float HitRadius => hitRadius;

    public bool IsAlive => currentHealth > 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (unit == null) return;
        healthBar.UpdateHealthBar(currentHealth, maxHealth);
    }
    void Awake()
    {
        currentHealth = maxHealth;
        healthBar = GetComponentInChildren<FloatingHealthBar>();
    }
    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        healthBar.UpdateHealthBar(currentHealth, maxHealth);
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        Destroy(unit);
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

}
