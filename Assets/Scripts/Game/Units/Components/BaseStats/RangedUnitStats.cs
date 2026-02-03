using UnityEngine;

public abstract class RangedUnitStats : BaseUnitStats
{
    [Header("Projectile")]
    [SerializeField] protected GameObject projectilePrefab;
    protected CombatComponent combat;

    protected override void Awake()
    {
        base.Awake();
        combat = GetComponent<CombatComponent>();
    }

    public virtual void Shoot(ITargetable target)
    {
        if (projectilePrefab == null || target == null) return;

        GameObject projObj = Instantiate(projectilePrefab, unit.transform.position, Quaternion.identity);
        var projScript = projObj.GetComponent<IProjectile>();
        if (projScript == null) return;

        projObj.layer = target.Team == Team.North
            ? LayerMask.NameToLayer("SouthTeamProjectile")
            : LayerMask.NameToLayer("NorthTeamProjectile");

        projScript.SetTarget(target);
        projScript.Init(this, GetAttackDamage());
        projScript.OnHit += (t, dmg) => combat.ApplyProjectileDamage(t, dmg, projObj);
    }
}
