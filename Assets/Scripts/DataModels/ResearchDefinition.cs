using JetBrains.Annotations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ResearchTree
{
    [JsonProperty("scalingPresets")]
    public Dictionary<string, ScalingPreset> ScalingPresets { get; set; }

    [JsonProperty("research")]
    public Dictionary<string, List<ResearchDefinition>> ResearchByCategory { get; set; }
    public Dictionary<ResearchCategory, List<ResearchDefinition>> ResearchByCategoryEnum { get; private set; }

    public void ApplyScalingPresets()
    {
        foreach (var list in ResearchByCategoryEnum.Values)
        {
            foreach (var research in list)
            {
                if (research.CostScaling != null)
                    research.CostScaling.ResolvedStages = ResolveScaling(research.CostScaling);

                if (research.TimeScaling != null)
                    research.TimeScaling.ResolvedStages = ResolveScaling(research.TimeScaling);
            }
        }
    }

    private ScalingStage[] ResolveScaling(ResearchScaling scaling)
    {
        if (scaling == null)
        {
            Debug.LogWarning("Scaling is null. Returning empty stages.");
            return Array.Empty<ScalingStage>();
        }

        var preset = GetScalingPreset(scaling.Preset);

        if (preset == null)
        {
            Debug.LogWarning($"Preset '{scaling.Preset}' not found for scaling. Returning empty stages.");
            return Array.Empty<ScalingStage>();
        }

        // clone preset (avoid shared references)
        var stages = preset.Stages
            .Select(s => new ScalingStage
            {
                StageNumber = s.StageNumber,
                MinLevel = s.MinLevel,
                MaxLevel = s.MaxLevel,
                MultiplierPerLevel = s.MultiplierPerLevel
            })
            .ToList();

        if (scaling.Overrides != null)
        {
            foreach (var o in scaling.Overrides)
            {
                var stage = stages.FirstOrDefault(s => s.StageNumber == o.StageNumber);
                if (stage == null)
                    continue;

                stage.MultiplierPerLevel = o.MultiplierPerLevel;
            }
        }

        return stages
            .OrderBy(s => s.MinLevel)
            .ToArray();
    }

    public ScalingPreset GetScalingPreset(string presetName)
    {
        ScalingPresets.TryGetValue(presetName, out var preset);
        return preset;
    }

    public List<ResearchDefinition> GetResearchByCategory(ResearchCategory category)
    {
        ResearchByCategoryEnum.TryGetValue(category, out var research);
        return research;
    }

    public ResearchDefinition GetResearchById(string id)
    {
        // extract category from id
        var research = ResearchByCategoryEnum.Values.SelectMany(list => list)
            .FirstOrDefault(r => r.Id == id);
        return research;
    }

    public ResearchCategory GetResearchCategoryById(string id)
    {
        var research = GetResearchById(id);
        if (research == null)
            return default;

        return research.Category;
    }

    public IEnumerable<ResearchCategory> GetCategories(bool sorted = true)
    {
        var categories = ResearchByCategoryEnum.Keys;
        return sorted ? categories.OrderBy(c => c) : categories;
    }

    public void NormalizeCategoryKeys()
    {
        ResearchByCategoryEnum = new Dictionary<ResearchCategory, List<ResearchDefinition>>();

        foreach (var kvp in ResearchByCategory)
        {
            if (Enum.TryParse<ResearchCategory>(kvp.Key, true, out var category))
            {
                ResearchByCategoryEnum[category] = kvp.Value;
            }
            else
            {
                Debug.LogWarning($"Unknown category key in JSON: {kvp.Key}");
            }
        }
    }
}

public class ResearchDefinition
{
    public string Id;
    public string Name;
    public string Description;
    public ResearchCategory Category;   // Unit, Tower, Castle, Economy, GlobalAbility

    public int MaxLevel;

    public ResearchScaling CostScaling;
    public ResearchScaling TimeScaling;

    public StatEffect[] Effects;     // Applied per level

    public ResearchPrerequisite[] Prerequisites;
}

public enum ResearchCategory
{
    Unit,
    Tower,
    Castle,
    Economy,
    GlobalAbility
}

public enum ResearchScalingType
{
    Time,
    Cost
}

public class ResearchScaling
{
    public float BaseValue;
    public string Preset;
    public ScalingOverride[] Overrides;

    [JsonIgnore]
    public ScalingStage[] ResolvedStages;

    public int GetAmountForLevel(int level)
    {
        float value = BaseValue;

        foreach (var stage in ResolvedStages)
        {
            int start = Math.Max(stage.MinLevel, 2); // skip level 1
            int end = Math.Min(stage.MaxLevel, level);

            if (end < start)
                continue;

            int count = end - start + 1;

            value *= Mathf.Pow(stage.MultiplierPerLevel, count);
        }

        return Mathf.RoundToInt(value);
    }

}

public class ScalingPreset
{
    public ScalingStage[] Stages;
}

public class ScalingStage
{
    public int StageNumber;
    public int MinLevel;
    public int MaxLevel;
    public float MultiplierPerLevel;
}

public class ScalingOverride
{
    public int StageNumber;
    public float MultiplierPerLevel;
}

public class ResearchPrerequisite
{
    public string RequiredResearchId;    // specific research requirement
    public int RequiredLevel;            // minimum level required

    public string RequiredAchievementId; // optional
}
