using UnityEngine;

[CreateAssetMenu(menuName = "Game/Ability Triggers/Cooldown")]
public class CooldownTriggerDefinition : AbilityTriggerDefinition
{
    public override AbilityTrigger Create(BaseUnitStats owner, AbilityDefinition abilityDefinition)
    {
        return new OnCooldownAbilityTrigger(owner, abilityDefinition);
    }
}