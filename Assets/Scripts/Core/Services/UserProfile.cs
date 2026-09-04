using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using UnityEngine;

[DisallowMultipleComponent]
public class UserProfile : MonoBehaviour
{
    public static UserProfile Instance { get; private set; }

    public string userName;
    public int currency;
    public Dictionary<DifficultyLevel, double> UserHighScore { get; private set; } = new Dictionary<DifficultyLevel, double>
    {
        { DifficultyLevel.Easy, 0 },
        { DifficultyLevel.Medium, 0 },
        { DifficultyLevel.Hard, 0 }
    };

    public bool IsLoggedIn() => AuthenticationService.Instance.IsSignedIn;

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

    public async Task<Dictionary<DifficultyLevel, double>> GetUserScore()
    {
        UserHighScore = await LeaderboardService.Instance.GetUserScores();

        return UserHighScore;
    }
}
