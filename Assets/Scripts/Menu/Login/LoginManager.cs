using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

[DisallowMultipleComponent]
public class LoginManager : MonoBehaviour
{
    public static LoginManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private UIDocument uiDocument;

    private VisualElement panel_signIn;
    private VisualElement panel_name;
    private Button btn_signInGoogle;
    private Button btn_continueGuest;
    private TextField input_name;
    private Button btn_login;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private async void Start()
    {
        await UnityServices.InitializeAsync();

        //Clears saved token FOR TESTING
        //AuthenticationService.Instance.ClearSessionToken();

        var auth = AuthenticationService.Instance;

        // A cached session (guest or Google) is restored without asking; only first launches get the sign-in choice
        if (!auth.IsSignedIn && auth.SessionTokenExists)
        {
            try
            {
                await auth.SignInAnonymouslyAsync();
                Debug.Log("Signed in! PlayerID: " + auth.PlayerId);
            }
            catch (AuthenticationException e)
            {
                Debug.LogError("Authentication failed: " + e);
                return;
            }
        }

        if (auth.IsSignedIn && !string.IsNullOrEmpty(auth.PlayerName))
        {
            UserProfile.Instance.userName = auth.PlayerName;
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

        panel_signIn = UtilityUIBinding.QRequired<VisualElement>(root, "Panel_SignIn");
        panel_name = UtilityUIBinding.QRequired<VisualElement>(root, "Panel_Name");
        btn_signInGoogle = UtilityUIBinding.QRequired<Button>(root, "Btn_SignInGoogle");
        btn_continueGuest = UtilityUIBinding.QRequired<Button>(root, "Btn_ContinueGuest");
        input_name = UtilityUIBinding.QRequired<TextField>(root, "Input_Username");
        btn_login = UtilityUIBinding.QRequired<Button>(root, "Btn_Login");

        btn_signInGoogle.style.display = PlayGamesManager.IsSupported ? DisplayStyle.Flex : DisplayStyle.None;

        btn_signInGoogle.clicked += OnSignInGoogleClicked;
        btn_continueGuest.clicked += OnContinueGuestClicked;
        btn_login.clicked += OnLoginClicked;

        ShowPanel(AuthenticationService.Instance.IsSignedIn ? panel_name : panel_signIn);
    }

    private void ShowPanel(VisualElement panel)
    {
        panel_signIn.style.display = panel == panel_signIn ? DisplayStyle.Flex : DisplayStyle.None;
        panel_name.style.display = panel == panel_name ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private async void OnSignInGoogleClicked()
    {
        await SignInAsync(async auth =>
        {
            string authCode = await PlayGamesManager.GetServerAuthCodeAsync();
            if (authCode == null)
            {
                return false;
            }

            await auth.SignInWithGooglePlayGamesAsync(authCode);
            return true;
        });
    }

    private async void OnContinueGuestClicked()
    {
        await SignInAsync(async auth =>
        {
            await auth.SignInAnonymouslyAsync();
            return true;
        });
    }

    // signIn returns false if the player cancelled. On cancel or failure the choice stays on screen
    // so the player can retry or pick another option.
    private async Task SignInAsync(System.Func<IAuthenticationService, Task<bool>> signIn)
    {
        var auth = AuthenticationService.Instance;
        panel_signIn.SetEnabled(false);

        bool signedIn;
        try
        {
            signedIn = await signIn(auth);
        }
        catch (RequestFailedException e)
        {
            Debug.LogWarning("Sign-in failed: " + e);
            signedIn = false;
        }

        if (!signedIn)
        {
            panel_signIn.SetEnabled(true);
            return;
        }

        Debug.Log("Signed in! PlayerID: " + auth.PlayerId);

        if (!string.IsNullOrEmpty(auth.PlayerName))
        {
            UserProfile.Instance.userName = auth.PlayerName;
            Login();
            return;
        }

        ShowPanel(panel_name);
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
        if (btn_signInGoogle != null)
        {
            btn_signInGoogle.clicked -= OnSignInGoogleClicked;
        }

        if (btn_continueGuest != null)
        {
            btn_continueGuest.clicked -= OnContinueGuestClicked;
        }

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
