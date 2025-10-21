using UnityEngine;

public class CavalierStats : UnitStats, IUnit, ITargetable
{
    [Header("Reference")]
    [SerializeField] private GameObject unit;
    [SerializeField] FloatingHealthBar healthBar;


    [Header("Attributes")]
    [SerializeField] private float cost = 100;
    [SerializeField] private int maxHealth = 400;
    [SerializeField] private int currentHealth;
    [SerializeField] private int attackDamage = 30;
    [SerializeField] private float attackSpeed = 0.75f;
    [SerializeField] private float attackRange = 0.25f;
    [SerializeField] private float hitRadius = 0.135f;
    [SerializeField] private float movementSpeed = 4f;

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
