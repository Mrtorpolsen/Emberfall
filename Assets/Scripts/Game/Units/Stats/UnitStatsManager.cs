using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UnitStatsManager : MonoBehaviour
{
    public static UnitStatsManager Instance { get; private set; }
    StatsBootstrapper statsBootstrapper;
    UnitStatsCalculator unitStatsCalculator;

    private Dictionary<string, FinalStats> finalStatsByUnit = new();
    private Dictionary<string, GameObject> prefabByUnitKey = new();

    private void Awake()
    {
        Instance = this;

        unitStatsCalculator = new UnitStatsCalculator();
        //For testing
        try
        {
            statsBootstrapper = new StatsBootstrapper();
            statsBootstrapper.LoadAndBuildTalents();
        }
        catch
        {
            Debug.LogWarning("StatsBootstrapper not initialized. Skipping talent application.");
            statsBootstrapper = null;
        }

        BuildPrefabLookup();
        CalculateAllFinalStats();
    }

    private void BuildPrefabLookup()
    {
        prefabByUnitKey.Clear();

        //Add the prefabs that needs more stats
        AddPrefab(Prefabs.fighterPrefab);
        AddPrefab(Prefabs.rangerPrefab);
        AddPrefab(Prefabs.cavalierPrefab);

        //Special
        AddPrefab(Prefabs.assasinPrefab);
        AddPrefab(Prefabs.sapperPrefab);

        //Boss
        AddPrefab(Prefabs.giantPrefab);

        //Elites
        AddPrefab(Prefabs.eliteFighterPrefab);
        AddPrefab(Prefabs.eliteCavalierPrefab);
    }

    private void AddPrefab(GameObject prefab)
    {
        if (prefab == null) return;
        string unitKey = prefab.name.ToLowerInvariant();
        prefabByUnitKey[unitKey] = prefab;
    }

    private void CalculateAllFinalStats()
    {
        if (statsBootstrapper == null) return;


        foreach (var kvp in prefabByUnitKey)
        {
            string unitName = kvp.Key;
            GameObject prefab = kvp.Value;

            if (!statsBootstrapper.TalentsByUnit.TryGetValue(unitName, out var unitTalents))
                continue;

            var appliedTalents = new List<AppliedTalent>();

            foreach (var talent in unitTalents)
            {
                appliedTalents.Add(new AppliedTalent
                {
                    Effects = talent.effects,
                    Purchased = talent.purcashed
                });
            }

            FinalStats finalStats = BuildStatsFromBase(prefab);
            unitStatsCalculator.ApplyTalents(ref finalStats, appliedTalents);
            finalStatsByUnit[unitName] = finalStats;
        }
    }

    public FinalStats GetStats(string unitName)
    {
        return finalStatsByUnit.TryGetValue(unitName, out var stats) ? stats : null;
    }

    public FinalStats GetEnemyStats(string unitName, WaveController.EnemyScalingContext scaling)
    {
        if (!prefabByUnitKey.TryGetValue(unitName, out var prefab))
        {
            Debug.LogError($"No prefab found for unitKey: {unitName}");
            return null;
        }

        FinalStats finalStats = BuildStatsFromBase(prefab);

        if (finalStats == null)
        {
            Debug.LogError("Failed to get finalstats");
            return null;
        }

        return unitStatsCalculator.CalculateEnemyStats(scaling.waveIndex, finalStats);
    }

    private FinalStats BuildStatsFromBase(GameObject unitPrefab)
    {
        BaseUnitStats baseStats = unitPrefab.GetComponent<BaseUnitStats>();

        if (baseStats == null)
        {
            Debug.LogError("Failed to get basestats in BuildStatsFromBase");
            return null;
        }

        FinalStats finalStats = new FinalStats
        {
            health = baseStats.MaxHealth,
            attackDamage = baseStats.AttackDamage,
            attackSpeed = baseStats.AttackSpeed,
            attackRange = baseStats.AttackRange,
            critChance = baseStats.CritChance,
            critMultiplier = baseStats.CritMultiplier,
            movementSpeed = baseStats.MovementSpeed,
            hitRadius = baseStats.HitRadius,
            cost = baseStats.Cost
        };

        return finalStats;
    }

    private void LogStats(string unitName, FinalStats stats)
    {
        Debug.Log(
            $"[FinalStats] {unitName}\n" +
            $"  Health: {stats.health}\n" +
            $"  Damage: {stats.attackDamage}\n" +
            $"  Attack Speed: {stats.attackSpeed:F3}\n" +
            $"  Range: {stats.attackRange:F2}\n" +
            $"  Move Speed: {stats.movementSpeed:F2}\n" +
            $"  Hit Radius: {stats.hitRadius:F2}\n" +
            $"  Cost: {stats.cost:F1}\n" +
            $"  Armor: {stats.armor:F1}"
        );
    }

    private void LogStats(string unitName, BaseUnitStats stats)
    {
        Debug.Log(
            $"[BaseStats] {unitName}\n" +
            $"  Health: {stats.MaxHealth}\n" +
            $"  Damage: {stats.AttackDamage}\n" +
            $"  Attack Speed: {stats.AttackSpeed:F3}\n" +
            $"  Range: {stats.AttackRange:F2}\n" +
            $"  Move Speed: {stats.MovementSpeed:F2}\n" +
            $"  Hit Radius: {stats.HitRadius:F2}\n" +
            $"  Cost: {stats.Cost:F1}\n" +
            $"  Armor: {stats.Armor:F1}"
        );
    }

}

