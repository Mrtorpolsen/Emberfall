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
    protected void Start()
    {
        if (isTest)
        {
            currentHealth = 100000000;
            GameManager.Instance.AddCurrency(Team.South, 100000);
        }
    }
#endif

    protected override GameObject Root => castle != null ? castle : gameObject;

    protected override void OnDeath()
    {
        GameManager.Instance.SetGameOver(true, Team);
    }

    //protected override void OnDrawGizmosSelected()
    //{
    //    Gizmos.color = Color.red;
    //    Gizmos.DrawWireSphere(transform.position, HitRadius);
    //}
}
