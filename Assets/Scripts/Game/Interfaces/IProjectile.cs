public interface IProjectile
{
    void SetTarget(ITargetable target);
    void Init(RangedShooter shooter, int damage);
}
