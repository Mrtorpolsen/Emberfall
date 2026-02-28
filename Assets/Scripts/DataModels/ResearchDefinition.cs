using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ResearchTree
{
    [JsonProperty("research")]
    public Dictionary<string, List<ResearchDefinition>> ResearchByCategory { get; set; }
    public Dictionary<ResearchCategory, List<ResearchDefinition>> ResearchByCategoryEnum { get; private set; }

    public List<ResearchDefinition> GetResearchByCategory(ResearchCategory category)
    {
        ResearchByCategoryEnum.TryGetValue(category, out var research);
        return research;
    }

    public ResearchDefinition GetResearchById(string id)
    {
        // extract category from id or from definition
        var research = ResearchByCategoryEnum.Values.SelectMany(list => list)
            .FirstOrDefault(r => r.Id == id);
        return research;
    }

    public ResearchCategory GetResearchCategoryById(string id)
    {
        ResearchCategory category = GetResearchById(id).Category;
        return category;
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

    public AppliedEffect[] Effects;     // Applied per level

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

public class ResearchScaling
{
    public float BaseValue;
    public float MultiplierPerLevel;
}

public class ResearchPrerequisite
{
    public string RequiredResearchId;    // specific research requirement
    public int RequiredLevel;            // minimum level required

    public string RequiredAchievementId; // optional
}
