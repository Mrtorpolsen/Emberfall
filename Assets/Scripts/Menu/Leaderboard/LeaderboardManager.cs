using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Models;
using UnityEngine;

//Needs to be initialized earlier and a singleton since its used in game scene.
public class LeaderboardManager : MonoBehaviour
{
    public static LeaderboardManager main;

    private string leaderboardId = "High_Scores";
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

        if (UserProfile.main.UserHighScore > score)
        {
            Debug.Log("Score too low to record");
            return;
        }

        try
        {
            //takes int
            var userEntry = await LeaderboardsService.Instance
                .AddPlayerScoreAsync(leaderboardId, score);

            //Look into only calling this if higher than previous highscore

            await UserProfile.main.GetUserScore();

        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to submit score: " + e.Message);
        }
    }

    public async Task GetScores()
    {
        var scoresResponse = await LeaderboardsService.Instance
            .GetScoresAsync(leaderboardId);

        userScores = scoresResponse.Results;
    }
}
