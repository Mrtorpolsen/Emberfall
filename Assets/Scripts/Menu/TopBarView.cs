using UnityEngine;
using UnityEngine.UIElements;

public class TopBarView : MonoBehaviour
{
    [Header("References")]
    [SerializeField] UIDocument uiDocument;

    private Label label_username;
    private Label label_currency;

    private void Awake()
    {
        label_username = uiDocument.rootVisualElement.Q<Label>("Label_UserName");
        label_currency = uiDocument.rootVisualElement.Q<Label>("Label_UserCurrency");

        if (!string.IsNullOrEmpty(UserProfile.Instance?.userName))
        {
            label_username.text = UserProfile.Instance.userName;
        }
        else
        {
            label_username.text = "Failed to fetch username";
        }

    }
}
