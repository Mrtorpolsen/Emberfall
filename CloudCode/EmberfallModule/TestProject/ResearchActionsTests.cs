using EmberfallModule;

namespace TestProject;

public class ResearchActionsTests
{
    const long Now = 1_800_000_000;

    static ResearchTree Tree => DefinitionsStore.Research;

    // A research without prerequisites and with several levels, from the real definitions
    static ResearchDefinition Research() => Tree.ResearchByCategoryEnum.Values.SelectMany(list => list)
        .First(r => r.MaxLevel > 1 && (r.Prerequisites == null || r.Prerequisites.Length == 0));

    static ServerProgress WithCinders(int cinders)
    {
        var progress = new ServerProgress().EnsureDefaults();
        progress.Currency.Cinders = cinders;
        return progress;
    }

    static int Duration(ResearchDefinition research, int level) => ResearchRules.GetDurationSeconds(research, level);

    [Test]
    public void StartChargesAndUsesTheServersTime()
    {
        var research = Research();
        int cost = ResearchRules.GetCost(research, 1);
        var progress = WithCinders(cost + 5);

        var outcome = ResearchActions.Start(progress, Tree, research.Id, fromLevel: 0, Now);

        Assert.That(outcome, Is.EqualTo(ActionOutcome.Applied));
        Assert.That(progress.Currency.Cinders, Is.EqualTo(5));
        var active = progress.Research.ActiveResearch.Single();
        Assert.That(active.TargetLevel, Is.EqualTo(1));
        Assert.That(active.StartTime, Is.EqualTo(Now));
    }

    [Test]
    public void ARetryOfAStartedResearchChargesNothing()
    {
        var research = Research();
        var progress = WithCinders(ResearchRules.GetCost(research, 1) * 3);
        ResearchActions.Start(progress, Tree, research.Id, fromLevel: 0, Now);
        int cindersAfterFirst = progress.Currency.Cinders;

        var retry = ResearchActions.Start(progress, Tree, research.Id, fromLevel: 0, Now + 5);

        Assert.That(retry, Is.EqualTo(ActionOutcome.Unchanged));
        Assert.That(progress.Currency.Cinders, Is.EqualTo(cindersAfterFirst));
        Assert.That(progress.Research.ActiveResearch.Single().StartTime, Is.EqualTo(Now));
    }

    [Test]
    public void AGameOutOfStepWithTheServerIsRefused()
    {
        var research = Research();
        var progress = WithCinders(1_000_000);
        progress.Research.CompletedResearch[research.Id] = 2;

        Assert.That(ResearchActions.Start(progress, Tree, research.Id, fromLevel: 0, Now),
            Is.EqualTo(ActionOutcome.Refused(ProgressResponse.OutOfSync)));
        Assert.That(progress.Research.ActiveResearch, Is.Empty);
    }

    [Test]
    public void NotEnoughCindersIsRefusedWithoutCharging()
    {
        var research = Research();
        var progress = WithCinders(ResearchRules.GetCost(research, 1) - 1);

        Assert.That(ResearchActions.Start(progress, Tree, research.Id, fromLevel: 0, Now),
            Is.EqualTo(ActionOutcome.Refused(nameof(ResearchStartCheck.NotEnoughCurrency))));
        Assert.That(progress.Research.ActiveResearch, Is.Empty);
    }

    [Test]
    public void CompletingBeforeTheTimeIsUpIsRefused()
    {
        var research = Research();
        var progress = WithCinders(1_000_000);
        ResearchActions.Start(progress, Tree, research.Id, fromLevel: 0, Now);

        var outcome = ResearchActions.Complete(progress, Tree, research.Id, Now + Duration(research, 1) - 1);

        Assert.That(outcome, Is.EqualTo(ActionOutcome.Refused(ProgressResponse.NotDone)));
        Assert.That(ResearchRules.GetLevel(progress.Research, research.Id), Is.Zero);
    }

    [Test]
    public void CompletingAfterTheTimeIsUpRaisesTheLevel()
    {
        var research = Research();
        var progress = WithCinders(1_000_000);
        ResearchActions.Start(progress, Tree, research.Id, fromLevel: 0, Now);

        var outcome = ResearchActions.Complete(progress, Tree, research.Id, Now + Duration(research, 1));

        Assert.That(outcome, Is.EqualTo(ActionOutcome.Applied));
        Assert.That(ResearchRules.GetLevel(progress.Research, research.Id), Is.EqualTo(1));
        Assert.That(progress.Research.ActiveResearch, Is.Empty);
    }

    [Test]
    public void CompletingWhatIsNotRunningChangesNothing()
    {
        var research = Research();
        var progress = WithCinders(0);
        progress.Research.CompletedResearch[research.Id] = 1;

        Assert.That(ResearchActions.Complete(progress, Tree, research.Id, Now), Is.EqualTo(ActionOutcome.Unchanged));
        Assert.That(ResearchRules.GetLevel(progress.Research, research.Id), Is.EqualTo(1));
    }

    [Test]
    public void ResearchRemovedFromTheDefinitionsStillCompletes()
    {
        var progress = WithCinders(0);
        progress.Research.ActiveResearch.Add(new ActiveResearch(ResearchCategory.Unit, "removed_research", 3, "Gone", Now));

        Assert.That(ResearchActions.Complete(progress, Tree, "removed_research", Now), Is.EqualTo(ActionOutcome.Applied));
        Assert.That(progress.Research.ActiveResearch, Is.Empty);
    }
}
