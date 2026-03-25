using System.Collections.Generic;
using UnityEngine;

public abstract class AbilityAction : ScriptableObject
{
    public TargetingDefinition targeting;
    public AbilityDefinition abilityDefinition;
    public abstract void Execute(TargetRegistry registry);
}