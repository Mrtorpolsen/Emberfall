using System;
using System.Threading.Tasks;

public class ResearchNodeDefinition
{
    public string name;
    public string researchLevelCurrent;
    public string researchLevelNext;
    public string description;
    public string researchTime;
    public string cost;
    public ResearchCategory category;
    public Func<Task> onClick;
}
