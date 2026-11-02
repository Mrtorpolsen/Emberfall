using System.Collections.Generic;
using UnityEngine;

public class BombTowerBaseStatsComponent : TowerUnitStats, IProjectileImpact
{
    private readonly List<Collider2D> explosionHits = new();
    private ContactFilter2D contactFilter;
    private LayerMask enemyLayer;

    protected override void Awake()
    {
        base.Awake();
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
        // The list overload grows as needed, so a big crowd can't overflow it and escape the blast
        Physics2D.OverlapCircle(
            position,
            base.SplashRadius,
            contactFilter,
            explosionHits
        );

        foreach (Collider2D hit in explosionHits)
        {
            if (!hit.TryGetComponent<ITargetable>(out var target))
                continue;

            if (target.Team == metadata.Team) continue;

            Transform t = target.Transform;

            if (t == null) continue;

            target.TakeDamage(AttackDamage);
        }
    }
}
