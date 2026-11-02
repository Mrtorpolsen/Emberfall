using System.Collections.Generic;
using Unity.Services.Leaderboards.Models;
using UnityEngine;

public class LeaderboardUIController : IUIScreenController
{
    private LeaderboardView view;

    private DifficultyLevel defaultDifficultyLevel = DifficultyLevel.Medium;

    public void Initialize(IUIScreenView screenView)
    {
        if (screenView is not LeaderboardView leaderboardView)
        {
            Debug.LogError("LeaderboardUIController received wrong view type.");
            return;
        }

        this.view = leaderboardView;
        OpenLeaderboard(defaultDifficultyLevel);
    }

    public async void OpenLeaderboard(DifficultyLevel difficultyLevel)
    {
        LoadingSpinner.Instance.ShowSpinner();

        try
        {
            // A session that started offline may be able to sign in now
            if (!LeaderboardService.Instance.IsLoggedIn() && await IdentityService.Instance.TryReconnectAsync(playerAction: true))
            {
                // Wait for the queued scores the reconnect is submitting, so the player's own score shows up
                await LeaderboardService.Instance.SubmitPendingScoresAsync();
            }

            Dictionary<DifficultyLevel, List<LeaderboardEntry>> scores = await LeaderboardService.Instance.GetScores(difficultyLevel);

            view.ClearLeaderboard();
            view.RenderLeaderboard(scores[difficultyLevel]);
            view.SetActiveLeaderboardBtn(difficultyLevel);
        }
        catch (System.Exception e)
        {
            // Offline (not signed in) or the service is unreachable
            Debug.LogWarning("Failed to load leaderboard: " + e.Message);
            Toast.Show("Couldn't load the leaderboard. Check your connection.");
        }
        finally
        {
            LoadingSpinner.Instance.HideSpinner();
        }
    }

    public void Cleanup()
    {
        view.ClearLeaderboard();
    }

}
