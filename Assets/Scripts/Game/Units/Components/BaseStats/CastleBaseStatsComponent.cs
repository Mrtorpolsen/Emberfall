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

    public Team Team;
    Team IHasTeam.Team => Team;
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
        GameManager.Instance.SetGameOver(true, Team);
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
