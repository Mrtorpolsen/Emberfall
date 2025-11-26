using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class LoginManager : MonoBehaviour
{
    public static LoginManager main;

    [Header("References")]
    [SerializeField] public UIDocument uiDocument;

    private TextField input_name;
    private Button btn_login;

    async void Awake()
    {
        if (main != null && main != this)
        {
            Destroy(main);
        }
        else
        {
            main = this;
        }

        await UnityServices.InitializeAsync();
        //If username already exists, just log in
        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("Signed in! PlayerID: " + AuthenticationService.Instance.PlayerId);
        }
        catch (AuthenticationException e)
        {
            Debug.LogError(e);
        }

        if (!string.IsNullOrEmpty(AuthenticationService.Instance.PlayerName))
        {
            Debug.Log(AuthenticationService.Instance.PlayerName);
            UserProfile.main.userName = AuthenticationService.Instance.PlayerName;
            SceneManager.LoadScene("UI_Root");
        }
    }

    private void OnEnable()
    {
        btn_login = uiDocument.rootVisualElement.Q<Button>("Btn_Login");

        btn_login.RegisterCallback<ClickEvent>(OnLoginClicked);
    }

    private void OnDisable()
    {
        btn_login.UnregisterCallback<ClickEvent>(OnLoginClicked);
    }

    public async void SetUserName()
    {
        string newName = input_name.text;

        if(string.IsNullOrEmpty(newName))
        {
            //insert error message and profanity filter with new clause
            return;
        }

        try
        {
            await AuthenticationService.Instance.UpdatePlayerNameAsync(newName);
            Debug.Log("Name updated: " + newName);
        }
        catch (AuthenticationException e)
        {
            Debug.LogError(e);
        }
    }


    private void OnLoginClicked(ClickEvent evt)
    {
        SetUserName();
        Debug.Log("Login clicked");
        SceneManager.LoadScene("UI_Root");
    }
}
