using EmberfallModule;

namespace TestProject;

public class ProgressActionsTests
{
    static ServerProgress Progress(int cinders)
    {
        var progress = new ServerProgress().EnsureDefaults();
        progress.Currency.Cinders = cinders;
        return progress;
    }

    [Test]
    public void RefundGivesBackWhatWasSpentAndClearsTalents()
    {
        var progress = Progress(10);
        TalentRules.ApplyPurchase(progress.Talents, "fighter", "stat_health_1_T1", 25);
        TalentRules.ApplyPurchase(progress.Talents, "fighter", "stat_health_1_T1", 25);

        var outcome = ProgressActions.RefundTalents(progress);

        Assert.That(outcome, Is.EqualTo(ActionOutcome.Applied));
        Assert.That(progress.Currency.Cinders, Is.EqualTo(60));
        Assert.That(progress.Talents.Purchases, Is.Empty);
        Assert.That(progress.Talents.CurrencySpent, Is.Empty);
    }

    [Test]
    public void RefundingTwiceGivesNothingMore()
    {
        var progress = Progress(0);
        TalentRules.ApplyPurchase(progress.Talents, "fighter", "stat_health_1_T1", 25);
        ProgressActions.RefundTalents(progress);

        Assert.That(ProgressActions.RefundTalents(progress), Is.EqualTo(ActionOutcome.Unchanged));
        Assert.That(progress.Currency.Cinders, Is.EqualTo(25));
    }

    [Test]
    public void RefundGivesBackEmbersToo()
    {
        var progress = Progress(0);
        progress.Talents.CurrencySpent[CurrencyTypes.Embers] = 3;

        ProgressActions.RefundTalents(progress);

        Assert.That(progress.Currency.Embers, Is.EqualTo(3));
    }

    [Test]
    public void TutorialGiftIsGivenOnce()
    {
        var progress = Progress(100);

        Assert.That(ProgressActions.ClaimTutorialGift(progress), Is.EqualTo(ActionOutcome.Applied));
        Assert.That(ProgressActions.ClaimTutorialGift(progress), Is.EqualTo(ActionOutcome.Unchanged));

        Assert.That(progress.Currency.Cinders, Is.EqualTo(100 + ProgressActions.TutorialGiftCinders));
        Assert.That(progress.HasReceivedLoginGift, Is.True);
    }
}
