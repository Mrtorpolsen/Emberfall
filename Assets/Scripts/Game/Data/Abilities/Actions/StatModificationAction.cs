using UnityEngine;

[CreateAssetMenu(menuName = "Game/Ability Actions/StatModification")]
public class StatModificationAction : AbilityAction
{
    public StatType statType;
    public ModifierType modifierType;
    public float value;
    public float duration; // Duration in seconds, 0 permanent

    public override void Execute(TargetRegistry registry)
    {
        if (!AbilityCooldownManager.Instance.CanUse(abilityDefinition))
            return;

        var targets = targeting.Resolve(registry);

        foreach (BaseUnitStats target in targets)
        {
            if (target == null || !target.IsAlive)
                continue;

            EffectSystem.Instance.ApplyEffect(target, new StatModifierEffect(statType, modifierType, value), duration);
        }
    }
}