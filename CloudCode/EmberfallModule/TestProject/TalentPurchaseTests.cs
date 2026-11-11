using EmberfallModule;

namespace TestProject;

public class TalentPurchaseTests
{
    static TalentTree Tree => DefinitionsStore.Talents;

    // A stat talent with no prerequisites and several levels, from the real definitions
    static (string Unit, Talent Talent) BuyableTalent()
    {
        foreach (var (unit, talents) in Tree.TalentsByUnit)
        {
            var talent = talents.FirstOrDefault(t =>
                t.Type == TalentType.StatModifier && t.Purchase.MaxPurchases > 1 &&
                (t.Prerequisites == null || t.Prerequisites.Length == 0));

            if (talent != null)
                return (unit, talent);
        }

        throw new InvalidOperationException("No buyable stat talent in Talents.json");
    }

    static ServerProgress WithCinders(int cinders)
    {
        var progress = new ServerProgress().EnsureDefaults();
        progress.Currency.Cinders = cinders;
        return progress;
    }

    [Test]
    public void BuyingTakesTheCostAndAddsALevel()
    {
        var (unit, talent) = BuyableTalent();
        int cost = talent.GetCurrentCost();
        var progress = WithCinders(cost + 10);

        var outcome = TalentPurchase.Apply(progress, Tree, unit, talent.Id, fromLevel: 0);

        Assert.That(outcome, Is.EqualTo(ActionOutcome.Applied));
        Assert.That(progress.Currency.Cinders, Is.EqualTo(10));
        Assert.That(TalentRules.GetOwned(progress.Talents, unit, talent.Id), Is.EqualTo(1));
        Assert.That(progress.Talents.CurrencySpent[CurrencyTypes.Cinders], Is.EqualTo(cost));
    }

    [Test]
    public void ARetryOfAnAppliedPurchaseBuysNothing()
    {
        var (unit, talent) = BuyableTalent();
        var progress = WithCinders(talent.GetCurrentCost() * 3);
        TalentPurchase.Apply(progress, Tree, unit, talent.Id, fromLevel: 0);
        int cindersAfterFirst = progress.Currency.Cinders;

        var retry = TalentPurchase.Apply(progress, Tree, unit, talent.Id, fromLevel: 0);

        Assert.That(retry, Is.EqualTo(ActionOutcome.Unchanged));
        Assert.That(progress.Currency.Cinders, Is.EqualTo(cindersAfterFirst));
        Assert.That(TalentRules.GetOwned(progress.Talents, unit, talent.Id), Is.EqualTo(1));
    }

    [Test]
    public void GameThinkingThePlayerOwnsMoreIsOutOfStep()
    {
        var (unit, talent) = BuyableTalent();
        var progress = WithCinders(100_000);

        var outcome = TalentPurchase.Apply(progress, Tree, unit, talent.Id, fromLevel: 2);

        Assert.That(outcome, Is.EqualTo(ActionOutcome.Refused(ProgressResponse.OutOfSync)));
        Assert.That(progress.Currency.Cinders, Is.EqualTo(100_000));
    }

    [Test]
    public void ServerOwnsTwoMoreThanTheGameThinksIsOutOfStep()
    {
        var (unit, talent) = BuyableTalent();
        var progress = WithCinders(100_000);
        TalentRules.AddLevel(progress.Talents, unit, talent.Id);
        TalentRules.AddLevel(progress.Talents, unit, talent.Id);

        Assert.That(TalentPurchase.Apply(progress, Tree, unit, talent.Id, fromLevel: 0),
            Is.EqualTo(ActionOutcome.Refused(ProgressResponse.OutOfSync)));
    }

    [Test]
    public void NotEnoughCindersIsRefusedWithoutCharging()
    {
        var (unit, talent) = BuyableTalent();
        var progress = WithCinders(talent.GetCurrentCost() - 1);

        var outcome = TalentPurchase.Apply(progress, Tree, unit, talent.Id, fromLevel: 0);

        Assert.That(outcome, Is.EqualTo(ActionOutcome.Refused(nameof(TalentPurchaseCheck.NotEnoughCurrency))));
        Assert.That(progress.Currency.Cinders, Is.EqualTo(talent.GetCurrentCost() - 1));
        Assert.That(progress.Talents.Purchases, Is.Empty);
    }

    [Test]
    public void UnknownTalentIsRefused()
    {
        var (unit, _) = BuyableTalent();

        Assert.That(TalentPurchase.Apply(WithCinders(100_000), Tree, unit, "no_such_talent_T1", fromLevel: 0),
            Is.EqualTo(ActionOutcome.Refused(nameof(TalentPurchaseCheck.UnknownTalent))));
    }
}
