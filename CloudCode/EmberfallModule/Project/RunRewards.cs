using System;
using System.Collections.Generic;
using System.Linq;

namespace EmberfallModule;

// Cinders for finished runs (docs/cloud-save.md). The server can't see a run, so it limits the rate instead:
// runs are paid only for as many minutes as real time has passed on its clock since the last paid minute,
// with at most MaxBankedHours saved up, and each run counts at most MaxRunMinutes. Offline runs are claimed
// later the same way, so they're no easier to cheat than online ones.
public static class RunRewards
{
    public const int MaxBankedHours = 12;
    public const int MaxRunMinutes = 120;
    public const int MaxRememberedRuns = 100;
    public const int MaxRunsPerClaim = 50;

    public static ActionOutcome Claim(ServerProgress progress, IReadOnlyList<PendingRun> runs, long now)
    {
        long clock = Math.Max(progress.RewardClock, now - MaxBankedHours * 3600L);
        int cinders = 0;
        bool changed = false;

        foreach (var run in runs.Take(MaxRunsPerClaim).OrderBy(r => r.EndedAtUtc))
        {
            // Already paid (a retry), or not a run
            if (string.IsNullOrEmpty(run.RunId) || progress.ClaimedRunIds.Contains(run.RunId))
                continue;

            long availableMinutes = Math.Max(0, (now - clock) / 60);
            int minutes = (int)Math.Min(Math.Clamp(run.Minutes, 0, MaxRunMinutes), availableMinutes);

            if (minutes > 0)
            {
                cinders += CinderRewardCalculator.GetCinders(minutes, run.Difficulty);
                clock += minutes * 60L;
            }

            // Counted even when nothing was paid, so it isn't resent forever
            progress.ClaimedRunIds.Add(run.RunId);
            changed = true;
        }

        if (!changed)
            return ActionOutcome.Unchanged;

        if (progress.ClaimedRunIds.Count > MaxRememberedRuns)
        {
            progress.ClaimedRunIds.RemoveRange(0, progress.ClaimedRunIds.Count - MaxRememberedRuns);
        }

        progress.RewardClock = clock;
        progress.Currency.Add(CurrencyTypes.Cinders, cinders);

        return ActionOutcome.Applied;
    }
}
