using System;
using System.Collections.Generic;

[Serializable]
public class PlayerResearchState
{
    public Dictionary<string, int> CompletedResearch = new();   //id,stacks
    public List<ActiveResearch> ActiveResearches = new();
}

[Serializable]
public class ActiveResearch
{
    public string ResearchId;
    public int TargetLevel;        // the level being researched
    public long StartTime; // unix timestamp
}