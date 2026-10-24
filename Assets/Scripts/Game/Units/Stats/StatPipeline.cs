using System.Collections.Generic;

// The one rule for combining modifiers on a stat, used by progression (talents/research)
// and by runtime effects: base * (1 + sum of Percent) + sum of Flat.
// Percent values are fractions (0.1 = +10%) and only scale the base, not Flat bonuses.
// A Set overrides everything; if there are several, the last one wins.
public static class StatPipeline
{
    public static float Calculate(float baseValue, IEnumerable<StatModifier> modifiers)
    {
        float flat = 0f;
        float percent = 0f;
        bool hasSet = false;
        float setValue = 0f;

        foreach (var mod in modifiers)
        {
            switch (mod.Type)
            {
                case ModifierType.Flat:
                    flat += mod.Value;
                    break;

                case ModifierType.Percent:
                    percent += mod.Value;
                    break;

                case ModifierType.Set:
                    hasSet = true;
                    setValue = mod.Value;
                    break;
            }
        }

        if (hasSet) return setValue;

        return baseValue * (1f + percent) + flat;
    }
}
