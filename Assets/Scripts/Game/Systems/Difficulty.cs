public class Difficulty
{
    public DifficultyLevel Level { get; }

    public float DifficultyScaling { get; }
    public float StartingThreat { get; }
    public int ScalingDelay { get; }

    public Difficulty(
        DifficultyLevel level,
        float difficultyScaling,
        float startingThreat,
        int scalingDelay)
    {
        Level = level;
        DifficultyScaling = difficultyScaling;
        StartingThreat = startingThreat;
        ScalingDelay = scalingDelay;
    }
}
public static class Difficulties
{
    public static Difficulty Get(DifficultyLevel level)
    {
        return level switch
        {
            DifficultyLevel.Easy => Easy,
            DifficultyLevel.Medium => Medium,
            DifficultyLevel.Hard => Hard,
            _ => Medium
        };
    }
    public static readonly Difficulty Easy = new(
        DifficultyLevel.Easy,
        1.02f,
        100,
        10
    );

    public static readonly Difficulty Medium = new(
        DifficultyLevel.Medium,
        1.04f,
        150,
        6
    );

    public static readonly Difficulty Hard = new(
        DifficultyLevel.Hard,
        1.06f,
        200,
        3
    );
}