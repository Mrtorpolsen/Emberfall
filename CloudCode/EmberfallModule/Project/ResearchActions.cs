namespace EmberfallModule;

// Research actions on the server's progress, timed by the server's clock (now, unix seconds)
public static class ResearchActions
{
    // Starts the level after fromLevel (the level the game shows). If that level is already running, an earlier
    // try went through, so nothing is charged again.
    public static ActionOutcome Start(ServerProgress progress, ResearchTree tree, string researchId, int fromLevel, long now)
    {
        var state = progress.Research;
        var active = state.ActiveResearch.Find(a => a.ResearchId == researchId);

        if (active != null && active.TargetLevel == fromLevel + 1)
            return ActionOutcome.Unchanged;

        if (ResearchRules.GetLevel(state, researchId) != fromLevel)
            return ActionOutcome.Refused(ProgressResponse.OutOfSync);

        var check = ResearchRules.CanStart(tree, state, researchId, progress.Currency.Cinders);

        if (check != ResearchStartCheck.Ok)
            return ActionOutcome.Refused(check.ToString());

        int cost = ResearchRules.GetCost(tree.GetResearchById(researchId), fromLevel + 1);
        progress.Currency.Spend(CurrencyTypes.Cinders, cost);
        ResearchRules.Start(tree, state, researchId, now);

        return ActionOutcome.Applied;
    }

    // Completes the running research once its time is up. Nothing running (an earlier try completed it) is fine.
    public static ActionOutcome Complete(ServerProgress progress, ResearchTree tree, string researchId, long now)
    {
        var state = progress.Research;
        var active = state.ActiveResearch.Find(a => a.ResearchId == researchId);

        if (active == null)
            return ActionOutcome.Unchanged;

        // Research removed from the definitions can't be timed; finish it rather than leave the category stuck
        if (tree.GetResearchById(researchId) != null && !ResearchRules.IsDone(tree, active, now))
            return ActionOutcome.Refused(ProgressResponse.NotDone);

        ResearchRules.Complete(state, active);
        return ActionOutcome.Applied;
    }
}
