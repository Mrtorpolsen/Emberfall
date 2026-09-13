using UnityEngine;

[CreateAssetMenu(menuName = "Game/Ability Actions/Heal")]
public class HealAction : AbilityAction
{
    public ModifierType type;
    public float valueToHeal;

    public override void Execute(AbilityContext context)
    {
        if (AbilityCooldownManager.Instance == null)
        {
            Debug.LogError($"AbilityCooldownManager instance is null. \n Name: {context.Caster.name}  \n {abilityDefinition.name}");
            return;
        }

        if (!AbilityCooldownManager.Instance.CanUse(abilityDefinition, context.Caster))
            return;

        var targets = targeting.Resolve(context.TargetRegistry);

        foreach (BaseUnitStats target in targets)
        {
            if (target == null || !target.IsAlive)
                continue;

            float amount = type == ModifierType.Flat ? valueToHeal : target.MaxHealth * valueToHeal;
            target.Heal(Mathf.RoundToInt(amount));
        }
    }
}