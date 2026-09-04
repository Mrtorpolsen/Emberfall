using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Models;
using UnityEngine;

//Needs to be initialized earlier and a singleton since its used in game scene.
public class LeaderboardService : MonoBehaviour
{
    public static LeaderboardService Instance { get; private set; }

    private string leaderboardId;
    private readonly Dictionary<DifficultyLevel, string> leaderboardIds = new()
    {
        { DifficultyLevel.Easy, "High_Scores_Easy" },
        { DifficultyLevel.Medium, "High_Scores_Medium" },
        { DifficultyLevel.Hard, "High_Scores_Hard" }
    };
    public Dictionary<DifficultyLevel, List<LeaderboardEntry>> userScores { get; } = new Dictionary<DifficultyLevel, List<LeaderboardEntry>>();

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

    //Think about adding score localy and then try to add it on restart,
    //incase of a service being down or no internet
    public async void AddScore(float timeSurvived, DifficultyLevel difficultyLevel)
    {
        if(!IsLoggedIn())
        {
            Debug.Log("Not logged in");
            return;
        }

        int score = Mathf.FloorToInt(timeSurvived);

        if (UserProfile.Instance.UserHighScore[difficultyLevel] > score)
        {
            Debug.Log("Score too low to record");
            return;
        }

        try
        {
            leaderboardId = leaderboardIds[difficultyLevel];

            //takes int
            var userEntry = await LeaderboardsService.Instance
                .AddPlayerScoreAsync(leaderboardId, score);

            //Look into only calling this if higher than previous highscore

            await UserProfile.Instance.GetUserScore();

        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to submit score: " + e.Message);
        }
    }

    public async Task<Dictionary<DifficultyLevel, List<LeaderboardEntry>>> GetScores(DifficultyLevel difficultyLevel)
    {
        Dictionary<DifficultyLevel, List<LeaderboardEntry>> scores = new();

        LeaderboardScoresPage scoresResponse =
            await LeaderboardsService.Instance.GetScoresAsync(leaderboardIds[difficultyLevel]);

        scores[difficultyLevel] = scoresResponse.Results;


        return scores;
    }

    public async Task<Dictionary<DifficultyLevel, double>> GetUserScores()
    {
        Dictionary<DifficultyLevel, double> userHighScores = new()
        {
            { DifficultyLevel.Easy, 0 },
            { DifficultyLevel.Medium, 0 },
            { DifficultyLevel.Hard, 0 }
        };

        if (!IsLoggedIn())
        {
            Debug.LogError("Not logged in");
            return userHighScores;
        }

        try
        {
            foreach (var leaderboard in leaderboardIds)
            {
                var scoreResponse = await LeaderboardsService.Instance
                    .GetPlayerScoreAsync(leaderboard.Value);
                
                if (scoreResponse != null)
                {
                    userHighScores[leaderboard.Key] = scoreResponse.Score;
                }
            }

            return userHighScores;
        }
        catch (System.Exception e)
        {
            Debug.Log("Failed to get score: " + e.Message);
            return userHighScores;
        }
    }
}
