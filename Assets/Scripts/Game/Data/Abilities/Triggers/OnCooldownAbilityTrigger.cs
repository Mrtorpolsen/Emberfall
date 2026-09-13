public class OnCooldownAbilityTrigger : AbilityTrigger
{
    private string cooldownKey;

    public OnCooldownAbilityTrigger(BaseUnitStats owner,
        AbilityDefinition abilityDefinition)
        : base(owner, abilityDefinition)
    {
    }

    public override void Initialize()
    {
        cooldownKey = AbilityCooldownManager.Instance.GetCooldownKey(
            abilityDefinition,
            owner
        );

        AbilityCooldownManager.Instance.OnCooldownComplete += OnCooldownComplete;

        Trigger();
    }

    private void OnCooldownComplete(string key)
    {
        if (key != cooldownKey)
            return;

        Trigger();
    }

    public override void Dispose()
    {
        AbilityCooldownManager.Instance.OnCooldownComplete -= OnCooldownComplete;
    }
}