public enum AbilityTriggerType
{
    OnCooldown,
}

public static class AbilityTriggerFactory
{
    public static AbilityTrigger Create(
        AbilityTriggerType triggerType,
        BaseUnitStats owner,
        AbilityDefinition ability)
    {
        return triggerType switch
        {
            AbilityTriggerType.OnCooldown =>
                new OnCooldownAbilityTrigger(owner, ability),

            _ => throw new System.ArgumentOutOfRangeException(
                nameof(triggerType),
                triggerType,
                "Unknown ability trigger type."
            )
        };
    }
}

