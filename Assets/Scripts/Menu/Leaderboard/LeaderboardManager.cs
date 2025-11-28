using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Models;
using UnityEngine;

public class LeaderboardManager : MonoBehaviour
{
    public static LeaderboardManager main;

    private string leaderboardId = "High_Scores";
    public float UserHighScore { get; private set; } = 0;
    public List<LeaderboardEntry> userScores;

    public bool IsLoggedIn() => AuthenticationService.Instance.IsSignedIn;

    private void Awake()
    {
        if (main != null) { Destroy(gameObject); return; }
        main = this;
        DontDestroyOnLoad(gameObject);
    }

    //Think about adding score localy and then try to add it on restart,
    //incase of a service being down or no internet
    public async void AddScore(float timeSurvived)
    {
        if(!IsLoggedIn())
        {
            Debug.Log("Not logged in");
            return;
        }

        int score = Mathf.FloorToInt(timeSurvived);

        try
        {
            //takes int
            var userEntry = await LeaderboardsService.Instance
                .AddPlayerScoreAsync(leaderboardId, score);

            //Look into only calling this if higher than previous highscore
            await GetUserScore();

        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to submit score: " + e.Message);
        }
    }

    public async Task<double> GetUserScore()
    {
        if (!IsLoggedIn())
        {
            Debug.LogError("Not logged in");
            return 0;
        }

        try
        {
            var scoreResponse = await LeaderboardsService.Instance
                .GetPlayerScoreAsync(leaderboardId);

            if (scoreResponse != null)
            {
                UserHighScore = (float)scoreResponse.Score;
            }
            else
            {
                UserHighScore = 0;
            }
            return UserHighScore;
        }
        catch (System.Exception e)
        {
            Debug.Log("Failed to get score: " + e.Message);
            return UserHighScore;
        }
    }

    public async Task GetScores()
    {
        var scoresResponse = await LeaderboardsService.Instance
            .GetScoresAsync(leaderboardId);

        userScores = scoresResponse.Results;
    }
}
