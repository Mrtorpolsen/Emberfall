using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class UnitStatsManager : MonoBehaviour
{
    public static UnitStatsManager Instance { get; private set; }
    StatsBootstrapper statsBootstrapper;

    [System.Serializable]
    public struct UnitEntry
    {
        public GameObject prefab;
    }

    [Header("References")]
    public List<UnitEntry> unitPrefabs;

    private Dictionary<string, FinalStats> finalStatsByUnit = new();
    private Dictionary<string, GameObject> prefabByUnitKey = new();


    private void Awake()
    {
        Instance = this;

        statsBootstrapper = new StatsBootstrapper();
        statsBootstrapper.LoadAndBuildTalents();

        BuildPrefabLookup();
        CalculateAllFinalStats();
    }

    private void CalculateAllFinalStats()
    {
        foreach (UnitEntry unitPrefab in unitPrefabs)
        {
            string unitKey = unitPrefab.prefab.name.ToLowerInvariant();

            FinalStats finalStats = BuildStatsFromBase(unitPrefab.prefab);

            if (finalStats == null)
            {
                Debug.LogError("Failed to get finalstats");
                continue;
            }

            ApplyTalents(unitKey, ref finalStats);

            finalStatsByUnit[unitKey] = finalStats;
        }
    }

    private void ApplyTalents(string unitName, ref FinalStats stats)
    {
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

        LogStats(unitName, finalStats);

        return finalStats;
    }


    private FinalStats BuildStatsFromBase(GameObject unitPrefab)
    {
        BaseUnitStats baseStats = unitPrefab.GetComponent<BaseUnitStats>();

        if (baseStats == null)
        {
            Debug.LogError("Failed to get basestats in SetFinalStats");
            return null;
        }

        LogStats(unitPrefab.name, baseStats);

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

    private void BuildPrefabLookup()
    {
        prefabByUnitKey.Clear();

        foreach (UnitEntry entry in unitPrefabs)
        {
            if (entry.prefab == null)
                continue;

            string unitKey = entry.prefab.name.ToLowerInvariant();

            if (prefabByUnitKey.ContainsKey(unitKey))
            {
                Debug.LogError($"Duplicate unitKey detected: {unitKey}");
                continue;
            }

            prefabByUnitKey[unitKey] = entry.prefab;
        }
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
            $"  Cost: {stats.cost:F1}" +
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
            $"  Cost: {stats.Cost:F1}" +
            $"  Armor: {stats.Armor:F1}"
        );
    }

}

