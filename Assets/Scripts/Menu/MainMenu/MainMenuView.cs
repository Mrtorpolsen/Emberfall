using UnityEngine;
using UnityEngine.UIElements;

public class MainMenuView : IUIScreenView
{
    private Label label_highscore;

    public void Initialize(VisualElement root)
    {

        label_highscore = root.Q<Label>("Label_HighScore");

        label_highscore.text = Utility.FormatTime(UserProfile.Instance.UserHighScore);
    }
}
