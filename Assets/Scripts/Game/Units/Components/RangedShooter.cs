using UnityEngine;

[RequireComponent(typeof(BaseUnitStats))]
[RequireComponent(typeof(CombatComponent))]
public class RangedShooter : MonoBehaviour, IAttack
{
    [Header("Projectile")]
    [SerializeField] private GameObject projectilePrefab;

    private BaseUnitStats stats;
    private IProjectileImpact impact;

    private void Awake()
    {
        stats = GetComponent<BaseUnitStats>();
        impact = GetComponent<IProjectileImpact>();
    }

    public void Attack(ITargetable target)
    {
        if (projectilePrefab == null || target == null) return;

        GameObject projObj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        var projScript = projObj.GetComponent<IProjectile>();
        if (projScript == null) return;

        projObj.layer = target.Team == Team.North
            ? LayerMask.NameToLayer("SouthTeamProjectile")
            : LayerMask.NameToLayer("NorthTeamProjectile");

        projScript.SetTarget(target);
        projScript.Init(this, stats.GetAttackDamage());
    }

    // Projectiles report hits here; the hit still lands if the shooter died while the projectile was in flight
    public static void ApplyHit(RangedShooter shooter, ITargetable target, int damage, Vector2 position)
    {
        if (shooter != null && shooter.impact != null)
        {
            shooter.impact.OnProjectileImpact(target, damage, position);
            return;
        }

        target.TakeDamage(damage);
    }
}
