using UnityEngine;

public interface ITargetable
{
    GameObject GetGameObject();
    Team GetTeam();
    Transform GetTransform();
    void TakeDamage(int amount);
    void Die();
    float GetHitRadius();
    bool GetIsAlive();
}