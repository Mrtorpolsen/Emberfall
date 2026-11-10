// Cinders earned for surviving a run. Shared with the Cloud Code module (CloudCode/EmberfallModule),
// which grants run rewards on the server, so keep this plain C#: no UnityEngine, no services.
public static class CinderRewardCalculator
{
    private static readonly int[] cindersPerMinute = new int[]
    {
        40, 66, 98, 132, 168, 206, 246, 288, 330, 374,
        420, 466, 512, 560, 610, 660, 710, 760, 812, 864
    };

    // Past the table, each extra minute adds this many
    private const int CindersPerMinuteAfterTable = 20;

    public static int GetCinders(int minutes, DifficultyLevel difficulty)
    {
        if (minutes < 1) return 0;

        float multiplier = Difficulties.Get(difficulty).RewardMultiplier;

        if (minutes > cindersPerMinute.Length)
        {
            int extraMinutes = minutes - cindersPerMinute.Length;
            return (int)((cindersPerMinute[^1] + extraMinutes * CindersPerMinuteAfterTable) * multiplier);
        }

        return (int)(cindersPerMinute[minutes - 1] * multiplier);
    }
}
