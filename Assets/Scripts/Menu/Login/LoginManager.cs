using System.Linq;
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
    [SerializeField] private TextAsset profanityWords;

    private VisualElement panel_signIn;
    private VisualElement panel_name;
    private Button btn_signInGoogle;
    private Button btn_continueGuest;
    private TextField input_name;
    private Button btn_login;

    private const string LAST_PLAYER_NAME_KEY = "LastPlayerName";

    private ProfanityFilter profanityFilter;

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
        AuthenticationService.Instance.ClearSessionToken();

        var auth = AuthenticationService.Instance;

        // A cached session (guest or Google) is restored without asking; only first launches get the sign-in choice
        if (!auth.IsSignedIn && auth.SessionTokenExists)
        {
            try
            {
                await auth.SignInAnonymouslyAsync();
                Debug.Log("Signed in! PlayerID: " + auth.PlayerId);
            }
            catch (RequestFailedException e)
            {
                // Offline or the service is down. Any failure plays offline, so a server error never costs a guest their account.
                Debug.LogWarning("Restoring the session failed: " + e);

                if (TryStartOffline())
                {
                    return;
                }

                Toast.Show("Couldn't connect. Check your connection and try again.");
            }
        }

        if (auth.IsSignedIn && !string.IsNullOrEmpty(auth.PlayerName))
        {
            ApplyUserName(auth.PlayerName);
            Login();
            return;
        }

        // ensure rootVisualElement is ready
        uiDocument.rootVisualElement.schedule.Execute(_ => InitializeUI());
    }

    // Plays locally as the last signed-in player (UnityAuthIdentity falls back to the cached ID); sign-in is retried
    // on the next launch. Needs a previous online launch to have cached the ID and name.
    private bool TryStartOffline()
    {
        string playerName = PlayerPrefs.GetString(LAST_PLAYER_NAME_KEY, null);
        if (IdentityService.Instance.Current.GetPlayerId() == null || string.IsNullOrEmpty(playerName))
        {
            return false;
        }

        UserProfile.Instance.userName = playerName;
        Toast.Show("Couldn't connect. Playing offline; leaderboards are unavailable.");
        Login();
        return true;
    }

    // Cached for offline launches
    private static void ApplyUserName(string playerName)
    {
        UserProfile.Instance.userName = playerName;
        PlayerPrefs.SetString(LAST_PLAYER_NAME_KEY, playerName);
        PlayerPrefs.Save();
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

        Toast.Attach(root);

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

            // Suggested in the name prompt, which only shows if the account has no name yet
            input_name.value = ToNameSuggestion(PlayGamesManager.GetDisplayName());
            return true;
        }, "Couldn't sign in with Play Games. Try again or continue as guest.");
    }

    private async void OnContinueGuestClicked()
    {
        await SignInAsync(async auth =>
        {
            await auth.SignInAnonymouslyAsync();
            return true;
        }, "Couldn't sign in. Check your connection and try again.");
    }

    // signIn returns false if the player cancelled. On cancel or failure the choice stays on screen
    // so the player can retry or pick another option.
    private async Task SignInAsync(System.Func<IAuthenticationService, Task<bool>> signIn, string failureMessage)
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
            Toast.Show(failureMessage);
            panel_signIn.SetEnabled(true);
            return;
        }

        Debug.Log("Signed in! PlayerID: " + auth.PlayerId);

        if (!string.IsNullOrEmpty(auth.PlayerName))
        {
            ApplyUserName(auth.PlayerName);
            Login();
            return;
        }

        ShowPanel(panel_name);
    }

    // Player names can't contain whitespace and the field caps the length
    private string ToNameSuggestion(string displayName)
    {
        if (string.IsNullOrEmpty(displayName))
        {
            return "";
        }

        string name = new string(displayName.Where(c => !char.IsWhiteSpace(c)).ToArray());
        return name.Length > input_name.maxLength ? name.Substring(0, input_name.maxLength) : name;
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
            Toast.Show("Please enter a name.");
            return;
        }

        if (!IsNameAllowed(newName))
        {
            Toast.Show("That name isn't allowed. Try another one.");
            return;
        }

        try
        {
            await AuthenticationService.Instance.UpdatePlayerNameAsync(newName);
            ApplyUserName(AuthenticationService.Instance.PlayerName);
            Debug.Log("Name updated: " + newName);

            Login();
        }
        catch (AuthenticationException e)
        {
            Debug.LogError("Failed to update username: " + e);
            Toast.Show("Couldn't set that name. Try another one.");
        }
    }

    private bool IsNameAllowed(string playerName)
    {
        if (profanityWords == null)
        {
            // Don't lock everyone out of signing up over a missing reference
            Debug.LogError("Profanity word list is not assigned on LoginManager; name not checked");
            return true;
        }

        profanityFilter ??= new ProfanityFilter(profanityWords.text);
        return profanityFilter.IsAllowed(playerName);
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
