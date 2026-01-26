using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

[DisallowMultipleComponent]
public class LoginManager : MonoBehaviour
{
    public static LoginManager main;

    [Header("References")]
    [SerializeField] private UIDocument uiDocument;

    private TextField input_name;
    private Button btn_login;

    private void Awake()
    {
        if (main != null && main != this)
        {
            Destroy(gameObject);
            return;
        }

        main = this;
    }

    private async void Start()
    {
        await UnityServices.InitializeAsync();

        //Clears saved token FOR TESTING
        //AuthenticationService.Instance.ClearSessionToken();

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            try
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.Log("Signed in! PlayerID: " + AuthenticationService.Instance.PlayerId);
            }
            catch (AuthenticationException e)
            {
                Debug.LogError("Authentication failed: " + e);
                return;
            }
        }

        if (!string.IsNullOrEmpty(AuthenticationService.Instance.PlayerName))
        {
            UserProfile.Instance.userName = AuthenticationService.Instance.PlayerName;
            Login();
            return;
        }

        // ensure rootVisualElement is ready
        uiDocument.rootVisualElement.schedule.Execute(_ => InitializeUI());
    }

    private void InitializeUI()
    {
        if (uiDocument == null)
        {
            Debug.LogError("UIDocument reference is missing!");
            return;
        }

        var root = uiDocument.rootVisualElement;
        if (root == null)
        {
            Debug.LogError("rootVisualElement is null!");
            return;
        }

        input_name = root.Q<TextField>("Input_Username");
        btn_login = root.Q<Button>("Btn_Login");

        if (input_name == null || btn_login == null)
        {
            Debug.LogError("UI Elements not found! Check names in UXML.");
            return;
        }

        btn_login.clicked += OnLoginClicked;
    }

    private async void OnLoginClicked()
    {
        await SetUserName();
    }

    private async Task SetUserName()
    {
        if (input_name == null)
        {
            Debug.LogError("Input field is null!");
            return;
        }

        string newName = input_name.text.Trim();
        if (string.IsNullOrEmpty(newName))
        {
            Debug.LogWarning("Username cannot be empty!");
            return;
        }

        try
        {
            await AuthenticationService.Instance.UpdatePlayerNameAsync(newName);
            UserProfile.Instance.userName = AuthenticationService.Instance.PlayerName;
            Debug.Log("Name updated: " + newName);

            Login();
        }
        catch (AuthenticationException e)
        {
            Debug.LogError("Failed to update username: " + e);
        }
    }

    private void OnDestroy()
    {
        if (btn_login != null)
        {
            btn_login.clicked -= OnLoginClicked;
        }
    }

    private void Login()
    {
        SceneManager.LoadScene("UI_Root");
    }
}
