using System.Linq;
using UnityEngine;

public class SapperBaseStatsComponent : BaseUnitStats
{
    [SerializeField] private float explosionRadius = 1.5f;

    public override ThreatLevel UnitPrio => ThreatLevel.Immidate;

    private Collider2D[] hitBuffer;
    private ContactFilter2D contactFilter;
    private LayerMask enemyLayer;

    protected override void Awake()
    {
        base.Awake();
        hitBuffer = new Collider2D[32];
        enemyLayer = LayerMask.GetMask("SouthTeam");

        contactFilter = new ContactFilter2D
        {
            useLayerMask = true,
            layerMask = enemyLayer,
            useTriggers = true,
        };
    }

    public void Explode()
    {
        float radiusSquare = explosionRadius * explosionRadius;
        Debug.Log("yoooooo sapper!!!");

        int hitCount = Physics2D.OverlapCircle(
            transform.position,
            explosionRadius,
            contactFilter,
            hitBuffer
        );

        if (hitCount == hitBuffer.Length)
        {
            Debug.LogWarning("Overflow detected in sapper!!!");
        }

        for (int i = 0; i < hitCount; i++)
        {
            Collider2D hit = hitBuffer[i];

            if (!hit.TryGetComponent<ITargetable>(out var target))
                continue;

            if (target.Team == metadata.Team) continue;

            Transform t = target.Transform;

            if (t == null) continue;

            target.TakeDamage(AttackDamage);
        }
        //set death to trigger target switch
        currentHealth = 0;

        Destroy(gameObject);
    }
    protected override void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}