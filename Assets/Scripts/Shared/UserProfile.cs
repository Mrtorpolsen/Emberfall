using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Leaderboards;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

[DisallowMultipleComponent]
public class UserProfile : MonoBehaviour
{
    public static UserProfile Instance;

    public string userName;
    public int currency;
    public float UserHighScore { get; private set; } = 0;

    public bool IsLoggedIn() => AuthenticationService.Instance.IsSignedIn;
    private string leaderboardId = "High_Scores";

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
}
