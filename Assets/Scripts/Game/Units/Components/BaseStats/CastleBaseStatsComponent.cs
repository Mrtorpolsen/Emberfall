using UnityEngine;

public class CastleStatsBaseStatsComponent : BaseUnitStats
{
#if UNITY_EDITOR
    [Header("Testing")]
    [SerializeField] public bool isTest;
#endif

    [Header("Reference")]
    [SerializeField] public GameObject castle;

    [Header("Team")]
    public Team team => metadata.Team;

#if UNITY_EDITOR
    protected override void Start()
    {
        base.Start();
        if (isTest)
        {
            currentHealth = 100000000;
            GameManager.Instance.AddCurrency(Team.South, 100000);
        }
    }
#endif

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
