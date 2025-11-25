using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class MenuManager : MonoBehaviour
{
    public static MenuManager main {  get; private set; }

    [Header("References")]
    [SerializeField] public UIDocument uiDocument;
    [SerializeField] public VisualTreeAsset mainMenu;
    [SerializeField] public VisualTreeAsset splashScreen;

    private Coroutine activeCoroutine;

    private void Awake()
    {
        if (main != null && main != this) { Destroy(gameObject); return; }
        main = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        ShowSplash(mainMenu, 3f);
    }
    //Use this to load in ressources, maybe move to own file
    //Also remove the fake delay, so and make it await the actual loads of data
    //Maybe rewrite so you call the next screen, and the throw in the funcs to await before showing
    private async void ShowSplash(VisualTreeAsset nextScreen, float delay)
    {
        if(activeCoroutine != null)
        {
            StopCoroutine(activeCoroutine);
        }

        uiDocument.visualTreeAsset = splashScreen;

        activeCoroutine = StartCoroutine(SplashRoutine(nextScreen, delay));

        await LeaderboardManager.main.GetUserScore();
        await LeaderboardManager.main.GetScores();
    }

    private IEnumerator SplashRoutine(VisualTreeAsset nextScreen, float delay)
    {
        yield return new WaitForSeconds(delay);
        uiDocument.visualTreeAsset = nextScreen;
        InitializeActiveScreen();
        activeCoroutine = null;
    }

    private void InitializeActiveScreen()
    {
        // Find all IUIScreen scripts in the scene and initialize them
        foreach (var screen in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
        {
            if (screen is IUIScreen uiScreen)
            {
                uiScreen.Initialize(uiDocument);
            }
        }
    }
}
