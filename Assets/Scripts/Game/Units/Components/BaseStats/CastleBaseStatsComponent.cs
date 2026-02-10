using UnityEngine;

public class CastleStatsBaseStatsComponent : BaseUnitStats
{
    [Header("Reference")]
    [SerializeField] public GameObject castle;

    [Header("Team")]
    [SerializeField] public Team team => metadata.Team;


    protected override void Awake()
    {
        metadata = GetComponent<UnitMetadata>();
        currentHealth = MaxHealth;
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
        Gizmos.DrawWireSphere(transform.position, HitRadius);
    }
}
