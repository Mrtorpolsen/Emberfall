using System.Collections.Generic;
using UnityEngine.UIElements;

public class LeaderboardEvents : IUIScreenEvents
{
    private LeaderboardUIController controller;

    private readonly Dictionary<string, string> bindings = new()
    {
        { LeaderboardView.BTN_EASY, nameof(Btn_EasyClicked) },
        { LeaderboardView.BTN_MEDIUM, nameof(Btn_MediumClicked) },
        { LeaderboardView.BTN_HARD, nameof(Btn_HardClicked) }
    };

    public void BindEvents(VisualElement root,
        IUIScreenController controller = null,
        IUIScreenView view = null)
    {
        this.controller = controller as LeaderboardUIController;

        UtilityUIBinding.BindEvents(root, this, bindings);
    }

    private void Btn_EasyClicked()
    {
        controller.OpenLeaderboard(DifficultyLevel.Easy);
    }

    private void Btn_MediumClicked()
    {
        controller.OpenLeaderboard(DifficultyLevel.Medium);
    }

    private void Btn_HardClicked()
    {
        controller.OpenLeaderboard(DifficultyLevel.Hard);
    }

    public void Cleanup()
    {
        UtilityUIBinding.CleanupEvents(this);
    }
}