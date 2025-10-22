using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class MenuManager : MonoBehaviour
{
    public static MenuManager main;

    [Header("References")]
    [SerializeField] public UIDocument uiDocument;
    [SerializeField] public VisualTreeAsset mainMenu;
    [SerializeField] public VisualTreeAsset splashScreen;

    private Coroutine activeCoroutine;

    private void Awake()
    {
        if(main != null && main != this)
        {
            Destroy(main);
        } else
        {
            main = this;
        }
    }

    private void Start()
    {
        ShowSplash(mainMenu, 3f);
    }

    private void ShowSplash(VisualTreeAsset nextScreen, float delay)
    {
        if(activeCoroutine != null)
        {
            StopCoroutine(activeCoroutine);
        }

        uiDocument.visualTreeAsset = splashScreen;

        activeCoroutine = StartCoroutine(SplashRoutine(nextScreen, delay));
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
                uiScreen.Initialize(uiDocument);
        }
    }


}
