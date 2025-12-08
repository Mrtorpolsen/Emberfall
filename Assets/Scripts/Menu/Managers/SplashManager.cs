using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

public class SplashManager : MonoBehaviour
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

        // Load all needed data in parallel
        var loadTasks = new List<Task>
        {
            UserProfile.main.GetUserScore(),
            LeaderboardManager.main.GetScores(),
        };

        await Task.WhenAll(loadTasks);

        //Remove when done testing
        //await Task.Delay(3000);

        splashContainer.Remove(splashPanel);

        splashPanel.style.display = DisplayStyle.None;
        splashContainer.style.display = DisplayStyle.None;

        MenuManager.main.LoadScreen("MainMenu");

    }
}