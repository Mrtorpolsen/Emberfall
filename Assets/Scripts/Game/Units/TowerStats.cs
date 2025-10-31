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


    private Combat combat;

    public override Team Team { get; set; }
    public override float Cost => cost;
    public GameObject GetGameObject() => gameObject;
    public Team GetTeam() => Team;
    public float GetAttackRange() => attackRange;
    public int GetAttackDamage() => attackDamage;
    public float GetAttackSpeed() => attackSpeed;
    public bool GetIsAlive() => currentHealth > 0;
    public float GetHitRadius() => 0f;
    public float GetMovementSpeed() => 0f;


    public Transform GetTransform()
    {
        return (this != null) ? transform : null;
    }

    void Start()
    {
        if (unit == null) return;
    }
    void Awake()
    {
        currentHealth = maxHealth;
        combat = GetComponent<Combat>();
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
        towerProjectileObj.layer = target.GetTeam() == Team.North ? LayerMask.NameToLayer("SouthTeamProjectile") : LayerMask.NameToLayer("NorthTeamProjectile");
        towerProjectileScript.SetTarget(target);
        towerProjectileScript.Init(this, attackDamage);
        towerProjectileScript.OnHit += HandleArrowHit;
    }

    private void HandleArrowHit(ITargetable target, int damage)
    {
        combat.ApplyProjectileDamage(target, damage);
    }
}
