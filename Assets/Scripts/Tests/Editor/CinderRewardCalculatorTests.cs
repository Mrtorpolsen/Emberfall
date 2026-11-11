using NUnit.Framework;

public class CinderRewardCalculatorTests
{
    [TestCase(0)]
    [TestCase(-5)]
    public void UnderAMinuteEarnsNothing(int minutes)
    {
        Assert.That(CinderRewardCalculator.GetCinders(minutes, DifficultyLevel.Medium), Is.EqualTo(0));
    }

    [TestCase(1, 40)]
    [TestCase(10, 374)]
    [TestCase(20, 864)]
    public void MediumUsesTheTable(int minutes, int expected)
    {
        Assert.That(CinderRewardCalculator.GetCinders(minutes, DifficultyLevel.Medium), Is.EqualTo(expected));
    }

    [Test]
    public void PastTheTableEachMinuteAddsTwenty()
    {
        Assert.That(CinderRewardCalculator.GetCinders(25, DifficultyLevel.Medium), Is.EqualTo(864 + 5 * 20));
    }

    [TestCase(DifficultyLevel.Easy, 691)]      // 864 * 0.8
    [TestCase(DifficultyLevel.Hard, 1036)]     // 864 * 1.2, truncated
    [TestCase(DifficultyLevel.Nightmare, 1296)] // 864 * 1.5
    [TestCase(DifficultyLevel.Tutorial, 0)]
    public void DifficultyScalesTheReward(DifficultyLevel difficulty, int expected)
    {
        Assert.That(CinderRewardCalculator.GetCinders(20, difficulty), Is.EqualTo(expected));
    }
}
