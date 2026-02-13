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
    [SerializeField] public Team team => metadata.Team;



    //protected override void Awake() 
    //{
    //    metadata = GetComponent<UnitMetadata>();
    //    currentHealth = MaxHealth;
    //    healthBar = GetComponentInChildren<FloatingHealthBar>();
    //}
#if UNITY_EDITOR
    private void Start()
    {
        if (isTest)
        {
            currentHealth = 100000000;
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
