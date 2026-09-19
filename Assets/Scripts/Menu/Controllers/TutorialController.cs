using UnityEngine;
using UnityEngine.UIElements;

public class TutorialController : MonoBehaviour
{
    public static TutorialController Instance { get; private set; }

    [SerializeField] private VisualTreeAsset tutorialOverlayVTA;

    private VisualElement tutorialOverlayPanel;
    private VisualElement tutorialOverlayContainer;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void Initialze(VisualElement root)
    {
        //Create a container for splash if not already
        tutorialOverlayContainer = root.Q("TutorialOverlay");
        if (tutorialOverlayContainer == null)
        {
            tutorialOverlayContainer = new VisualElement { name = "TutorialOverlay" };
            root.Add(tutorialOverlayContainer);
        }

        tutorialOverlayContainer.style.position = Position.Absolute;
        tutorialOverlayContainer.style.top = 0;
        tutorialOverlayContainer.style.left = 0;
        tutorialOverlayContainer.style.right = 0;
        tutorialOverlayContainer.style.bottom = 0;

        tutorialOverlayPanel = tutorialOverlayVTA.CloneTree();
        tutorialOverlayContainer.Add(tutorialOverlayPanel);

        tutorialOverlayPanel.style.position = Position.Absolute;
        tutorialOverlayPanel.style.top = 0;
        tutorialOverlayPanel.style.left = 0;
        tutorialOverlayPanel.style.right = 0;
        tutorialOverlayPanel.style.bottom = 0;
    }

    public void Show()
    {
        tutorialOverlayPanel.style.display = DisplayStyle.Flex;
        tutorialOverlayContainer.style.display = DisplayStyle.Flex;
    }

    public void Hide()
    {
        tutorialOverlayPanel.style.display = DisplayStyle.None;
        tutorialOverlayContainer.style.display = DisplayStyle.None;
    }
}
