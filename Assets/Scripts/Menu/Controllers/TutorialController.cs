using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

public class TutorialController : MonoBehaviour
{
    public static TutorialController Instance { get; private set; }

    [SerializeField] private VisualTreeAsset tutorialOverlayVTA;
    
    private VisualElement tutorialOverlayPanel;
    private VisualElement tutorialOverlayContainer;

    private VisualElement highlightBox;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Initialize(VisualElement root)
    {
        ResetUI();

        tutorialOverlayContainer = root.Q("TutorialOverlayContainer");
        if (tutorialOverlayContainer == null)
        {
            tutorialOverlayContainer = new VisualElement { name = "TutorialOverlayContainer" };
            root.Add(tutorialOverlayContainer);
        }

        tutorialOverlayContainer.style.position = Position.Absolute;
        tutorialOverlayContainer.style.top = 0;
        tutorialOverlayContainer.style.left = 0;
        tutorialOverlayContainer.style.right = 0;
        tutorialOverlayContainer.style.bottom = 0;
        tutorialOverlayContainer.pickingMode = PickingMode.Ignore;

        tutorialOverlayPanel = tutorialOverlayVTA.CloneTree();
        tutorialOverlayContainer.Add(tutorialOverlayPanel);

        tutorialOverlayPanel.style.position = Position.Absolute;
        tutorialOverlayPanel.style.top = 0;
        tutorialOverlayPanel.style.left = 0;
        tutorialOverlayPanel.style.right = 0;
        tutorialOverlayPanel.style.bottom = 0;
        tutorialOverlayPanel.pickingMode = PickingMode.Ignore;


        highlightBox = tutorialOverlayPanel.Q("HighlightBox");

        Hide();
    }

    public void HighlightElement(VisualElement targetElement)
    {
        Vector2 localPosition = tutorialOverlayContainer.WorldToLocal(targetElement.worldBound.position);

        highlightBox.style.left = localPosition.x - 5;
        highlightBox.style.top = localPosition.y - 5;
        highlightBox.style.width = targetElement.worldBound.width + 10;
        highlightBox.style.height = targetElement.worldBound.height + 10;

        highlightBox.style.display = DisplayStyle.Flex;
        Show();
    }

    public void StopHighlightElement()
    {
        highlightBox.style.display = DisplayStyle.None;
        Hide();
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

    public void ResetUI()
    {
        if (tutorialOverlayContainer != null)
        {
            tutorialOverlayContainer.RemoveFromHierarchy();
        }

        tutorialOverlayPanel = null;
        tutorialOverlayContainer = null;
    }
}
