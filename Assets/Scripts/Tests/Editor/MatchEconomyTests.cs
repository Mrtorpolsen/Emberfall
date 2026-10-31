using NUnit.Framework;

public class MatchEconomyTests
{
    private static MatchEconomy CreateEconomy(float southCurrency = 300, float baseIncomePerTick = 20,
        float incomeMultiplier = 1f) =>
        new MatchEconomy(southCurrency, northCurrency: 1000, baseIncomePerTick, incomeInterval: 1f,
            incomeMultiplier, incomeUpgradeCost: 200);

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

    [Test]
    public void IncomePerTick_Fractional_RoundsToWholeNumber()
    {
        // 13 * 1.2 = 15.6
        var economy = CreateEconomy(baseIncomePerTick: 13, incomeMultiplier: 1.2f);

        Assert.AreEqual(16f, economy.IncomePerTick);
    }

    [Test]
    public void IncomePerTick_Midpoint_RoundsUp()
    {
        // 15 * 1.5 = 22.5 exactly; banker's rounding would give 22
        var economy = CreateEconomy(baseIncomePerTick: 15, incomeMultiplier: 1.5f);

        Assert.AreEqual(23f, economy.IncomePerTick);
    }

    [Test]
    public void IncomePerTick_AfterManyUpgrades_HasNoFloatDrift()
    {
        // Adding 0.2f five times leaves the multiplier slightly off 2
        var economy = CreateEconomy(southCurrency: 10000);

        for (int i = 0; i < 5; i++)
        {
            Assert.IsTrue(economy.TryUpgradeIncome());
        }

        Assert.AreEqual(40f, economy.IncomePerTick);
    }

    [Test]
    public void Tick_PaysRoundedIncome()
    {
        var economy = CreateEconomy(baseIncomePerTick: 13, incomeMultiplier: 1.2f);

        economy.Tick(1f);

        Assert.AreEqual(316f, economy.Get(Team.South));
    }
}
