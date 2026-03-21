using UnityEngine;

public interface IUnit : IHasTeam
{
    float AttackRange { get; }
    int AttackDamage { get; }
    float AttackSpeed { get; }
    float CritDamage { get; }
    float CritChance { get; }
    float MovementSpeed { get; }
    int MaxHealth { get; }
    int Armor { get; }
}
