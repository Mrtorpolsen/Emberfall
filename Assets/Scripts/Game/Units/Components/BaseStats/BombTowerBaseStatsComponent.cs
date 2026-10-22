using UnityEngine;

public class BombTowerBaseStatsComponent : TowerUnitStats, IProjectileImpact
{
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

    // Bombs splash around where they land instead of hitting a single target
    public void OnProjectileImpact(ITargetable target, int damage, Vector2 position)
    {
        Explode(position);
    }

    private void Explode(Vector2 position)
    {
        int hitCount = Physics2D.OverlapCircle(
            position,
            base.SplashRadius,
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
