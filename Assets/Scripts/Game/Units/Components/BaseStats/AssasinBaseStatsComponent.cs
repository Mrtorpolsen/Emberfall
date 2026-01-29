using UnityEngine;

public class AssasinBaseStatsComponent : BaseUnitStats
{
    public override ThreatLevel UnitPrio => ThreatLevel.Special;
}