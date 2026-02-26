using System;

public class ResearchCategoryNodeDefinition
{
    public bool isResearchActive;

    //Inactive research
    public string categoryName;
    public Action onClick;

    //Active research
    public string researchName;
    public string researchRank;
    public string researchTimeLeft;
}
