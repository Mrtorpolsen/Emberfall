using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

public class SplashScreenController : MonoBehaviour
{
    [SerializeField] private VisualTreeAsset splashVTA;
    [SerializeField] private UIDocument uIDocument;

    private VisualElement splashPanel;

    private async void Start()
    {
        var root = uIDocument.rootVisualElement;

        // Create a container for splash if not already
        var splashContainer = root.Q("SplashContainer");
        if (splashContainer == null)
        {
            splashContainer = new VisualElement { name = "SplashContainer" };
            root.Add(splashContainer);
        }

        splashContainer.style.position = Position.Absolute;
        splashContainer.style.top = 0;
        splashContainer.style.left = 0;
        splashContainer.style.right = 0;
        splashContainer.style.bottom = 0;

        splashPanel = splashVTA.CloneTree();
        splashContainer.Add(splashPanel);

        splashPanel.style.position = Position.Absolute;
        splashPanel.style.top = 0;
        splashPanel.style.left = 0;
        splashPanel.style.right = 0;
        splashPanel.style.bottom = 0;

        if (SaveService.Instance == null || IdentityService.Instance == null)
        {
            Debug.LogError("SaveService or IdentityService missing");
            return;
        }

        if (IdentityService.Instance.Current == null)
        {
            Debug.LogError("Identity not authenticated before Splash");
            return;
        }

        SaveService.Instance.InitializeForPlayer(IdentityService.Instance.Current.GetPlayerId());
        await SaveService.Instance.Load();

        // Load all needed data
        var loadTasks = new List<Task>
        {
            UserProfile.Instance.GetUserScore(),
            UtilityLoadAdressable.PreloadPlaceholder(),
        };

        await Task.WhenAll(loadTasks);

        //move this to a reward manager or service later
        if(!SaveService.Instance.Current.HasReceivedLoginGift)
        {
            CurrencyManager.Instance.Add(CurrencyTypes.Cinders, 2000);
            SaveService.Instance.Current.HasReceivedLoginGift = true;
            await SaveService.Instance.SaveAsync();
        }

        InitializeTopBar();
        //Remove when done testing
        //await Task.Delay(3000);

        splashContainer.Remove(splashPanel);

        splashPanel.style.display = DisplayStyle.None;
        splashContainer.style.display = DisplayStyle.None;

        try
        {
            await UIScreenRouter.Instance.LoadScreen("MainMenu");
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    private void InitializeTopBar()
    {
        var topBar = FindFirstObjectByType<TopBarView>();
        if (topBar == null)
        {
            Debug.LogError("TopBarView not found");
            return;
        }

        topBar.Initialize(
            UserProfile.Instance.userName,
            CurrencyManager.Instance.Get(CurrencyTypes.Cinders)
        );
    }
}