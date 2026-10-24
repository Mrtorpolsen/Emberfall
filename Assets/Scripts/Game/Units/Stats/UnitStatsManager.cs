using System;
using System.Collections.Generic;
using UnityEngine;

public class UnitStatsManager : MonoBehaviour
{
    public static UnitStatsManager Instance { get; private set; }

    private StatsBootstrapper statsBootstrapper;
    public StatsBootstrapper StatsBootstrapper
    {
        get => statsBootstrapper;
        set => statsBootstrapper = value;
    }

    private UnitStatsCalculator unitStatsCalculator;
    public UnitStatsCalculator UnitStatsCalculator
    {
        get => unitStatsCalculator;
        set => unitStatsCalculator = value;
    }

    private Dictionary<string, StatBlock> finalStatsByUnit = new();
    public Dictionary<string, StatBlock> FinalStatsByUnit
    {
        get => finalStatsByUnit;
        set => finalStatsByUnit = value;
    }

    private Dictionary<string, UnitStatsDefinition> unitStatsByUnitKey = new();
    public Dictionary<string, UnitStatsDefinition> UnitStatsByUnitKey
    {
        get => unitStatsByUnitKey;
        set => unitStatsByUnitKey = value;
    }

    private bool isInitialized = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Initialize()
    {
        if (isInitialized) return;

        if (unitStatsCalculator == null)
        {
            unitStatsCalculator = new UnitStatsCalculator();
        }

        //For testing
        try
        {
            if (statsBootstrapper == null)
            {
                statsBootstrapper = new StatsBootstrapper();
            }

            statsBootstrapper.LoadAndBuildTalents();
            statsBootstrapper.LoadAndBuildResearch();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            Debug.LogWarning("StatsBootstrapper not initialized. Skipping talent application.");
            statsBootstrapper = null;
        }

        BuildStatsLookup();
        CalculateAllFinalStats();

        isInitialized = true;
    }

    public void RecalculateAllFinalStats()
    {
        finalStatsByUnit.Clear();
        ReloadPlayerData();
        CalculateAllFinalStats();
    }

    // Player-spawnable units come from the SpawnDatabase; others (summons) are added on first use by GetStats(definition).
    // Enemies don't need to be here: GetEnemyStats takes their definition directly.
    private void BuildStatsLookup()
    {
        unitStatsByUnitKey.Clear();
        foreach (var spawn in SpawnDatabase.Instance.GetAllSpawns())
        {
            AddUnitStats(spawn.Stats);
        }
    }

    private void AddUnitStats(UnitStatsDefinition unitStats)
    {
        if (unitStats == null) return;
        string unitKey = unitStats.name.ToLowerInvariant();
        unitStatsByUnitKey[unitKey] = unitStats;
    }

    private StatBlock CalculateFinalStats(UnitStatsDefinition baseStats, string unitKey, Dictionary<ResearchCategory, List<AppliedStatModifier>> categoryModifiers)
    {
        ResearchCategory category = baseStats.category;

        StatBlock finalStats = BuildFinalStatsFromBase(baseStats);

        List<AppliedStatModifier> modifiers = new();

        //add research
        if (categoryModifiers.TryGetValue(category, out var categoryMods))
        {
            modifiers.AddRange(categoryMods);
        }

        //add talents
        if (statsBootstrapper.TalentsByUnit.TryGetValue(unitKey, out var unitTalents))
        {
            foreach (var talent in unitTalents)
            {
                modifiers.Add(new AppliedStatModifier
                {
                    Effects = talent.effects,
                    Stacks = talent.purchased
                });
            }
        }

        unitStatsCalculator.ApplyModifiers(finalStats, baseStats, modifiers);

        return finalStats;
    }

    //public for testing, look into moving it to its own service
    public void CalculateAllFinalStats()
    {
        if (statsBootstrapper == null) return;

        Dictionary<ResearchCategory, List<AppliedStatModifier>> categoryModifiers = GetResearchStatModifiers();

        foreach (var kvp in unitStatsByUnitKey)
        {
            StatBlock finalStats = CalculateFinalStats(kvp.Value, kvp.Key, categoryModifiers);

            finalStatsByUnit[kvp.Key] = finalStats;
        }
    }

    public void CalculateFinalStatsByKey(string unitKey)
    {
        if (statsBootstrapper == null) return;

        Dictionary<ResearchCategory, List<AppliedStatModifier>> categoryModifiers = GetResearchStatModifiers();

        if (!unitStatsByUnitKey.TryGetValue(unitKey, out var unit))
        {
            Debug.LogError($"No stats found for unitKey: {unitKey}");
            return;
        }

        UnitStatsDefinition baseStats = unit;

        StatBlock finalStats = CalculateFinalStats(baseStats, unitKey, categoryModifiers);

        finalStatsByUnit[unitKey] = finalStats;
    }

    public Dictionary<ResearchCategory, List<AppliedStatModifier>> GetResearchStatModifiers()
    {
        Dictionary<ResearchCategory, List<AppliedStatModifier>> categoryModifiers = new();

        foreach (var kvp in statsBootstrapper.ResearchByCategory)
        {
            var modifiers = new List<AppliedStatModifier>();

            foreach (var research in kvp.Value)
            {
                modifiers.Add(new AppliedStatModifier
                {
                    Effects = research.effects,
                    Stacks = research.purchased
                });
            }

            categoryModifiers[kvp.Key] = modifiers;
        }
        return categoryModifiers;
    }

    // Player unit stats; units outside the SpawnDatabase (e.g. summons) are calculated on first use, then kept up to date by RecalculateAllFinalStats
    public StatBlock GetStats(UnitStatsDefinition definition)
    {
        string unitKey = definition.name.ToLowerInvariant();

        if (finalStatsByUnit.TryGetValue(unitKey, out var stats))
            return stats;

        if (statsBootstrapper == null) return null;

        AddUnitStats(definition);
        stats = CalculateFinalStats(definition, unitKey, GetResearchStatModifiers());
        finalStatsByUnit[unitKey] = stats;

        return stats;
    }

    // Enemies use definition stats (no player progression) plus wave scaling
    public StatBlock GetEnemyStats(UnitStatsDefinition baseStats, WaveController.EnemyScalingContext scaling)
    {
        int delayWaves = 10; // Delay scaling for the first 10 waves

        StatBlock finalStats = BuildFinalStatsFromBase(baseStats);

        if (finalStats == null)
        {
            Debug.LogError("Failed to get finalstats");
            return null;
        }

        //last bit here is to delay scaling from being applied to the first 10 waves.
        if (scaling.waveIndex < delayWaves)
        {
            return finalStats;
        }

        return unitStatsCalculator.CalculateEnemyStats(scaling.waveIndex - delayWaves, finalStats, scaling.isRanged);
    }

    private StatBlock BuildFinalStatsFromBase(UnitStatsDefinition unitBaseStats)
    {
        if (unitBaseStats == null)
        {
            Debug.LogError("Failed to get unitBaseStats in BuildStatsFromBase");
            return null;
        }

        return unitBaseStats.ToStatBlock();
    }

    public void ReloadPlayerData()
    {
        statsBootstrapper.ReloadPlayerData();
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
            $"  Mass: {stats.Mass:F1}\n" +
            $"  Splash Radius: {stats.SplashRadius:F1}\n" +
            $"  Armor: {stats.Armor:F1}"
        );
    }

}

