using System;
using System.Threading.Tasks;

public class ResearchNodeDefinition
{
    public string name;
    public int researchLevelCurrent;
    public int researchLevelNext;
    public int maxLevel;
    public int cost;
    public string description;
    public string researchTime;
    public ResearchCategory category;
    public Func<Task> onClick;
}
