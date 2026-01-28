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
    [SerializeField] private Transform southGateClose;
    [SerializeField] private Transform southGateIntermediate;
    [SerializeField] private Transform southGateFar;
    [SerializeField] private Transform southEastTower;
    [SerializeField] private Transform southWestTower;

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

        unitStats.GetComponent<UnitMetadata>().SetTeam(team);

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

    public void AssignColor(SpriteRenderer sr, Team team)
    {
        string teamColorCode = team == Team.North ? enemyColor : playerColor;
        Color teamColor;

        if (UnityEngine.ColorUtility.TryParseHtmlString(teamColorCode, out teamColor))
        {
            sr.color = teamColor;
        }
    }

    public bool SpawnSouthFighter()
    {
        FinalStats finalStats = UnitStatsManager.Instance.GetStats("fighter");

        return SpawnUnit(Prefabs.fighterPrefab, southSpawn, Team.South, finalStats);
    }
    public bool SpawnSouthRanger()
    {
        FinalStats finalStats = UnitStatsManager.Instance.GetStats("ranger");

        return SpawnUnit(Prefabs.rangerPrefab, southSpawn, Team.South, finalStats);
    }
    public bool SpawnSouthCavalier()
    {
        FinalStats finalStats = UnitStatsManager.Instance.GetStats("cavalier");

        return SpawnUnit(Prefabs.cavalierPrefab, southSpawn, Team.South, finalStats);
    }
    public void SpawnSouthTower(Transform platform, GameObject towerPrefab)
    {
        SpawnUnit(towerPrefab, platform, Team.South);
    }

    public bool SpawnNorthFighter()
    {
        return SpawnUnit(Prefabs.fighterPrefab, northSpawn, Team.North);
    }
    public bool SpawnNorthRanger()
    {
        return SpawnUnit(Prefabs.rangerPrefab, northSpawn, Team.North);
    }
    public bool SpawnNorthCavalier()
    {
        return SpawnUnit(Prefabs.cavalierPrefab, northSpawn, Team.North);
    }
    public void SpawnSouthFigterUI()
    {
        SpawnSouthFighter();
    }
    public void SpawnSouthRangerUI()
    {
        SpawnSouthRanger();
    }
    public void SpawnSouthCavalierUI()
    {
        SpawnSouthCavalier();
    }
    public void SpawnSouthGateCloseUI()
    {
        SpawnSouthGateClose();
    }
    public void SpawnSouthGateIntermediateUI()
    {
        SpawnSouthGateIntermediate();
    }
    public void SpawnSouthGateFarUI()
    {
        SpawnSouthGateFar();
    }
    public bool SpawnSouthGateClose()
    {
        return SpawnUnit(Prefabs.gatePrefab, southGateClose, Team.South);
    }
    public bool SpawnSouthGateIntermediate()
    {
        return SpawnUnit(Prefabs.gatePrefab, southGateIntermediate, Team.South);
    }
    public bool SpawnSouthGateFar()
    {
        return SpawnUnit(Prefabs.gatePrefab, southGateFar, Team.South);
    }
}   

