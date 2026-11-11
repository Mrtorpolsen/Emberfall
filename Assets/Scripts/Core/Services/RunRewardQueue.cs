using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

// Finished runs whose Cinders the server hasn't granted yet (SaveGame.PendingRuns). A run is queued when it
// ends and claimed right away; offline or on a failure it stays queued and is claimed on reconnect or at the
// next launch. The server pays each run once (by RunId) and limits the rate (RunRewards in the module).
public static class RunRewardQueue
{
    // The server handles at most this many per claim (RunRewards.MaxRunsPerClaim)
    private const int MaxRunsPerClaim = 50;

    private static bool claiming;
    private static bool listening;

    public static void Add(int minutes, DifficultyLevel difficulty)
    {
        // Nothing to claim: under a minute, or the tutorial
        if (CinderRewardCalculator.GetCinders(minutes, difficulty) <= 0)
            return;

        SaveService.Instance.Current.PendingRuns.Add(new PendingRun
        {
            RunId = Guid.NewGuid().ToString("N"),
            Difficulty = difficulty,
            Minutes = minutes,
            EndedAtUtc = ServerClock.Now
        });

        SaveService.Instance.Save();
        _ = ClaimAsync();
    }

    public static void ListenForReconnect()
    {
        if (listening || IdentityService.Instance == null)
            return;

        IdentityService.Instance.Reconnected += () => _ = ClaimAsync();
        listening = true;
    }

    public static async Task ClaimAsync()
    {
        var save = SaveService.Instance != null ? SaveService.Instance.Current : null;

        if (claiming || save == null || save.PendingRuns.Count == 0 || !ServerActions.IsOnline)
            return;

        claiming = true;

        try
        {
            List<PendingRun> batch = save.PendingRuns.Take(MaxRunsPerClaim).ToList();

            var response = await ServerActions.SendAsync(module => module.ClaimRunRewards(JsonConvert.SerializeObject(batch)));

            // Kept for the next try
            if (response == null || !string.IsNullOrEmpty(response.Error))
                return;

            // The server has handled every run it was sent (paid, or already paid before)
            var sent = new HashSet<string>(batch.Select(run => run.RunId));
            SaveService.Instance.Current.PendingRuns.RemoveAll(run => sent.Contains(run.RunId));
            await SaveService.Instance.SaveAsync();
        }
        finally
        {
            claiming = false;
        }
    }
}
