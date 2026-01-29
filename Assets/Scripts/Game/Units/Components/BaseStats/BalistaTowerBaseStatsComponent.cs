using System.Collections.Generic;
using UnityEngine;

public class BalistaTowerStatsBaseStatsComponent : RangedUnitStats, ITargetingAgent
{
    protected IReadOnlyList<ThreatLevel> preferredPriorities = new List<ThreatLevel>
    {
        ThreatLevel.Boss,
        ThreatLevel.Immidate,
        ThreatLevel.Elite
    };

    public IReadOnlyList<ThreatLevel> PreferredPriorities => preferredPriorities;
}
