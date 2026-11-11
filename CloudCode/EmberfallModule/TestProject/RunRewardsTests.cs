using EmberfallModule;

namespace TestProject;

public class RunRewardsTests
{
    const long Now = 1_800_000_000;
    const long Hour = 3600;

    static ServerProgress Progress(long rewardClock)
    {
        var progress = new ServerProgress().EnsureDefaults();
        progress.RewardClock = rewardClock;
        return progress;
    }

    static PendingRun Run(string id, int minutes, DifficultyLevel difficulty = DifficultyLevel.Medium, long endedAt = Now) =>
        new PendingRun { RunId = id, Minutes = minutes, Difficulty = difficulty, EndedAtUtc = endedAt };

    static int Reward(int minutes, DifficultyLevel difficulty = DifficultyLevel.Medium) =>
        CinderRewardCalculator.GetCinders(minutes, difficulty);

    [Test]
    public void ARunIsPaidWhenRealTimeCoversIt()
    {
        var progress = Progress(rewardClock: Now - Hour);

        var outcome = RunRewards.Claim(progress, new[] { Run("a", 15) }, Now);

        Assert.That(outcome, Is.EqualTo(ActionOutcome.Applied));
        Assert.That(progress.Currency.Cinders, Is.EqualTo(Reward(15)));
        Assert.That(progress.RewardClock, Is.EqualTo(Now - Hour + 15 * 60));
        Assert.That(progress.ClaimedRunIds, Is.EqualTo(new[] { "a" }));
    }

    [Test]
    public void ARunIsPaidOnlyForMinutesThatHavePassed()
    {
        var progress = Progress(rewardClock: Now - 10 * 60);

        RunRewards.Claim(progress, new[] { Run("a", 15) }, Now);

        Assert.That(progress.Currency.Cinders, Is.EqualTo(Reward(10)));
        Assert.That(progress.RewardClock, Is.EqualTo(Now));
    }

    [Test]
    public void AtMostTwelveHoursBuildUp()
    {
        var progress = Progress(rewardClock: Now - 7 * 24 * Hour);
        var runs = Enumerable.Range(0, 10).Select(i => Run($"r{i}", 120)).ToArray();   // 20 hours of runs

        RunRewards.Claim(progress, runs, Now);

        Assert.That(progress.Currency.Cinders, Is.EqualTo(6 * Reward(120)));   // 12 hours
        Assert.That(progress.RewardClock, Is.EqualTo(Now));
    }

    [Test]
    public void ANewPlayerStartsWithTwelveHoursAvailable()
    {
        var progress = Progress(rewardClock: 0);

        RunRewards.Claim(progress, new[] { Run("a", 30) }, Now);

        Assert.That(progress.Currency.Cinders, Is.EqualTo(Reward(30)));
        Assert.That(progress.RewardClock, Is.EqualTo(Now - 12 * Hour + 30 * 60));
    }

    [Test]
    public void ARunCountsAtMostTwoHours()
    {
        var progress = Progress(rewardClock: Now - 12 * Hour);

        RunRewards.Claim(progress, new[] { Run("a", 500) }, Now);

        Assert.That(progress.Currency.Cinders, Is.EqualTo(Reward(RunRewards.MaxRunMinutes)));
    }

    [Test]
    public void ARetriedRunIsPaidOnce()
    {
        var progress = Progress(rewardClock: Now - Hour);
        RunRewards.Claim(progress, new[] { Run("a", 15) }, Now);

        var retry = RunRewards.Claim(progress, new[] { Run("a", 15) }, Now + 5);

        Assert.That(retry, Is.EqualTo(ActionOutcome.Unchanged));
        Assert.That(progress.Currency.Cinders, Is.EqualTo(Reward(15)));
    }

    [Test]
    public void DifficultyScalesTheReward()
    {
        var progress = Progress(rewardClock: Now - Hour);

        RunRewards.Claim(progress, new[] { Run("a", 15, DifficultyLevel.Nightmare) }, Now);

        Assert.That(progress.Currency.Cinders, Is.EqualTo(Reward(15, DifficultyLevel.Nightmare)));
    }

    [Test]
    public void ARunWithNoTimeLeftIsStillMarkedClaimed()
    {
        var progress = Progress(rewardClock: Now);

        var outcome = RunRewards.Claim(progress, new[] { Run("a", 15) }, Now);

        Assert.That(outcome, Is.EqualTo(ActionOutcome.Applied));
        Assert.That(progress.Currency.Cinders, Is.Zero);
        Assert.That(progress.ClaimedRunIds, Does.Contain("a"));
    }

    [Test]
    public void OnlyTheLastRunIdsAreRemembered()
    {
        var progress = Progress(rewardClock: Now - 12 * Hour);
        progress.ClaimedRunIds.AddRange(Enumerable.Range(0, RunRewards.MaxRememberedRuns).Select(i => $"old{i}"));

        RunRewards.Claim(progress, new[] { Run("new", 1) }, Now);

        Assert.That(progress.ClaimedRunIds, Has.Count.EqualTo(RunRewards.MaxRememberedRuns));
        Assert.That(progress.ClaimedRunIds.Last(), Is.EqualTo("new"));
        Assert.That(progress.ClaimedRunIds, Does.Not.Contain("old0"));
    }

    [Test]
    public void OldProgressIsUpgradedToVersion2()
    {
        var progress = new ServerProgress { Version = 1, ClaimedRunIds = null! }.EnsureDefaults();

        Assert.That(progress.Version, Is.EqualTo(2));
        Assert.That(progress.ClaimedRunIds, Is.Empty);
        Assert.That(progress.RewardClock, Is.Zero);
    }
}
