public class Difficulty
{
    public DifficultyLevel Level { get; }

    public float StartingThreat { get; }
    public float ThreatScaling { get; }
    public int ScalingDelay { get; }
    public float RewardMultiplier { get; }
    public bool StatScaling { get; }

    public Difficulty(
        DifficultyLevel level,
        float startingThreat,
        float threatScaling,
        int scalingDelay,
        float rewardMultiplier,
        bool statScaling = false)
    {
        Level = level;
        StartingThreat = startingThreat;
        ThreatScaling = threatScaling;
        ScalingDelay = scalingDelay;
        RewardMultiplier = rewardMultiplier;
        StatScaling = statScaling;
    }
}
public static class Difficulties
{
    public static Difficulty Get(DifficultyLevel level)
    {
        return level switch
        {
            DifficultyLevel.Tutorial => Tutorial,
            DifficultyLevel.Easy => Easy,
            DifficultyLevel.Medium => Medium,
            DifficultyLevel.Hard => Hard,
            DifficultyLevel.Nightmare => Nightmare,
            _ => Medium
        };
    }

    public static readonly Difficulty Tutorial = new(
        DifficultyLevel.Tutorial,
        100,
        1.02f,
        1,
        0f
    );

    public static readonly Difficulty Easy = new(
        DifficultyLevel.Easy,
        150,
        1.02f,
        10,
        0.8f
    );

    public static readonly Difficulty Medium = new(
        DifficultyLevel.Medium,
        200,
        1.04f,
        6,
        1f
    );

    public static readonly Difficulty Hard = new(
        DifficultyLevel.Hard,
        250,
        1.06f,
        3,
        1.2f
    );

    public static readonly Difficulty Nightmare = new(
        DifficultyLevel.Nightmare,
        300,
        1.08f,
        0,
        1.5f,
        true
    );
}