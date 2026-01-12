using UnityEngine;

public class RangerStats : UnitStats, IUnit, ITargetable
{
    [Header("Reference")]
    [SerializeField] private GameObject unit;
    [SerializeField] FloatingHealthBar healthBar;
    [SerializeField] private GameObject arrowPrefab;

    [Header("Attributes")]
    [SerializeField] private float cost = 75;
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int attackDamage = 20;
    [SerializeField] private int currentHealth;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackSpeed = 0.5f;
    [SerializeField] private float hitRadius = 0.135f;
    [SerializeField] private float movementSpeed = 1.75f;


    private CombatManager combat;

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

    void Start()
    {
        if (unit == null) return;
        healthBar.UpdateHealthBar(currentHealth, maxHealth);
    }
    void Awake()
    {
        currentHealth = maxHealth;
        healthBar = GetComponentInChildren<FloatingHealthBar>();
        combat = GetComponent<CombatManager>();
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

    public void Shoot(ITargetable target)
    {
        GameObject arrowObj = Instantiate(arrowPrefab, unit.transform.position, Quaternion.identity);
        Arrow arrowScript = arrowObj.GetComponent<Arrow>();
        arrowObj.layer = target.Team == Team.North ? LayerMask.NameToLayer("SouthTeamProjectile") : LayerMask.NameToLayer("NorthTeamProjectile");
        arrowScript.SetTarget(target);

        arrowScript.Init(this, attackDamage);
        arrowScript.OnHit += HandleArrowHit;
    }

    private void HandleArrowHit(ITargetable target, int damage)
    {
        combat.ApplyProjectileDamage(target, damage);
    }
}
