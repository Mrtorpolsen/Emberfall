namespace EmberfallModule;

// Smaller progress actions that need no definitions
public static class ProgressActions
{
    // Cinders for finishing the gameplay tutorial, once per player
    public const int TutorialGiftCinders = 2000;

    // Gives back everything recorded as spent on talents and clears them. Nothing to refund (an earlier try went
    // through) changes nothing.
    public static ActionOutcome RefundTalents(ServerProgress progress)
    {
        var talents = progress.Talents;

        if (talents.Purchases.Count == 0 && talents.CurrencySpent.Count == 0)
            return ActionOutcome.Unchanged;

        foreach (var (currency, amount) in talents.CurrencySpent)
        {
            if (amount > 0)
            {
                progress.Currency.Add(currency, amount);
            }
        }

        talents.Purchases.Clear();
        talents.CurrencySpent.Clear();

        return ActionOutcome.Applied;
    }

    public static ActionOutcome ClaimTutorialGift(ServerProgress progress)
    {
        if (progress.HasReceivedLoginGift)
            return ActionOutcome.Unchanged;

        progress.Currency.Add(CurrencyTypes.Cinders, TutorialGiftCinders);
        progress.HasReceivedLoginGift = true;

        return ActionOutcome.Applied;
    }
}
