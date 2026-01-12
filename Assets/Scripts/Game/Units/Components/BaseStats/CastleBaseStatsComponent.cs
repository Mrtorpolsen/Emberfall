using UnityEngine;

public class CastleStats : BaseUnitStats
{
    [Header("Reference")]
    [SerializeField] public GameObject castle;

    [Header("Team")]
    [SerializeField] private Team team;
    public override Team Team
    {
        get => team;
        set => team = value;
    }

    protected override void Awake()
    {
        currentHealth = maxHealth;
        healthBar = GetComponentInChildren<FloatingHealthBar>();
    }

    public override void Die()
    {
        Destroy(castle != null ? castle : gameObject);
        GameManager.Instance.SetGameOver(true, Team);
    }

    protected override void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, hitRadius);
    }
}
