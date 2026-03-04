using System;

public class ResearchCategoryNodeDefinition
{
    public bool isResearchActive;

    //Inactive research
    public ResearchCategory category;
    public Action onClick;

    //Active research
    public string researchName;
    public string researchRank;
    public string researchTimeLeft;
}
