using UnityEngine;

public class CastleStats : MonoBehaviour, IUnit, ITargetable
{
    [Header("Reference")]
    [SerializeField] public GameObject castle;
    [SerializeField] FloatingHealthBar healthBar;


    [Header("Attributes")]
    [SerializeField] public float cost = 0;
    [SerializeField] public int maxHealth = 500;
    [SerializeField] public int currentHealth;
    [SerializeField] public int attackDamage = 0;
    [SerializeField] public float attackSpeed = 0f;
    [SerializeField] public float attackRange = 0f;
    [SerializeField] public float hitRadius = 0.26f;
    [SerializeField] public float movementSpeed = 0f;

    [SerializeField] public Team team;
    public GameObject GetGameObject() => gameObject;
    public Team GetTeam() => team;
    public float GetAttackRange() => attackRange;
    public int GetAttackDamage() => attackDamage;
    public float GetAttackSpeed() => attackSpeed;
    public bool GetIsAlive() => currentHealth > 0;
    public float GetHitRadius() => hitRadius;
    public float GetMovementSpeed() => movementSpeed;

    void Start()
    {
        healthBar.UpdateHealthBar(currentHealth, maxHealth);
    }

    private void Awake()
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

    void Die()
    {
        Destroy(castle);
        GameManager.main.SetGameOver(true, team);
    }

    public Transform GetTransform()
    {
        return (this != null) ? transform : null;
    }

    void ITargetable.Die()
    {
        Die();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, hitRadius);
    }

}
