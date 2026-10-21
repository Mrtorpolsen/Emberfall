using System;

[System.Serializable]
public class UnitAbility
{
    public AbilityDefinition ability;
    public AbilityTriggerType triggerType;
    public bool canUse;

    [NonSerialized] public AbilityTrigger trigger;
    [NonSerialized] public Action triggeredHandler;
}