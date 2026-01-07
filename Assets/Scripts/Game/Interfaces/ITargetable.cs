using UnityEngine;

public interface ITargetable : IHasTeam
{
    GameObject GameObject { get; }
    Transform Transform { get; }
    float HitRadius { get; }
    bool IsAlive { get; }

    void TakeDamage(int amount);
    void Die();
}