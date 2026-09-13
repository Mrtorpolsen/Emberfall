using System.Collections.Generic;
using UnityEngine.UIElements;

public class LeaderboardEvents : IUIScreenEvents
{
    private LeaderboardUIController controller;

    private readonly Dictionary<string, string> bindings = new()
    {
        { LeaderboardView.BTN_EASY, nameof(Btn_EasyClicked) },
        { LeaderboardView.BTN_MEDIUM, nameof(Btn_MediumClicked) },
        { LeaderboardView.BTN_HARD, nameof(Btn_HardClicked) },
        { LeaderboardView.BTN_NIGHTMARE, nameof(Btn_NightmareClicked) }
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

    private void Btn_NightmareClicked()
    {
        controller.OpenLeaderboard(DifficultyLevel.Nightmare);
    }

    public void Cleanup()
    {
        UtilityUIBinding.CleanupEvents(this);
    }
}