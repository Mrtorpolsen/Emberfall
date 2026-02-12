using UnityEngine;

[RequireComponent(typeof(BaseUnitStats))]
[RequireComponent(typeof(CombatComponent))]
public class RangedShooter : MonoBehaviour
{
    [Header("Projectile")]
    [SerializeField] private GameObject projectilePrefab;

    private BaseUnitStats stats;
    private CombatComponent combat;

    private void Awake()
    {
        stats = GetComponent<BaseUnitStats>();
        combat = GetComponent<CombatComponent>();
    }

    public void Shoot(ITargetable target)
    {
        if (projectilePrefab == null || target == null) return;

        GameObject projObj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        var projScript = projObj.GetComponent<IProjectile>();
        if (projScript == null) return;

        projObj.layer = target.Team == Team.North
            ? LayerMask.NameToLayer("SouthTeamProjectile")
            : LayerMask.NameToLayer("NorthTeamProjectile");

        projScript.SetTarget(target);
        projScript.Init(stats, stats.GetAttackDamage());
        projScript.OnHit += (t, dmg) => combat.ApplyProjectileDamage(t, dmg, projObj);
    }
}
