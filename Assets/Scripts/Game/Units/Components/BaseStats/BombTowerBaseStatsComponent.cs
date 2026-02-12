using UnityEngine;

public class BombTowerBaseStatsComponent : TowerUnitStats
{
    [SerializeField] private float explosionRadius = 0.4f;

    private Collider2D[] hitBuffer;
    private ContactFilter2D contactFilter;
    private LayerMask enemyLayer;

    protected override void Awake()
    {
        base.Awake();
        hitBuffer = new Collider2D[16];
        enemyLayer = LayerMask.GetMask("NorthTeam");

        contactFilter = new ContactFilter2D
        {
            useLayerMask = true,
            layerMask = enemyLayer,
            useTriggers = true,
        };
    }

    public void Explode(GameObject projObj)
    {
        float radiusSquare = explosionRadius * explosionRadius;

        int hitCount = Physics2D.OverlapCircle(
            projObj.transform.position,
            explosionRadius,
            contactFilter,
            hitBuffer
        );

        if (hitCount == hitBuffer.Length)
        {
            Debug.LogWarning("Overflow detected in bombtower!!!");
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
    }
}
