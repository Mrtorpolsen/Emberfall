using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

public class Bootstrapper : MonoBehaviour
{
    [FormerlySerializedAs("splashlVTA")]
    [SerializeField] private VisualTreeAsset splashVTA;
    [SerializeField] private UIDocument uIDocument;

    private async void Start()
    {
        var root = uIDocument.rootVisualElement;
        Toast.Attach(root);

        SplashScreenController splashScreenController = new SplashScreenController(splashVTA);

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

        // Before the save loads: loading it builds the talent and research trees from these
        await ServerDefinitions.FetchAsync();

        SaveService.Instance.InitializeForPlayer(IdentityService.Instance.Current.GetPlayerId());
        await SaveService.Instance.Load();

        // Unlocks are known once Load returns (its handlers have run); repairs loadouts still holding units
        // whose talents were refunded before refunds re-locked them
        if (LoadoutService.Instance != null && LoadoutService.Instance.RemoveLockedDefinitions())
        {
            await SaveService.Instance.SaveAsync();
        }

        // Load all needed data
        var loadTasks = new List<Task>
        {
            UserProfile.Instance.GetUserScore(),
            UtilityLoadAddressable.PreloadPlaceholder(),
            UtilityLoadAddressable.PreloadIcons(),
        };

        await Task.WhenAll(loadTasks);

        // The tutorial finished offline: its gift is still due
        if (SaveService.Instance.Current.Client.HasCompletedTutorial)
        {
            await TutorialGift.ClaimIfDueAsync();
        }

        // Scores from offline matches or failed submits; after GetUserScore so the "too low" check sees the server's scores
        LeaderboardService.Instance.SubmitPendingScoresInBackground();

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
        var topBar = FindAnyObjectByType<TopBarView>();
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