using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

// Validates the talent/research JSON so data mistakes fail here instead of silently in play
public class DataValidationTests
{
    private const string TalentsPath = "Assets/Resources_moved/Data/Talents.json";
    private const string ResearchPath = "Assets/Resources_moved/Data/Research.json";

    [Test]
    public void TalentsJson_DeserializesIntoTalentTree()
    {
        string json = LoadText(TalentsPath);

        Assert.DoesNotThrow(() => JsonConvert.DeserializeObject<TalentTree>(json));
    }

    [Test]
    public void ResearchJson_DeserializesIntoResearchTree()
    {
        string json = LoadText(ResearchPath);

        Assert.DoesNotThrow(() => JsonConvert.DeserializeObject<ResearchTree>(json));
    }

    [Test]
    public void StatModifierTalents_HaveValidEffects()
    {
        var errors = new List<string>();
        var talentData = (JObject)JObject.Parse(LoadText(TalentsPath))["talentData"];

        foreach (var talent in talentData.Properties())
        {
            if ((string)talent.Value["Type"] != nameof(TalentType.StatModifier)) continue;

            ValidateEffects($"Talent '{talent.Name}'", talent.Value["Effects"], errors);
        }

        Assert.IsEmpty(errors, string.Join("\n", errors));
    }

    [Test]
    public void StatResearch_HasValidEffects()
    {
        var errors = new List<string>();
        var researchByCategory = (JObject)JObject.Parse(LoadText(ResearchPath))["research"];

        foreach (var category in researchByCategory.Properties())
        {
            foreach (var research in category.Value)
            {
                // StatsBootstrapper doesn't apply these categories as stats
                if (Enum.TryParse((string)research["Category"], true, out ResearchCategory researchCategory) &&
                    (researchCategory == ResearchCategory.GlobalAbility || researchCategory == ResearchCategory.Economy))
                    continue;

                ValidateEffects($"Research '{research["Id"]}'", research["Effects"], errors);
            }
        }

        Assert.IsEmpty(errors, string.Join("\n", errors));
    }

    [Test]
    public void SpawnDefinitions_ResolveToStats()
    {
        var errors = new List<string>();

        foreach (string guid in AssetDatabase.FindAssets("t:SpawnDefinition"))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var spawn = AssetDatabase.LoadAssetAtPath<SpawnDefinition>(path);

            if (spawn.UnitPrefab == null)
            {
                errors.Add($"{path} has no unit prefab");
                continue;
            }

            if (spawn.Stats == null)
            {
                errors.Add($"{path} has no stats");
                continue;
            }

            // Spawning looks stats up by the lowercased prefab name, which must match the stats asset name
            if (spawn.UnitPrefab.name.ToLowerInvariant() != spawn.Stats.name.ToLowerInvariant())
            {
                errors.Add($"{path}: prefab '{spawn.UnitPrefab.name}' doesn't match stats '{spawn.Stats.name}'");
            }

            var unitStats = spawn.UnitPrefab.GetComponent<BaseUnitStats>();

            if (unitStats == null)
            {
                errors.Add($"{path}: prefab '{spawn.UnitPrefab.name}' has no BaseUnitStats on its root");
            }
            else if (unitStats.statsDef != spawn.Stats)
            {
                errors.Add($"{path}: prefab '{spawn.UnitPrefab.name}' uses stats '{(unitStats.statsDef != null ? unitStats.statsDef.name : "none")}', not '{spawn.Stats.name}'");
            }
        }

        Assert.IsEmpty(errors, string.Join("\n", errors));
    }

    private static void ValidateEffects(string owner, JToken effects, List<string> errors)
    {
        if (effects == null || !effects.HasValues)
        {
            errors.Add($"{owner} has no Effects");
            return;
        }

        foreach (var effect in effects)
        {
            string target = (string)effect["Target"];

            // A missing Target would silently deserialize as StatType 0 (Health)
            if (!Enum.TryParse(target, out StatType stat) || !Enum.IsDefined(typeof(StatType), stat))
            {
                errors.Add($"{owner} has unknown Target '{target}'");
            }
            else if (stat == StatType.Income)
            {
                errors.Add($"{owner} targets Income, which is not a unit stat");
            }

            string operation = (string)effect["Operation"];

            if (!Enum.TryParse(operation, out EffectOperation op) || !Enum.IsDefined(typeof(EffectOperation), op))
            {
                errors.Add($"{owner} has unknown Operation '{operation}'");
            }

            if (effect["Value"]?.Type is not (JTokenType.Float or JTokenType.Integer))
            {
                errors.Add($"{owner} has a missing or non-numeric Value");
            }
        }
    }

    private static string LoadText(string path)
    {
        var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
        Assert.IsNotNull(asset, $"Could not load {path}");
        return asset.text;
    }
}
