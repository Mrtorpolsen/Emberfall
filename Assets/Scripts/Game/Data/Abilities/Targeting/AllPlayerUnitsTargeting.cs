using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu(menuName = "Game/Targeting/All Player Units")]
public class AllPlayerUnitsTargeting : TargetingDefinition
{
    public override IEnumerable<BaseUnitStats> Resolve(TargetRegistry registry)
    {
        // ToList - Need to snapshot the targets at the time of execution
        // to avoid issues with units dying or spawning during the heal process
        return registry.GetAllPlayerUnits().ToList();
    }
}