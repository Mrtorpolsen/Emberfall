using UnityEditor;
using UnityEngine;

public class UnitBaseStats
{
    public int health;
    public int attackDamage;
    public float attackSpeed;
    public float movementSpeed;
}

public class Unit : MonoBehaviour
{
    public UnitBaseStats baseStats;
    public RuntimeStats finalStats;
}