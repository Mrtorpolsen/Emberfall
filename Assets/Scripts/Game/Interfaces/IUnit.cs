using UnityEngine;

public interface IUnit : IHasTeam
{
    float AttackRange { get; }
    int AttackDamage { get; }
    float AttackSpeed { get; }
    float MovementSpeed { get; }
}
