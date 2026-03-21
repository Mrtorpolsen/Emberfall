using System.Collections;
using UnityEngine;

public class StatModifierEffect : IEffect
{
    public EffectId Id => EffectId.StatModifier;

    private StatModifier modifier;

    public StatModifierEffect(StatType stat, ModifierType type, float value)
    {
        modifier = new StatModifier
        {
            Stat = stat,
            Type = type,
            Value = value,
        };
    }

    public void OnApply(BaseUnitStats target)
    {
        target.AddModifier(modifier);
    }

    public void OnExpire(BaseUnitStats target)
    {
        target.RemoveModifier(modifier);
    }

    public void Tick(BaseUnitStats target, float deltaTime)
    {
        // Nothing needed for simple stat modifiers
    }
}
