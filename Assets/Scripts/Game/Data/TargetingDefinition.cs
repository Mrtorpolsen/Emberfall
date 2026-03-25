using System.Collections.Generic;
using UnityEngine;

public abstract class TargetingDefinition : ScriptableObject
{
    public abstract IEnumerable<BaseUnitStats> Resolve(TargetRegistry registry);
}