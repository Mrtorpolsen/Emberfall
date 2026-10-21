using System;

public abstract class AbilityTrigger
{
    protected readonly BaseUnitStats owner;
    protected readonly AbilityDefinition abilityDefinition;

    public event Action Triggered;

    protected AbilityTrigger(BaseUnitStats owner, AbilityDefinition abilityDefinition)
    {
        this.owner = owner;
        this.abilityDefinition = abilityDefinition;
    }

    protected void Trigger()
    {
        Triggered?.Invoke();
    }

    public virtual void Initialize() { }

    public virtual void Tick() { }

    public virtual void Dispose() { }
}