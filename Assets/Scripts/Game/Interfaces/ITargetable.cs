using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public interface ITargetable : IHasTeam
{
    GameObject GameObject { get; }
    Transform Transform { get; }
    float HitRadius { get; }
    bool IsAlive { get; }
    bool IsTargetable { get; }
    void TakeDamage(int amount);
    void Die();
    ThreatLevel UnitPrio { get; }
}