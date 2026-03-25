using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Ability Actions/Heal")]
public class HealAction : AbilityAction
{
    public ModifierType type;
    public float valueToHeal;

    public override void Execute(TargetRegistry registry)
    {
        if (!AbilityCooldownManager.Instance.CanUse(abilityDefinition))
            return;

        var targets = targeting.Resolve(registry);

        foreach (BaseUnitStats target in targets)
        {
            if (target == null || !target.IsAlive)
                continue;

            float amount = type == ModifierType.Flat ? valueToHeal : target.MaxHealth * valueToHeal;
            target.Heal(Mathf.RoundToInt(amount));
        }
    }
}