using UnityEngine;

public class BerserkerBaseStatsComponent : BaseUnitStats
{
    public override ThreatLevel UnitPrio => ThreatLevel.High;
}
