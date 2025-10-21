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


    private Combat combat;

    public override Team Team { get; set; }
    public override float Cost => cost;
    public GameObject GetGameObject() => gameObject;
    public Team GetTeam() => Team;
    public float GetAttackRange() => attackRange;
    public int GetAttackDamage() => attackDamage;
    public float GetAttackSpeed() => attackSpeed;
    public bool GetIsAlive() => currentHealth > 0;
    public float GetHitRadius() => hitRadius;
    public float GetMovementSpeed() => movementSpeed;


    public Transform GetTransform()
    {
        return (this != null) ? transform : null;
    }

    void Start()
    {
        if (unit == null) return;
        healthBar.UpdateHealthBar(currentHealth, maxHealth);
    }
    void Awake()
    {
        currentHealth = maxHealth;
        healthBar = GetComponentInChildren<FloatingHealthBar>();
        combat = GetComponent<Combat>();
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
        arrowObj.layer = target.GetTeam() == Team.North ? LayerMask.NameToLayer("SouthTeamProjectile") : LayerMask.NameToLayer("NorthTeamProjectile");
        arrowScript.SetTarget(target);

        arrowScript.Init(this, attackDamage);
        arrowScript.OnHit += HandleArrowHit;
    }

    private void HandleArrowHit(ITargetable target, int damage)
    {
        combat.ApplyProjectileDamage(target, damage);
    }
}
