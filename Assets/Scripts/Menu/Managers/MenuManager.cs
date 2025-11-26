using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class MenuManager : MonoBehaviour
{
    public static MenuManager main { get; private set; }

    [Header("Root UI Document")]
    [SerializeField] private UIDocument uiRootDocument;

    [Header("Content Screens")]
    [SerializeField] private VisualTreeAsset mainMenuVTA;
    [SerializeField] private VisualTreeAsset leaderboardVTA;

    private VisualElement contentContainer;
    private VisualElement currentScreen;

    private void Awake()
    {
        if (main != null && main != this)
        {
            Destroy(gameObject);
            return;
        }

        main = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        var root = uiRootDocument.rootVisualElement;
        contentContainer = root.Q<VisualElement>("ContentContainer");

        if (contentContainer == null)
        {
            Debug.LogError("ContentContainer not found! Check UI_Root.uxml");
            return;
        }
    }

    public void LoadScreen(string screenName)
    {
        VisualTreeAsset screenVTA = screenName switch
        {
            "MainMenu" => mainMenuVTA,
            "Leaderboard" => leaderboardVTA,
            _ => null
        };

        if (screenVTA == null)
        {
            Debug.LogError($"No VTA assigned for screen '{screenName}'");
            return;
        }

        SwapContent(screenVTA);
    }

    private void SwapContent(VisualTreeAsset vta)
    {
        // Remove previous screen
        currentScreen?.RemoveFromHierarchy();

        // Create new screen
        currentScreen = vta.CloneTree();
        //set templatecontainer to take space
        currentScreen.style.flexGrow = 1;

        contentContainer.Add(currentScreen);

        // Inject initialization if implemented
        foreach (var mono in GetComponentsInChildren<MonoBehaviour>(true))
        {
            Debug.Log("Mono" + mono);

            if (mono is IUIScreen screen)
            {
                Debug.Log("Runing ini");
                screen.Initialize();
            }
        }
    }
}
