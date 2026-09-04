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
            Dictionary<DifficultyLevel, List<LeaderboardEntry>> scores = await LeaderboardService.Instance.GetScores(difficultyLevel);

            view.ClearLeaderboard();
            view.RenderLeaderboard(scores[difficultyLevel]);
            view.SetActiveLeaderboardBtn(difficultyLevel);
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
