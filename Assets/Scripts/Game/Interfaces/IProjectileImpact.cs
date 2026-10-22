using UnityEngine;

// Optional on a ranged unit: replaces the default single-target damage when its projectile lands (e.g. splash)
public interface IProjectileImpact
{
    void OnProjectileImpact(ITargetable target, int damage, Vector2 position);
}
