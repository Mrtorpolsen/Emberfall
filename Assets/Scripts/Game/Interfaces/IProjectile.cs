using UnityEngine;
using System;

public interface IProjectile
{
    void SetTarget(ITargetable target);
    void Init(IUnit shooter, int damage);
    event Action<ITargetable, int> OnHit;
}
