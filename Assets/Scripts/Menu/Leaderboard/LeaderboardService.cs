using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Exceptions;
using Unity.Services.Leaderboards.Models;
using UnityEngine;

//Needs to be initialized earlier and a singleton since its used in game scene.
public class LeaderboardService : MonoBehaviour
{
    public static LeaderboardService Instance { get; private set; }

    private readonly Dictionary<DifficultyLevel, string> leaderboardIds = new()
    {
        { DifficultyLevel.Easy, "High_Scores_Easy" },
        { DifficultyLevel.Medium, "High_Scores_Medium" },
        { DifficultyLevel.Hard, "High_Scores_Hard" },
        { DifficultyLevel.Nightmare, "High_Scores_Nightmare" }
    };
    public Dictionary<DifficultyLevel, List<LeaderboardEntry>> userScores { get; } = new Dictionary<DifficultyLevel, List<LeaderboardEntry>>();

    public bool IsLoggedIn() => AuthenticationService.Instance.IsSignedIn;

    // Scores that haven't reached the server yet; see SubmitPendingScoresAsync
    private PendingScoreQueue pendingScores;
    private Task submitTask;
    private bool submitAgain;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        pendingScores = new PendingScoreQueue(new PlayerPrefsScoreStore());
    }

    // The score is queued first and only removed from the queue once the server has it, so a match played offline
    // (or a failed submit) is sent after the next sign-in or reconnect instead of being lost.
    public async void AddScore(float timeSurvived, DifficultyLevel difficultyLevel)
    {
        if (!PendingScoreQueue.IsRanked(difficultyLevel))
            return;

        int score = Mathf.FloorToInt(timeSurvived);

        // Offline the known high scores are 0, so every score passes and the queue keeps the best one
        if (UserProfile.Instance.UserHighScore[difficultyLevel] > score)
        {
            Debug.Log("Score too low to record");
            return;
        }

        string playerId = IdentityService.Instance.Current.GetPlayerId();
        if (string.IsNullOrEmpty(playerId))
        {
            Debug.LogWarning("No player ID; score not recorded");
            return;
        }

        // Keeps the higher of this and any score already queued for the difficulty
        pendingScores.Add(playerId, difficultyLevel, score);

        if (!IsLoggedIn())
        {
            Debug.Log("Not logged in; score queued until the next sign-in");
            return;
        }

        await SubmitPendingScoresAsync();
    }

    // Fire-and-forget version for callers that shouldn't wait on the network (e.g. the splash screen)
    public async void SubmitPendingScoresInBackground(bool refreshScoresFirst = false)
    {
        try
        {
            await SubmitPendingScoresAsync(refreshScoresFirst);
        }
        catch (System.Exception e)
        {
            Debug.LogException(e);
        }
    }

    // Sends the queued scores, then refreshes UserProfile's high scores if any were sent. Does nothing while signed
    // out. Calls made while a submit is running join it (and trigger one more pass for scores queued meanwhile).
    // refreshScoresFirst reloads the server's high scores before submitting, for when they may be stale (after a
    // reconnect they are still the offline zeros).
    public Task SubmitPendingScoresAsync(bool refreshScoresFirst = false)
    {
        if (submitTask != null && !submitTask.IsCompleted)
        {
            submitAgain = true;
            return submitTask;
        }

        submitTask = RunSubmitLoopAsync(refreshScoresFirst);
        return submitTask;
    }

    // Removes this player's queued scores, e.g. when the account is deleted
    public void ClearPendingScores()
    {
        pendingScores.Clear(IdentityService.Instance.Current.GetPlayerId());
    }

    private async Task RunSubmitLoopAsync(bool refreshScoresFirst)
    {
        if (refreshScoresFirst && IsLoggedIn())
        {
            await UserProfile.Instance.GetUserScore();
        }

        do
        {
            submitAgain = false;
            await SubmitPendingOnceAsync();
        }
        while (submitAgain);
    }

    private async Task SubmitPendingOnceAsync()
    {
        if (!IsLoggedIn())
        {
            return;
        }

        string playerId = IdentityService.Instance.Current.GetPlayerId();
        bool anySubmitted = false;

        foreach (var pending in pendingScores.GetPending(playerId))
        {
            DifficultyLevel difficultyLevel = pending.Key;
            int score = pending.Value;

            // Same check as AddScore: the server already has a better score, so this one would change nothing
            if (UserProfile.Instance.UserHighScore[difficultyLevel] > score)
            {
                pendingScores.MarkSubmitted(playerId, difficultyLevel, score);
                continue;
            }

            try
            {
                //takes int
                await LeaderboardsService.Instance.AddPlayerScoreAsync(leaderboardIds[difficultyLevel], score);
                pendingScores.MarkSubmitted(playerId, difficultyLevel, score);
                anySubmitted = true;
            }
            catch (System.Exception e)
            {
                // Likely a connection problem; keep this and the remaining scores for the next try
                Debug.LogWarning($"Failed to submit {difficultyLevel} score, kept for later: {e.Message}");
                break;
            }
        }

        if (anySubmitted)
        {
            await UserProfile.Instance.GetUserScore();
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
            { DifficultyLevel.Hard, 0 },
            { DifficultyLevel.Nightmare, 0 }
        };

        if (!IsLoggedIn())
        {
            // Expected when playing offline
            Debug.Log("Not logged in; high scores unavailable");
            return userHighScores;
        }

        foreach (var leaderboard in leaderboardIds)
        {
            try
            {
                var scoreResponse = await LeaderboardsService.Instance
                    .GetPlayerScoreAsync(leaderboard.Value);

                if (scoreResponse != null)
                {
                    userHighScores[leaderboard.Key] = scoreResponse.Score;
                }
            }
            catch (LeaderboardsException e) when (e.Reason == LeaderboardsExceptionReason.EntryNotFound)
            {
                // No score on this leaderboard yet; keep 0 and check the others
            }
            catch (System.Exception e)
            {
                // Likely a connection problem; stop instead of waiting on every remaining request
                Debug.Log("Failed to get score: " + e.Message);
                break;
            }
        }

        return userHighScores;
    }
}
