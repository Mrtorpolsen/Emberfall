using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;

public class PlayerResearchState
{
    public Dictionary<string, int> CompletedResearch = new();   //id,stacks
    public List<ActiveResearch> ActiveResearch = new();
}

public class ActiveResearch
{
    [JsonConverter(typeof(StringEnumConverter))]
    public ResearchCategory ResearchCategory;
    public string ResearchId;
    public int TargetLevel; // the level being researched
    public long StartTime;  // unix timestamp
    public string ResearchName;

    public ActiveResearch(
        ResearchCategory category,
        string researchId,
        int targetLevel,
        string researchName)
    {
        ResearchCategory = category;
        ResearchId = researchId;
        TargetLevel = targetLevel;
        StartTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        ResearchName = researchName;
    }
}