using Unity.VisualScripting;
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

    [Header("Prefabs")]
    [SerializeField] private GameObject fighterPrefab;
    [SerializeField] private GameObject rangerPrefab;
    [SerializeField] private GameObject cavalierPrefab;
    [SerializeField] private GameObject gatePrefab;
    [SerializeField] private GameObject towerPrefab;

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

    public bool SpawnUnit(GameObject prefab, Transform spawnPoint, Team team)
    {
        UnitMetadata stats = prefab.GetComponent<UnitMetadata>();

        if (stats == null)
        {
            Debug.LogError("Prefab has no UnitStats component!");
            return false;
        }

        if (GameManager.Instance.currency[team] >= stats.Cost || team == Team.North)
        {
            GameObject unit = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
            SpriteRenderer sr = unit.GetComponent<SpriteRenderer>();

            if (sr != null) 
            {
                AssignColor(sr, team);
            }
            
            UnitMetadata unitStats = unit.GetComponent<UnitMetadata>();

            unitStats.GetComponent<UnitMetadata>().Team = team;

            if(prefab != gatePrefab)
            {
                unit.layer = LayerMask.NameToLayer(team.ToString() + "Team");
            }

            if(team == Team.South)
            {
                GameManager.Instance.SubtractCurrency(team, stats.Cost);
            }

            return true;
        }
        else
        {
            if(team == Team.South)
            {
                Debug.Log($"{team} - Insufficient currency");
            }

            return false;
        }
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

    public void SpawnSouthTower(Transform platform)
    {
        SpawnUnit(towerPrefab, platform, Team.South);
    }

    public bool SpawnNorthFighter()
    {
        return SpawnUnit(fighterPrefab, northSpawn, Team.North);
    }
    public bool SpawnNorthRanger()
    {
        return SpawnUnit(rangerPrefab, northSpawn, Team.North);
    }
    public bool SpawnNorthCavalier()
    {
        return SpawnUnit(cavalierPrefab, northSpawn, Team.North);
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
    public bool SpawnSouthFighter()
    {
        return SpawnUnit(fighterPrefab, southSpawn, Team.South);
    }
    public bool SpawnSouthRanger()
    {
        return SpawnUnit(rangerPrefab, southSpawn, Team.South);
    }
    public bool SpawnSouthCavalier()
    {
        return SpawnUnit(cavalierPrefab, southSpawn, Team.South);
    }
    public bool SpawnSouthGateClose()
    {
        return SpawnUnit(gatePrefab, southGateClose, Team.South);
    }
    public bool SpawnSouthGateIntermediate()
    {
        return SpawnUnit(gatePrefab, southGateIntermediate, Team.South);
    }
    public bool SpawnSouthGateFar()
    {
        return SpawnUnit(gatePrefab, southGateFar, Team.South);
    }
}   

