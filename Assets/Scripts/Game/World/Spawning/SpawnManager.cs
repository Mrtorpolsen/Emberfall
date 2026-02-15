using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance;

    [Header("North Reference")]
    [SerializeField] private Transform northSpawn;

    [Header("South Reference")]
    [SerializeField] private Transform southSpawn;
    [SerializeField] private Transform southWestTower;
    [SerializeField] private Transform southEastTower;

    private string playerColor = "#2E3A5E";
    private string enemyColor = "#A0170A";

    private void Awake()
    {
        if(Instance ==  null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void UpgradeIncome()
    {
        if (GameManager.Instance.currency[Team.South] >= GameManager.Instance.incomeUpgradeCost)
        {
            GameManager.Instance.UpgradeIncomeModifier();
            GameManager.Instance.SubtractCurrency(Team.South, GameManager.Instance.incomeUpgradeCost);
            GameManager.Instance.incomeUpgradeCost += 50;
            UIManager.Instance.UpdateIncomeCostText();
        }
        else
        {
            Debug.Log($"{Team.South} - Insufficient currency");
        }
    }

    public bool SpawnSouthUnit(GameObject prefab, string unitName)
    {
        FinalStats finalStats = UnitStatsManager.Instance.GetStats(unitName);

        return SpawnUnit(prefab, southSpawn, Team.South, finalStats);
    }

    public bool SpawnSouthTower(GameObject prefab, SpawnSide spawnSide, out GameObject spawnedTower, FinalStats finalStats = null)
    {
        spawnedTower = null;

        Transform towerPoint = spawnSide == SpawnSide.West ? southWestTower : southEastTower;

        return SpawnUnit(prefab, towerPoint, Team.South, out spawnedTower, finalStats);
    }

    public bool SpawnUnit(GameObject prefab, Transform spawnPoint, Team team, FinalStats finalStats = null)
    {
        BaseUnitStats stats = prefab.GetComponent<BaseUnitStats>();

        if (stats == null)
        {
            Debug.LogError("Prefab has no UnitStats component!");
            return false;
        }

        if (team == Team.South && GameManager.Instance.currency[Team.South] < stats.Cost)
        {
            Debug.LogWarning("Not enough currency");
            return false;
        }

        GameObject unit = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
        stats = unit.GetComponent<BaseUnitStats>();

        if (stats != null && finalStats != null)
        {
            stats.ApplyFinalStats(finalStats);
        }

        SpriteRenderer sr = unit.GetComponent<SpriteRenderer>();

        if (sr != null) 
        {
            AssignColor(sr, team);
        }
            
        UnitMetadata unitStats = unit.GetComponent<UnitMetadata>();

        unitStats.SetTeam(team);

        if(prefab != Prefabs.gatePrefab)
        {
            unit.layer = LayerMask.NameToLayer(team.ToString() + "Team");
        }

        if(team == Team.South)
        {
            GameManager.Instance.SubtractCurrency(team, stats.Cost);
        }

        return true;
    }

    public bool SpawnUnit(
            GameObject prefab,
            Transform spawnPoint,
            Team team,
            out GameObject spawnedUnit,
            FinalStats finalStats = null
    )
    {
        spawnedUnit = null;

        BaseUnitStats stats = prefab.GetComponent<BaseUnitStats>();
        if (stats == null)
        {
            Debug.LogError("Prefab has no UnitStats component!");
            return false;
        }

        if (team == Team.South && GameManager.Instance.currency[Team.South] < stats.Cost)
        {
            Debug.LogWarning("Not enough currency");
            return false;
        }

        GameObject unit = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
        spawnedUnit = unit;

        stats = unit.GetComponent<BaseUnitStats>();
        if (stats != null && finalStats != null)
        {
            stats.ApplyFinalStats(finalStats);
        }

        SpriteRenderer sr = unit.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            AssignColor(sr, team);
        }

        UnitMetadata metadata = unit.GetComponent<UnitMetadata>();
        metadata.SetTeam(team);

        if (prefab != Prefabs.gatePrefab)
        {
            unit.layer = LayerMask.NameToLayer(team + "Team");
        }

        if (team == Team.South)
        {
            GameManager.Instance.SubtractCurrency(team, stats.Cost);
        }

        return true;
    }

    public void AssignColor(SpriteRenderer sr, Team team)
    {
        string teamColorCode = team == Team.North ? enemyColor : playerColor;
        Color teamColor;

        if (UnityEngine.ColorUtility.TryParseHtmlString(teamColorCode, out teamColor))
        {
            sr.color = teamColor;
        }
    }
}   

