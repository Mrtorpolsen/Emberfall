using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MenuManager : MonoBehaviour
{
    public static MenuManager main { get; private set; }

    [Header("UI Document")]
    [SerializeField] private UIDocument uiDocument;

    [Header("Content Screens")]
    [SerializeField] private VisualTreeAsset mainMenuVTA;
    [SerializeField] private VisualTreeAsset leaderboardVTA;

    private VisualElement contentContainer;
    private VisualElement currentScreen;
    private IUIScreen currentView;
    private IUIScreenEvents currentEvents;
    private bool hasInitialized = false;

    private Dictionary<string, ScreenDefinition> screens =
        new Dictionary<string, ScreenDefinition>();

    private void Awake()
    {
        if (main != null && main != this)
        {
            Destroy(gameObject);
            return;
        }

        main = this;
        DontDestroyOnLoad(gameObject);

        // Listen for any scene load
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        var root = uiDocument.rootVisualElement;
        contentContainer = root.Q<VisualElement>("ContentContainer");

        if (contentContainer == null)
        {
            Debug.LogError("ContentContainer not found! Check UI_Root.uxml");
            return;
        }

        RegisterScreen<MainMenuView, MainMenuEvents>("MainMenu", mainMenuVTA);
        RegisterScreen<LeaderboardView, LeaderboardEvents>("Leaderboard", leaderboardVTA);
    }

    public void RegisterScreen<TView, TEvents>(string name, VisualTreeAsset vta)
        where TView : IUIScreen, new()
        where TEvents : IUIScreenEvents, new()
    {
        screens[name] = new ScreenDefinition(
            vta,
            () => new TView(),
            () => new TEvents()
        );
    }

    public void LoadScreen(string screenName)
    {
        if (!screens.TryGetValue(screenName, out var def))
        {
            Debug.LogError($"No screen registered with name '{name}'");
            return;
        }

        SwapContent(def);
    }

    private void SwapContent(ScreenDefinition def)
    {
        // Remove previous screen
        currentScreen?.RemoveFromHierarchy();

        // Clean up old events
        currentEvents?.Cleanup();

        // Clone the template container
        currentScreen = def.vta.CloneTree();
        currentScreen.style.flexGrow = 1;
        contentContainer.Add(currentScreen);

        // IMPORTANT: get the first child — the real screen root
        VisualElement screenRoot = currentScreen[0];

        // Initialize view
        currentView = def.createView();
        currentEvents = def.createEvents();

        // Bind events
        currentView.Initialize(screenRoot);
        currentEvents.BindEvents(screenRoot);
    }
    //Checks if its UI_Root that getting loaded, then reassigns references and loads mainmenu
    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        if(scene.name != "UI_Root")
        {
            return;
        }
        if(!hasInitialized)
        {
            hasInitialized = true;
            return;
        }

        Debug.Log("UI_Root loaded, reinitializing MenuManager UI references");

        UIDocument newUIDocument = FindFirstObjectByType<UIDocument>();

        if (newUIDocument == null)
        {
            Debug.LogError("newUIDocument not found in UI_Root");
        }

        if (uiDocument == null)
        {
            uiDocument = newUIDocument;
        }
        // Re-query root and content container
        var root = uiDocument.rootVisualElement;
        contentContainer = root.Q<VisualElement>("ContentContainer");

        if(contentContainer == null)
        {
            Debug.LogError("ContentContainer not found in UI_Root!");
            return;
        }

        // Load main menu by default
        LoadScreen("MainMenu");
    }
}
