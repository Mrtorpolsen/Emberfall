namespace EmberfallModule;

// What an action did to the progress: Changed means it must be written; Error is a ProgressResponse code
public readonly record struct ActionOutcome(bool Changed, string? Error)
{
    public static readonly ActionOutcome Applied = new(true, null);
    public static readonly ActionOutcome Unchanged = new(false, null);
    public static ActionOutcome Refused(string error) => new(false, error);
}

public static class TalentPurchase
{
    // Buys the level after fromLevel. fromLevel is how many levels the game thought the player owned: if the
    // server already has one more, an earlier try went through (the answer was lost), so nothing is bought again.
    public static ActionOutcome Apply(ServerProgress progress, TalentTree tree, string unit, string talentId, int fromLevel)
    {
        int owned = TalentRules.GetOwned(progress.Talents, unit, talentId);

        if (owned == fromLevel + 1)
            return ActionOutcome.Unchanged;

        if (owned != fromLevel)
            return ActionOutcome.Refused(ProgressResponse.OutOfSync);

        var check = TalentRules.CanPurchase(tree, progress.Talents, unit, talentId, progress.Currency.Cinders);

        if (check != TalentPurchaseCheck.Ok)
            return ActionOutcome.Refused(check.ToString());

        int cost = tree.GetTalentById(unit, talentId).GetCurrentCost();
        progress.Currency.Spend(CurrencyTypes.Cinders, cost);
        TalentRules.ApplyPurchase(progress.Talents, unit, talentId, cost);

        return ActionOutcome.Applied;
    }
}
