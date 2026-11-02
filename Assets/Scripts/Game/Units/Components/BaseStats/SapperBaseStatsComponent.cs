using System.Collections.Generic;
using UnityEngine;

public class SapperBaseStatsComponent : BaseUnitStats, IAttack
{
    public override ThreatLevel UnitPrio => ThreatLevel.Immediate;

    private readonly List<Collider2D> explosionHits = new();
    private Collider2D[] contactBuffer;
    private ContactFilter2D contactFilter;
    private LayerMask enemyLayer;

    protected override void Awake()
    {
        base.Awake();
        contactBuffer = new Collider2D[8];
        enemyLayer = LayerMask.GetMask("SouthTeam");

        contactFilter = new ContactFilter2D
        {
            useLayerMask = true,
            layerMask = enemyLayer,
            useTriggers = true,
        };
    }

    // Explodes on contact with any enemy, not only the one TargetComponent picked
    private void FixedUpdate()
    {
        if (!IsAlive) return;

        int count = Physics2D.OverlapCircle(transform.position, HitRadius + AttackRange, contactFilter, contactBuffer);

        for (int i = 0; i < count; i++)
        {
            if (!contactBuffer[i].TryGetComponent<ITargetable>(out var target)) continue;
            if (target.Team == metadata.Team || !target.IsTargetable) continue;

            Attack(target);
            return;
        }
    }

    // Sappers attack by blowing themselves up
    public void Attack(ITargetable target)
    {
        // The contact check and CombatComponent can both trigger in the same frame; explode once
        if (!IsAlive) return;

        Explode();
        Die();
    }

    private void Explode()
    {
        // The list overload grows as needed, so a big crowd can't overflow it and escape the blast
        Physics2D.OverlapCircle(
            transform.position,
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
        //To invalidate itself as a target.
        currentHealth = 0;
    }

    protected override void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, base.SplashRadius);
    }
}