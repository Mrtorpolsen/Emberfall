using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance { get; private set; }

    [Header("North Reference")]
    [SerializeField] private Transform northSpawn;

    [Header("South Reference")]
    [SerializeField] private Transform southSpawn;
    [SerializeField] private Transform southWestTower;
    [SerializeField] private Transform southEastTower;

    private float spawnOffSet = 0.4f;

    private string playerColor = "#2E3A5E";
    private string enemyColor = "#A0170A";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
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

    public bool SpawnSouthUnit(GameObject prefab)
    {
        return SpawnUnit(prefab, southSpawn, Team.South, GetPlayerStats(prefab));
    }

    public bool SpawnSouthTower(GameObject prefab, SpawnSide spawnSide, out GameObject spawnedTower)
    {
        Transform towerPoint = spawnSide == SpawnSide.West ? southWestTower : southEastTower;

        return SpawnUnitAtPoint(prefab, towerPoint, Team.South, out spawnedTower, GetPlayerStats(prefab));
    }

    // Upgraded stats via the prefab's own stats definition, so renaming a prefab can't break the lookup
    private static StatBlock GetPlayerStats(GameObject prefab)
    {
        return prefab.TryGetComponent<BaseUnitStats>(out var prefabStats)
            ? UnitStatsManager.Instance.GetStats(prefabStats.statsDef)
            : null;
    }

    // Spawns near the point with a small random x offset so units don't stack
    public bool SpawnUnit(GameObject prefab, Transform spawnPoint, Team team, StatBlock finalStats = null)
    {
        Vector3 spawnPos = spawnPoint.position;
        spawnPos.x += Random.Range(-spawnOffSet, spawnOffSet);

        return TrySpawn(prefab, spawnPos, spawnPoint.rotation, team, finalStats, chargeCost: team == Team.South, out _);
    }

    // Spawns exactly at the point (towers, generals)
    public bool SpawnUnitAtPoint(GameObject prefab, Transform spawnPoint, Team team, out GameObject spawnedUnit, StatBlock finalStats = null)
    {
        return TrySpawn(prefab, spawnPoint.position, spawnPoint.rotation, team, finalStats, chargeCost: team == Team.South, out spawnedUnit);
    }

    // Summons are free and spawn for the caster's team; player summons get upgraded stats, enemy summons current wave scaling
    public bool SpawnSummon(GameObject prefab, BaseUnitStats caster)
    {
        StatBlock finalStats;

        if (caster.Team == Team.South)
        {
            finalStats = GetPlayerStats(prefab);
        }
        else
        {
            finalStats = prefab.TryGetComponent<BaseUnitStats>(out var prefabStats)
                ? UnitStatsManager.Instance.GetEnemyStats(prefabStats.statsDef, WaveController.Instance.CreateScalingContext(prefab))
                : null;
        }

        Vector3 spawnPos = caster.transform.position;
        spawnPos.x += Random.Range(-spawnOffSet, spawnOffSet);

        return TrySpawn(prefab, spawnPos, caster.transform.rotation, caster.Team, finalStats, chargeCost: false, out _);
    }

    private bool TrySpawn(GameObject prefab, Vector3 position, Quaternion rotation, Team team,
        StatBlock finalStats, bool chargeCost, out GameObject spawnedUnit)
    {
        spawnedUnit = null;

        BaseUnitStats prefabStats = prefab.GetComponent<BaseUnitStats>();

        if (prefabStats == null)
        {
            Debug.LogError("Prefab has no UnitStats component!");
            return false;
        }

        // A prefab's BaseUnitStats never ran Awake, so read cost from the stats it will spawn with
        float cost = finalStats != null ? finalStats.Get(StatType.Cost) : prefabStats.statsDef.cost;

        if (chargeCost && GameManager.Instance.currency[team] < cost)
        {
            Debug.LogWarning("Not enough currency");
            return false;
        }

        GameObject unit = Instantiate(prefab, position, rotation);
        BaseUnitStats unitBase = unit.GetComponent<BaseUnitStats>();

        if (finalStats != null)
        {
            unitBase.ApplyFinalStats(finalStats);
        }

        SpriteRenderer sr = unit.GetComponent<SpriteRenderer>();

        if (sr != null)
        {
            AssignColor(sr, team);
        }

        unit.GetComponent<UnitMetadata>().SetTeam(team);
        unit.layer = LayerMask.NameToLayer(team + "Team");

        if (chargeCost)
        {
            GameManager.Instance.SubtractCurrency(team, cost);
        }

        TargetRegistry.Instance.RegisterUnit(unitBase);

        spawnedUnit = unit;
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

