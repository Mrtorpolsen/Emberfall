using System;
using System.Collections.Generic;
using System.Linq;

namespace EmberfallModule;

// The one-time import of a player's local save (closed-test players who had progress before cloud save).
// The client can't be trusted, so everything is limited to what the definitions allow.
public static class ProgressImport
{
    // Every talent in the game costs about 224k Cinders (Nov 2026), so more than this was never earned honestly
    public const int MaxCinders = 250_000;
    // Nothing earns Embers yet
    public const int MaxEmbers = 1_000;

    public static ServerProgress Sanitize(ServerProgress? local, TalentTree talents, ResearchTree research, long now)
    {
        var result = new ServerProgress
        {
            HasReceivedLoginGift = local?.HasReceivedLoginGift ?? false
        };

        if (local == null)
            return result;

        result.Currency.Cinders = Math.Clamp(local.Currency?.Cinders ?? 0, 0, MaxCinders);
        result.Currency.Embers = Math.Clamp(local.Currency?.Embers ?? 0, 0, MaxEmbers);

        ImportTalents(local.Talents, result.Talents, talents);
        ImportResearch(local.Research, result.Research, research, now);

        return result;
    }

    // Known, purchasable talents only, at most MaxPurchases levels. What was spent is recomputed from the levels
    // (costs are flat per tier), so a refund can never return more than the talents cost.
    static void ImportTalents(PlayerTalentState? local, PlayerTalentState result, TalentTree tree)
    {
        if (local?.Purchases == null)
            return;

        int spent = 0;

        foreach (var (unit, unitPurchases) in local.Purchases)
        {
            if (unitPurchases?.PurchasedTalents == null)
                continue;

            foreach (var (talentId, levels) in unitPurchases.PurchasedTalents)
            {
                Talent talent = tree.GetTalentById(unit, talentId);

                if (talent == null || !TalentRules.IsPurchasableType(talent.Type))
                    continue;

                int kept = Math.Clamp(levels, 0, talent.Purchase.MaxPurchases);

                for (int i = 0; i < kept; i++)
                {
                    TalentRules.AddLevel(result, unit, talentId);
                }

                spent += kept * talent.GetCurrentCost();
            }
        }

        if (spent > 0)
        {
            result.CurrencySpent[CurrencyTypes.Cinders] = spent;
        }
    }

    // Known research only, levels at most MaxLevel. Active research is kept when it's the next level, one per
    // category and didn't start in the future; it keeps its device start time.
    static void ImportResearch(PlayerResearchState? local, PlayerResearchState result, ResearchTree tree, long now)
    {
        if (local == null)
            return;

        foreach (var (researchId, level) in local.CompletedResearch ?? new Dictionary<string, int>())
        {
            ResearchDefinition definition = tree.GetResearchById(researchId);

            if (definition == null)
                continue;

            int kept = Math.Clamp(level, 0, definition.MaxLevel);

            if (kept > 0)
            {
                result.CompletedResearch[researchId] = kept;
            }
        }

        foreach (var active in local.ActiveResearch ?? new List<ActiveResearch>())
        {
            if (active == null)
                continue;

            ResearchDefinition definition = tree.GetResearchById(active.ResearchId);

            if (definition == null ||
                active.TargetLevel != ResearchRules.GetLevel(result, definition.Id) + 1 ||
                active.TargetLevel > definition.MaxLevel ||
                active.StartTime > now ||
                ResearchRules.GetActiveInCategory(result, definition.Category) != null)
            {
                continue;
            }

            result.ActiveResearch.Add(new ActiveResearch(definition.Category, definition.Id, active.TargetLevel, definition.Name, active.StartTime));
        }
    }
}
