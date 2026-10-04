using UnityEngine.UIElements;

public class SplashScreenController
{
    private readonly VisualTreeAsset splashVTA;

    private VisualElement splashPanel;
    private VisualElement splashContainer;

    public SplashScreenController(VisualTreeAsset splashVTA)
    {
        this.splashVTA = splashVTA;
    }
    public void Initialize(VisualElement root)
    {
        // Create a container for splash if not already
        splashContainer = root.Q("SplashContainer");
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
    }

    public void Show()
    {
        splashPanel.style.display = DisplayStyle.Flex;
        splashContainer.style.display = DisplayStyle.Flex;
    }

    public void Hide()
    {
        splashPanel.style.display = DisplayStyle.None;
        splashContainer.style.display = DisplayStyle.None;
    }
}
