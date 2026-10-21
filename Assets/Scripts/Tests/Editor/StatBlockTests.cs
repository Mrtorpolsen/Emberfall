using NUnit.Framework;

public class StatBlockTests
{
    [Test]
    public void SetThenGet_ReturnsValue()
    {
        var block = new StatBlock();

        block.Set(StatType.AttackRange, 2.5f);
        block.Add(StatType.AttackRange, 0.5f);

        Assert.AreEqual(3f, block.Get(StatType.AttackRange));
        Assert.AreEqual(0f, block.Get(StatType.Armor));
    }

    [Test]
    public void Clone_IsIndependent()
    {
        var block = new StatBlock();
        block.Set(StatType.Health, 100f);

        var clone = block.Clone();
        clone.Set(StatType.Health, 50f);

        Assert.AreEqual(100f, block.Get(StatType.Health));
        Assert.AreEqual(50f, clone.Get(StatType.Health));
    }

    [Test]
    public void GetInt_RoundsOnRead()
    {
        var block = new StatBlock();
        block.Set(StatType.Health, 104.6f);

        Assert.AreEqual(105, block.GetInt(StatType.Health));
    }
}
