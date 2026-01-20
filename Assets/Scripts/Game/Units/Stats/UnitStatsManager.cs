using System.Collections.Generic;
using UnityEngine;

public class UnitStatsManager : MonoBehaviour
{
    public static UnitStatsManager Instance { get; private set; }
    StatsBootstrapper statsBootstrapper;

    private Dictionary<string, FinalStats> finalStatsByUnit = new();
    private Dictionary<string, GameObject> prefabByUnitKey = new();

    private void Awake()
    {
        Instance = this;

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
        AddPrefab(Prefabs.giantPrefab);
        AddPrefab(Prefabs.eliteFighterPrefab);
    }

    private void AddPrefab(GameObject prefab)
    {
        if (prefab == null) return;
        string unitKey = prefab.name.ToLowerInvariant();
        prefabByUnitKey[unitKey] = prefab;
    }

    private void CalculateAllFinalStats()
    {
        foreach (var kvp in prefabByUnitKey)
        {
            string unitKey = kvp.Key;
            GameObject prefab = kvp.Value;

            FinalStats finalStats = BuildStatsFromBase(prefab);
            ApplyTalents(unitKey, ref finalStats);
            finalStatsByUnit[unitKey] = finalStats;
        }
    }

    private void ApplyTalents(string unitName, ref FinalStats stats)
    {
        //for testing
        if (statsBootstrapper == null) return;

        if (!statsBootstrapper.TalentsByUnit.TryGetValue(unitName, out var unitTalents))
            return;

        foreach (var talent in unitTalents)
        {
            foreach (var effect in talent.effects)
            {
                float magnitude = effect.Operation switch
                {
                    EffectOperation.Add => effect.Value * talent.purcashed,
                    EffectOperation.Multiply => Mathf.Pow(effect.Value, talent.purcashed),
                    EffectOperation.Set => effect.Value,
                    _ => effect.Value
                };
                ApplyEffect(effect.Target, effect.Operation, magnitude, ref stats);
            }
        }
    }

    private void ApplyEffect(EffectTarget target, EffectOperation operation,
        float value, ref FinalStats stats)
    {
        switch (target)
        {
            case EffectTarget.Health:
                Apply(ref stats.health, operation, value);
                break;

            case EffectTarget.Damage:
                Apply(ref stats.attackDamage, operation, value);
                break;

            case EffectTarget.AttackSpeed:
                Apply(ref stats.attackSpeed, operation, value);
                break;

            case EffectTarget.Range:
                Apply(ref stats.attackRange, operation, value);
                break;

            case EffectTarget.Armor:
                Apply(ref stats.armor, operation, value);
                break;

            case EffectTarget.CritChance:
                Apply(ref stats.critChance, operation, value);
                break;

            case EffectTarget.CritDamage:
                Apply(ref stats.critMultiplier, operation, value);
                break;
        }
    }

    private void Apply(ref int stat, EffectOperation operation, float value)
    {
        int intValue = Mathf.RoundToInt(value);

        switch (operation)
        {
            case EffectOperation.Add:
                stat += intValue;
                break;

            case EffectOperation.Multiply:
                stat = Mathf.RoundToInt(stat * value);
                break;

            case EffectOperation.Set:
                stat = intValue;
                break;
        }
    }

    private void Apply(ref float stat, EffectOperation operation, float value)
    {
        switch (operation)
        {
            case EffectOperation.Add:
                stat += value;
                break;

            case EffectOperation.Multiply:
                stat *= value;
                break;

            case EffectOperation.Set:
                stat = value;
                break;
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

        //scaling multipliers
        float wave = scaling.waveIndex + 1;
        int hpPercent = Mathf.RoundToInt(2f * Mathf.Pow(wave, 0.85f));
        int dmgPercent = Mathf.RoundToInt(2f * Mathf.Pow(wave, 0.75f));

        float hpMultiplier = 1f + (hpPercent * 0.01f);
        float dmgMultiplier = 1f + (dmgPercent * 0.01f);

        ApplyEffect(EffectTarget.Health, EffectOperation.Multiply, hpMultiplier, ref finalStats);
        ApplyEffect(EffectTarget.Damage, EffectOperation.Multiply, dmgMultiplier, ref finalStats);

        return finalStats;
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

