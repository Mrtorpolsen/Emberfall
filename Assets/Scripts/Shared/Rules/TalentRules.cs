using System.Collections.Generic;

public enum TalentPurchaseCheck
{
    Ok,
    UnknownTalent,
    // Only stat and unit-unlock talents are sold; the others are placeholders for now
    NotPurchasable,
    MaxedOut,
    PrerequisitesNotMet,
    NotEnoughCurrency
}

// Talent purchase rules on the player's talent state. Shared with the Cloud Code module
// (CloudCode/EmberfallModule), which validates purchases on the server, so keep this plain C#: no UnityEngine.
public static class TalentRules
{
    public static bool IsPurchasableType(TalentType type) =>
        type == TalentType.StatModifier || type == TalentType.UnitUnlock;

    public static int GetOwned(PlayerTalentState state, string unit, string talentId)
    {
        if (state.Purchases.TryGetValue(unit, out var unitPurchases) &&
            unitPurchases.PurchasedTalents != null &&
            unitPurchases.PurchasedTalents.TryGetValue(talentId, out int count))
        {
            return count;
        }

        return 0;
    }

    // Levels bought in one tier of a unit's tree; talents no longer in the tree don't count
    public static int GetPointsInTier(TalentTree tree, PlayerTalentState state, string unit, int tier)
    {
        if (!state.Purchases.TryGetValue(unit, out var unitPurchases) || unitPurchases.PurchasedTalents == null)
            return 0;

        int points = 0;

        foreach (var purchase in unitPurchases.PurchasedTalents)
        {
            Talent talent = tree.GetTalentById(unit, purchase.Key);

            if (talent != null && talent.Tier == tier)
            {
                points += purchase.Value;
            }
        }

        return points;
    }

    public static bool ArePrerequisitesMet(TalentTree tree, PlayerTalentState state, string unit, TalentPrerequisite[] prerequisites)
    {
        if (prerequisites == null)
            return true;

        foreach (var prerequisite in prerequisites)
        {
            if (prerequisite.RequiredTier > 0 &&
                GetPointsInTier(tree, state, unit, prerequisite.RequiredTier) < prerequisite.RequiredPointsInTier)
            {
                return false;
            }

            // A talent in the same unit's tree, bought at least once
            if (!string.IsNullOrEmpty(prerequisite.RequiredUpgradeId) &&
                GetOwned(state, unit, prerequisite.RequiredUpgradeId) == 0)
            {
                return false;
            }

            // RequiredAchievementId is ignored until achievements exist
        }

        return true;
    }

    public static TalentPurchaseCheck CanPurchase(TalentTree tree, PlayerTalentState state, string unit, string talentId, int cinders)
    {
        Talent talent = tree.GetTalentById(unit, talentId);

        if (talent == null)
            return TalentPurchaseCheck.UnknownTalent;

        if (!IsPurchasableType(talent.Type))
            return TalentPurchaseCheck.NotPurchasable;

        if (GetOwned(state, unit, talentId) >= talent.Purchase.MaxPurchases)
            return TalentPurchaseCheck.MaxedOut;

        if (!ArePrerequisitesMet(tree, state, unit, talent.Prerequisites))
            return TalentPurchaseCheck.PrerequisitesNotMet;

        if (talent.GetCurrentCost() > cinders)
            return TalentPurchaseCheck.NotEnoughCurrency;

        return TalentPurchaseCheck.Ok;
    }

    // Records one more level and the Cinders paid for it (refunds give back CurrencySpent).
    // Taking the Cinders from the balance is the caller's job.
    public static void ApplyPurchase(PlayerTalentState state, string unit, string talentId, int cost)
    {
        AddLevel(state, unit, talentId);

        state.CurrencySpent.TryGetValue(CurrencyTypes.Cinders, out int spent);
        state.CurrencySpent[CurrencyTypes.Cinders] = spent + cost;
    }

    public static void AddLevel(PlayerTalentState state, string unit, string talentId)
    {
        if (!state.Purchases.TryGetValue(unit, out var unitPurchases))
        {
            unitPurchases = new UnitSaveData();
            state.Purchases[unit] = unitPurchases;
        }

        unitPurchases.PurchasedTalents ??= new Dictionary<string, int>();
        unitPurchases.PurchasedTalents.TryGetValue(talentId, out int count);
        unitPurchases.PurchasedTalents[talentId] = count + 1;
    }
}
