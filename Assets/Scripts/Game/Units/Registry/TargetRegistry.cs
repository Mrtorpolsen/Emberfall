using System.Collections.Generic;
using UnityEngine;

public class TargetRegistry : MonoBehaviour
{
    public static TargetRegistry Instance;

    public HashSet<BaseUnitStats> playerUnits = new();
    public HashSet<BaseUnitStats> enemyUnits = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public void RegisterUnit(BaseUnitStats unit)
    {
        if (unit.Team == Team.South)
        {
            playerUnits.Add(unit);
        }
        else
        {
            enemyUnits.Add(unit);
        }
    }

    public void UnregisterUnit(BaseUnitStats unit)
    {
        if (unit.Team == Team.South)
        {
            playerUnits.Remove(unit);
        }
        else
        {
            enemyUnits.Remove(unit);
        }
    }

    public IEnumerable<BaseUnitStats> GetAllPlayerUnits()
    {
        return playerUnits;
    }
}
