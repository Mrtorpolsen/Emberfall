using UnityEngine;

public interface IUnit
{
    float GetAttackRange();
    int GetAttackDamage();
    float GetAttackSpeed();
    float GetMovementSpeed();
    Team GetTeam();
}