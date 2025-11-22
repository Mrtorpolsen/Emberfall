using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

public class MainMenuView : MonoBehaviour
{
    [Header("References")]
    [SerializeField] public UIDocument uiDocument;

    private Label label_username;
    private Label label_currency;
    private Label label_highscore;

    public void Initialize()
    {
        var root = uiDocument.rootVisualElement;

        label_username = root.Q<Label>("Label_UserName");
        label_currency = root.Q<Label>("Label_UserCurrency");
        label_highscore = root.Q<Label>("Label_HighScore");

        if(!string.IsNullOrEmpty(UserProfile.main?.userName))
        {
            label_username.text = UserProfile.main.userName;
        }
        else
        {
            label_username.text = "Failed to fetch";
        }

        label_highscore.text = Utility.FormatTime(LeaderboardManager.main.UserHighScore);
    }
}
