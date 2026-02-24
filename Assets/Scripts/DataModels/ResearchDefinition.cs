using System.Collections.Generic;

public class ResearchDefinition
{
    public string Id;
    public string Name;
    public string Description;
    public int MaxLevel;

    public ResearchCategory Category;   // Unit, Tower, Castle, Economy, GlobalAbility

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
