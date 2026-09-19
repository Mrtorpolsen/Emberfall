public class CinderRewardCalculator
{
    private static readonly int[] cindersPerMinute = new int[]
    {
        40, 66, 98, 132, 168, 206, 246, 288, 330, 374,
        420, 466, 512, 560, 610, 660, 710, 760, 812, 864
    };

    public static int GetCinders(int minutes)
    {
        if (minutes < 1) return 0;

        if (minutes > 20)
        {
            return (int)((cindersPerMinute[19] + ((minutes - 20) * 20)) * Difficulties.Get(GameSettingsService.Instance.Difficulty).RewardMultiplier);
        }
        return (int)(cindersPerMinute[minutes - 1] * Difficulties.Get(GameSettingsService.Instance.Difficulty).RewardMultiplier);
    }
}
