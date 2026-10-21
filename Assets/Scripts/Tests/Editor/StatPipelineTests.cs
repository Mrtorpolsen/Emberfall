using System.Collections.Generic;
using NUnit.Framework;

public class StatPipelineTests
{
    private static StatModifier Mod(ModifierType type, float value) =>
        new StatModifier { Stat = StatType.AttackDamage, Type = type, Value = value };

    [Test]
    public void NoModifiers_ReturnsBase()
    {
        Assert.AreEqual(100f, StatPipeline.Calculate(100f, new List<StatModifier>()));
    }

    [Test]
    public void Percents_AreAdditive()
    {
        var mods = new List<StatModifier> { Mod(ModifierType.Percent, 0.1f), Mod(ModifierType.Percent, 0.1f) };

        // 100 * (1 + 0.1 + 0.1)
        Assert.AreEqual(120f, StatPipeline.Calculate(100f, mods), 0.0001f);
    }

    [Test]
    public void Percent_DoesNotScaleFlat()
    {
        var mods = new List<StatModifier> { Mod(ModifierType.Flat, 10f), Mod(ModifierType.Percent, 0.2f) };

        // 100 * 1.2 + 10
        Assert.AreEqual(130f, StatPipeline.Calculate(100f, mods), 0.0001f);
    }

    [Test]
    public void Set_OverridesOtherModifiers()
    {
        var mods = new List<StatModifier> { Mod(ModifierType.Flat, 10f), Mod(ModifierType.Set, 5f), Mod(ModifierType.Percent, 0.5f) };

        Assert.AreEqual(5f, StatPipeline.Calculate(100f, mods));
    }
}
