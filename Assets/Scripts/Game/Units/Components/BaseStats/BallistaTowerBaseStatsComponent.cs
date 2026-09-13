using System.Collections.Generic;

public class BallistaTowerStatsBaseStatsComponent : TowerUnitStats, ITargetingAgent
{
    protected IReadOnlyList<ThreatLevel> preferredPriorities = new List<ThreatLevel>
    {
        ThreatLevel.Boss,
        ThreatLevel.Immidate,
        ThreatLevel.Elite,
        ThreatLevel.High
    };

    public IReadOnlyList<ThreatLevel> PreferredPriorities => preferredPriorities;
}
