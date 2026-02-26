using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

public class MainMenuView : IUIScreenView
{
    private Label label_highscore;

    public Task InitializeAsync(VisualElement root)
    {
        label_highscore = root.Q<Label>("Label_HighScore");

        label_highscore.text = TimeFormatter.FormatTime(UserProfile.Instance.UserHighScore);

        return Task.CompletedTask;
    }
}
