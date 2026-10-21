using UnityEngine;

public abstract class AbilityTriggerDefinition : ScriptableObject
{
    public abstract AbilityTrigger Create(BaseUnitStats owner, AbilityDefinition abilityDefinition);
}