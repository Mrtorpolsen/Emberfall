using System.Collections.Generic;
using UnityEngine;

public class UnitRegistry : MonoBehaviour
{
    public static UnitRegistry Instance;

    public HashSet<BaseUnitStats> playerUnits = new();
    public HashSet<BaseUnitStats> enemyUnits = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(Instance);
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
