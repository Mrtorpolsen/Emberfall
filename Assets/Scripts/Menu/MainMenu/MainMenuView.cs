using UnityEngine;
using UnityEngine.UIElements;

public class MainMenuView : IUIScreen
{
    private VisualElement root;
    private Label label_highscore;

    public void Initialize(VisualElement root)
    {
        this.root = root;

        label_highscore = root.Q<Label>("Label_HighScore");

        label_highscore.text = Utility.FormatTime(UserProfile.main.UserHighScore);
    }
}
