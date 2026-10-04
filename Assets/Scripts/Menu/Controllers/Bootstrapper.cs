using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Bootstrapper : MonoBehaviour
{
    [SerializeField] private VisualTreeAsset splashlVTA;
    [SerializeField] private UIDocument uIDocument;

    private async void Start()
    {
        var root = uIDocument.rootVisualElement;

        SplashScreenController splashScreenController = new SplashScreenController(splashlVTA);

        splashScreenController.Initialize(root);

        splashScreenController.Show();

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
            UtilityLoadAddressable.PreloadPlaceholder(),
            UtilityLoadAddressable.PreloadIcons(),
        };

        await Task.WhenAll(loadTasks);

        UnitStatsManager.Instance.Initialize();

        InitializeTopBar();
        //Remove when done testing
        //await Task.Delay(3000);

        await TutorialFlow.Instance.Initialize(root);

        splashScreenController.Hide();

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