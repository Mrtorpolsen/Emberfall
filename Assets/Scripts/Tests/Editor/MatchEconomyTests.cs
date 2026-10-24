using NUnit.Framework;

public class MatchEconomyTests
{
    private static MatchEconomy CreateEconomy() =>
        new MatchEconomy(southCurrency: 300, northCurrency: 1000, baseIncomePerTick: 20, incomeInterval: 1f,
            incomeMultiplier: 1f, incomeUpgradeCost: 200);

    [Test]
    public void TrySpend_NotEnough_ReturnsFalseAndKeepsCurrency()
    {
        var economy = CreateEconomy();

        Assert.IsFalse(economy.TrySpend(Team.South, 301));
        Assert.AreEqual(300, economy.Get(Team.South));
    }

    [Test]
    public void TrySpend_Enough_SpendsAndRaisesEvent()
    {
        var economy = CreateEconomy();
        float reported = -1;
        economy.CurrencyChanged += (team, amount) => reported = amount;

        Assert.IsTrue(economy.TrySpend(Team.South, 100));
        Assert.AreEqual(200, economy.Get(Team.South));
        Assert.AreEqual(200, reported);
    }

    [Test]
    public void Tick_PaysIncomeOncePerInterval()
    {
        var economy = CreateEconomy();

        economy.Tick(0.5f);
        Assert.AreEqual(300, economy.Get(Team.South));

        economy.Tick(0.5f);
        Assert.AreEqual(320, economy.Get(Team.South));
    }

    [Test]
    public void TryUpgradeIncome_SpendsAndRaisesIncomeAndCost()
    {
        var economy = CreateEconomy();

        Assert.IsTrue(economy.TryUpgradeIncome());

        Assert.AreEqual(100, economy.Get(Team.South));
        Assert.AreEqual(1.2f, economy.IncomeMultiplier, 0.0001f);
        Assert.AreEqual(24f, economy.IncomePerTick, 0.0001f);
        Assert.AreEqual(250, economy.IncomeUpgradeCost);
    }

    [Test]
    public void TryUpgradeIncome_NotEnough_ChangesNothing()
    {
        var economy = CreateEconomy();
        economy.TrySpend(Team.South, 250);

        Assert.IsFalse(economy.TryUpgradeIncome());
        Assert.AreEqual(1f, economy.IncomeMultiplier);
        Assert.AreEqual(200, economy.IncomeUpgradeCost);
    }
}
