public class StatModifier
{
    // Converts a progression StatEffect (Multiply uses 1.05 = +5%) bought `stacks` times
    public static StatModifier FromEffect(StatEffect effect, int stacks)
    {
        return effect.Operation switch
        {
            EffectOperation.Add => new StatModifier { Stat = effect.Target, Type = ModifierType.Flat, Value = effect.Value * stacks },
            EffectOperation.Subtract => new StatModifier { Stat = effect.Target, Type = ModifierType.Flat, Value = -effect.Value * stacks },
            EffectOperation.Multiply => new StatModifier { Stat = effect.Target, Type = ModifierType.Percent, Value = (effect.Value - 1f) * stacks },
            _ => new StatModifier { Stat = effect.Target, Type = ModifierType.Set, Value = effect.Value }
        };
    }

    public StatType Stat;
    public ModifierType Type;
    public float Value;
}
