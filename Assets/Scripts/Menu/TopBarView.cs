using System;
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
        var root = uiDocument.rootVisualElement;
        label_username = root.Q<Label>("Label_UserName");
        label_currency = root.Q<Label>("Label_UserCurrency");
    }

    private void OnEnable()
    {
        CurrencyManager.Instance.OnCindersChanged += SetCurrency;
    }

    private void OnDisable()
    {
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
        label_currency.text = cinders.ToString();
    }
}
