#nullable disable // Unity code is nullable-oblivious; the Cloud Code module compiles this file with nullable on
public enum ResearchStartCheck
{
    Ok,
    UnknownResearch,
    // One research per category at a time
    CategoryBusy,
    MaxedOut,
    PrerequisitesNotMet,
    NotEnoughCurrency
}

// Research rules on the player's research state. Times are unix seconds passed in by the caller, so the
// server can use its own clock. Shared with the Cloud Code module (CloudCode/EmberfallModule), so keep this
// plain C#: no UnityEngine.
public static class ResearchRules
{
    public static int GetLevel(PlayerResearchState state, string researchId)
    {
        return state.CompletedResearch.TryGetValue(researchId, out int level) ? level : 0;
    }

    public static ActiveResearch GetActiveInCategory(PlayerResearchState state, ResearchCategory category)
    {
        return state.ActiveResearch.Find(active => active.ResearchCategory == category);
    }

    // Cost and duration of researching up to targetLevel
    public static int GetCost(ResearchDefinition research, int targetLevel) =>
        research.CostScaling.GetAmountForLevel(targetLevel);

    public static int GetDurationSeconds(ResearchDefinition research, int targetLevel) =>
        research.TimeScaling.GetAmountForLevel(targetLevel);

    public static bool ArePrerequisitesMet(PlayerResearchState state, ResearchPrerequisite[] prerequisites)
    {
        if (prerequisites == null)
            return true;

        foreach (var prerequisite in prerequisites)
        {
            if (!string.IsNullOrEmpty(prerequisite.RequiredResearchId) &&
                GetLevel(state, prerequisite.RequiredResearchId) < prerequisite.RequiredLevel)
            {
                return false;
            }

            // RequiredAchievementId is ignored until achievements exist
        }

        return true;
    }

    public static ResearchStartCheck CanStart(ResearchTree tree, PlayerResearchState state, string researchId, int cinders)
    {
        ResearchDefinition research = tree.GetResearchById(researchId);

        if (research == null)
            return ResearchStartCheck.UnknownResearch;

        if (GetActiveInCategory(state, research.Category) != null)
            return ResearchStartCheck.CategoryBusy;

        int currentLevel = GetLevel(state, researchId);

        if (currentLevel >= research.MaxLevel)
            return ResearchStartCheck.MaxedOut;

        if (!ArePrerequisitesMet(state, research.Prerequisites))
            return ResearchStartCheck.PrerequisitesNotMet;

        if (GetCost(research, currentLevel + 1) > cinders)
            return ResearchStartCheck.NotEnoughCurrency;

        return ResearchStartCheck.Ok;
    }

    // Starts the next level (check CanStart first). Taking the Cinders from the balance is the caller's job.
    public static ActiveResearch Start(ResearchTree tree, PlayerResearchState state, string researchId, long now)
    {
        ResearchDefinition research = tree.GetResearchById(researchId);
        int targetLevel = GetLevel(state, researchId) + 1;

        var active = new ActiveResearch(research.Category, research.Id, targetLevel, research.Name, now);
        state.ActiveResearch.Add(active);

        return active;
    }

    public static long GetEndTime(ResearchTree tree, ActiveResearch active)
    {
        ResearchDefinition research = tree.GetResearchById(active.ResearchId);
        return active.StartTime + GetDurationSeconds(research, active.TargetLevel);
    }

    public static bool IsDone(ResearchTree tree, ActiveResearch active, long now) => now >= GetEndTime(tree, active);

    public static void Complete(PlayerResearchState state, ActiveResearch active)
    {
        state.ActiveResearch.Remove(active);
        state.CompletedResearch[active.ResearchId] = active.TargetLevel;
    }
}
