using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

public class MainMenuView : IUIScreenView
{
    private Label labelEasy;
    private Label labelMedium;
    private Label labelHard;

    private const string EASY_LABEL = "Label_EasyHighScore";
    private const string MEDIUM_LABEL = "Label_MediumHighScore";
    private const string HARD_LABEL = "Label_HardHighScore";

    public Task InitializeAsync(VisualElement root)
    {
        labelEasy = UtilityUIBinding.QRequired<Label>(root, EASY_LABEL);
        labelMedium = UtilityUIBinding.QRequired<Label>(root, MEDIUM_LABEL);
        labelHard = UtilityUIBinding.QRequired<Label>(root, HARD_LABEL);

        var userHighScores = UserProfile.Instance.UserHighScore;

        labelEasy.text = TimeFormatter.FormatTimeMiliseconds((float)userHighScores[DifficultyLevel.Easy]);
        labelMedium.text = TimeFormatter.FormatTimeMiliseconds((float)userHighScores[DifficultyLevel.Medium]);
        labelHard.text = TimeFormatter.FormatTimeMiliseconds((float)userHighScores[DifficultyLevel.Hard]);

        return Task.CompletedTask;
    }
}
