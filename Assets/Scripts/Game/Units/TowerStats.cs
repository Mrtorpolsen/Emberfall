using UnityEngine;

public class TowerStats : UnitStats, IUnit
{
    [Header("Reference")]
    [SerializeField] private GameObject unit;
    [SerializeField] private GameObject towerProjectilePrefab;

    [Header("Attributes")]
    [SerializeField] private float cost = 250;
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth;
    [SerializeField] private int attackDamage = 30;
    [SerializeField] private float attackSpeed = 1.5f;
    [SerializeField] private float attackRange = 3.5f;
    [SerializeField] private float movementSpeed = 0f;


    private CombatManager combat;

    public override Team Team { get; set; }
    public override float Cost => cost;

    public float AttackRange => attackRange;

    public int AttackDamage => attackDamage;

    public float AttackSpeed => attackSpeed;

    public float MovementSpeed => movementSpeed;

    void Start()
    {
        if (unit == null) return;
    }
    void Awake()
    {
        currentHealth = maxHealth;
        combat = GetComponent<CombatManager>();
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
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
        GameObject towerProjectileObj = Instantiate(towerProjectilePrefab, unit.transform.position, Quaternion.identity);
        TowerProjectile towerProjectileScript = towerProjectileObj.GetComponent<TowerProjectile>();
        towerProjectileObj.layer = target.Team == Team.North ? LayerMask.NameToLayer("SouthTeamProjectile") : LayerMask.NameToLayer("NorthTeamProjectile");
        towerProjectileScript.SetTarget(target);
        towerProjectileScript.Init(this, attackDamage);
        towerProjectileScript.OnHit += HandleArrowHit;
    }

    private void HandleArrowHit(ITargetable target, int damage)
    {
        combat.ApplyProjectileDamage(target, damage);
    }
}
