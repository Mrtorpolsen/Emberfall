using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UnitStatsManager : MonoBehaviour
{
    [Header("Enemy Unit Stats Definitions")]
    [SerializeField] private List<UnitStatsDefinition> unitStatsDefinition;

    public static UnitStatsManager Instance { get; private set; }
    StatsBootstrapper statsBootstrapper;
    UnitStatsCalculator unitStatsCalculator;

    private Dictionary<string, FinalStats> finalStatsByUnit = new();
    private Dictionary<string, UnitStatsDefinition> unitStatsByUnitKey = new();

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

        BuildStatsLookup();
        CalculateAllFinalStats();
    }

    private void BuildStatsLookup()
    {
        foreach (var unitStats in unitStatsDefinition)
        {
            AddUnitStats(unitStats);
        }
    }

    private void AddUnitStats(UnitStatsDefinition unitStats)
    {
        if (unitStats == null) return;
        string unitKey = unitStats.name.ToLowerInvariant();
        unitStatsByUnitKey[unitKey] = unitStats;
    }

    private void CalculateAllFinalStats()
    {
        if (statsBootstrapper == null) return;

        foreach (var kvp in unitStatsByUnitKey)
        {
            string unitName = kvp.Key;
            UnitStatsDefinition prefab = kvp.Value;

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
        if (!unitStatsByUnitKey.TryGetValue(unitName, out var prefab))
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

    private FinalStats BuildStatsFromBase(UnitStatsDefinition unitBaseStats)
    {
        if (unitBaseStats == null)
        {
            Debug.LogError("Failed to get unitBaseStats in BuildStatsFromBase");
            return null;
        }

        FinalStats finalStats = new FinalStats
        {
            maxHealth = unitBaseStats.maxHealth,
            attackDamage = unitBaseStats.attackDamage,
            attackSpeed = unitBaseStats.attackSpeed,
            attackRange = unitBaseStats.attackRange,
            critChance = unitBaseStats.critChance,
            critMultiplier = unitBaseStats.critMultiplier,
            movementSpeed = unitBaseStats.movementSpeed,
            hitRadius = unitBaseStats.hitRadius,
            cost = unitBaseStats.cost
        };

        return finalStats;
    }

    private void LogStats(string unitName, FinalStats stats)
    {
        Debug.Log(
            $"[FinalStats] {unitName}\n" +
            $"  Health: {stats.maxHealth}\n" +
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

