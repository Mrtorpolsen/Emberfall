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

    [Header("Team Colors")]
    [SerializeField] private Color playerColor = new Color32(0x2E, 0x3A, 0x5E, 0xFF);
    [SerializeField] private Color enemyColor = new Color32(0xA0, 0x17, 0x0A, 0xFF);

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
        if (!GameManager.Instance.Economy.TryUpgradeIncome())
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
            ? UnitStatsManager.Instance.GetStats(prefabStats.StatsDef)
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
                ? UnitStatsManager.Instance.GetEnemyStats(prefabStats.StatsDef, WaveController.Instance.CreateScalingContext(prefab))
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
        float cost = finalStats != null ? finalStats.Get(StatType.Cost) : prefabStats.StatsDef.cost;

        if (chargeCost && !GameManager.Instance.Economy.TrySpend(team, cost))
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

        TargetRegistry.Instance.RegisterUnit(unitBase);

        spawnedUnit = unit;
        return true;
    }

    public void AssignColor(SpriteRenderer sr, Team team)
    {
        sr.color = team == Team.North ? enemyColor : playerColor;
    }
}

