using UnityEngine;
using UnityEngine.UIElements;

public class TopBarView : MonoBehaviour
{
    [Header("References")]
    [SerializeField] UIDocument uiDocument;

    private Button btn_account;
    private Label label_username;
    private Label label_cinders;

    private void Awake()
    {
        var root = uiDocument.rootVisualElement;

        btn_account = UtilityUIBinding.QRequired<Button>(root, "Btn_Account");
        label_username = UtilityUIBinding.QRequired<Label>(root, "Label_UserName");
        label_cinders = UtilityUIBinding.QRequired<Label>(root, "Label_UserCinders");
    }

    private void OnEnable()
    {
        CurrencyManager.Instance.OnCindersChanged += SetCurrency;
        btn_account.clicked += AccountPopup.Open;
    }

    private void OnDisable()
    {
        btn_account.clicked -= AccountPopup.Open;

        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnCindersChanged -= SetCurrency;
        }
    }

    public void Initialize(string userName, int startingCinders)
    {
        SetUserName(userName);
        SetCurrency(startingCinders);
    }

    private void SetUserName(string userName)
    {
        label_username.text = string.IsNullOrEmpty(userName)
            ? "Unknown User"
            : userName;
    }

    private void SetCurrency(int cinders)
    {
        label_cinders.text = cinders.ToString();
    }
}
