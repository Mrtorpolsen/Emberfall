using NUnit.Framework;

public class DamageCalculatorTests
{
    [Test]
    public void ApplyArmor_NoArmor_FullDamage()
    {
        Assert.AreEqual(50, DamageCalculator.ApplyArmor(50, 0));
    }

    [Test]
    public void ApplyArmor_HundredArmor_HalvesDamage()
    {
        Assert.AreEqual(25, DamageCalculator.ApplyArmor(50, 100));
    }

    [Test]
    public void ApplyArmor_AlwaysAtLeastOne()
    {
        Assert.AreEqual(1, DamageCalculator.ApplyArmor(1, 1000));
    }

    [Test]
    public void RollAttackDamage_RollUnderChance_Crits()
    {
        int damage = DamageCalculator.RollAttackDamage(20, critChance: 0.25f, critDamage: 1.5f, roll: 0.1f, out bool isCrit);

        Assert.IsTrue(isCrit);
        Assert.AreEqual(30, damage);
    }

    [Test]
    public void RollAttackDamage_RollOverChance_NoCrit()
    {
        int damage = DamageCalculator.RollAttackDamage(20, critChance: 0.25f, critDamage: 1.5f, roll: 0.5f, out bool isCrit);

        Assert.IsFalse(isCrit);
        Assert.AreEqual(20, damage);
    }

    [Test]
    public void RollAttackDamage_NoCritChance_NeverCrits()
    {
        DamageCalculator.RollAttackDamage(20, critChance: 0f, critDamage: 1.5f, roll: 0f, out bool isCrit);

        Assert.IsFalse(isCrit);
    }
}
