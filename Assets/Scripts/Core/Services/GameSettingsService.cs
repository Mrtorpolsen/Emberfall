using UnityEngine;

public class GameSettingsService : MonoBehaviour
{
    public static GameSettingsService Instance { get; private set; }

    public DifficultyLevel Difficulty { get; private set; } = DifficultyLevel.Medium;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetDifficulty(DifficultyLevel difficulty)
    {
        Difficulty = difficulty;
    }
}