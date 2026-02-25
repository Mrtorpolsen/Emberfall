using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

public class ResearchTree
{
    [JsonProperty("research")]
    public Dictionary<string, List<ResearchDefinition>> ResearchByCategory { get; set; }

    public List<ResearchDefinition> GetResearchByCategory(string category)
    {
        ResearchByCategory.TryGetValue(category.ToLower(), out var research);
        return research;
    }
    public ResearchDefinition GetResearchById(string id)
    {
        string category = id.Split("_")[0];
        return GetResearchByCategory(category).Find(research => research.Id == id);
    }
    public ResearchCategory GetResearchCategoryById(string id)
    {
        ResearchCategory category = GetResearchById(id).Category;
        return category;
    }
    public IEnumerable<string> GetCategories(bool sorted = true)
    {
        var categories = ResearchByCategory.Keys;
        return sorted ? categories.OrderBy(c => c) : categories;
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
