using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;
using static TutorialFlow;

public class TutorialPresenter
{
    private VisualTreeAsset tutorialOverlayVTA;

    private VisualElement root;

    //tutorialOverlayPanel is the templateContainer
    private VisualElement tutorialOverlayPanel;
    private VisualElement tutorialOverlayContainer;
    private VisualElement tutorialContainer;

    private VisualElement highlightBox;

    private VisualElement topBlocker;
    private VisualElement rightBlocker;
    private VisualElement bottomBlocker;
    private VisualElement leftBlocker;

    private VisualElement tutorialPopupContainer;
    private VisualElement tutorialPopupContent;
    private VisualElement img;

    private Label heading;
    private Label description;

    private VisualElement btnContainer;
    private Button cta;
    private Action oldActionRef = null;

    private const string HIGHLIGHT_BOX = "HighlightBox";
    private const string TOP_BLOCKER = "TopBlocker";
    private const string RIGHT_BLOCKER = "RightBlocker";
    private const string BOTTOM_BLOCKER = "BottomBlocker";
    private const string LEFT_BLOCKER = "LeftBlocker";
    private const string TUTORIAL_CONTAINER = "TutorialContainer";
    private const string TUTORIAL_POPUP_CONTAINER = "TutorialPopupContainer";
    private const string TUTORIAL_POPUP_CONTENT = "TutorialPopupContent";
    private const string IMG = "Img";
    private const string LABEL_HEADING = "Label_Heading";
    private const string LABEL_DESCRIPTION = "Label_Description";
    private const string BTN_CONTAINER = "BtnContainer";
    private const string BTN_CTA = "Btn_CTA";

    private const string TUTORIAL_OVERLAY_ADDRESSABLE = "UI/TutorialOverlay";

    public async Task Initialize(VisualElement root)
    {
        ResetOverlay();

        this.root = root;

        tutorialOverlayVTA = await Addressables.LoadAssetAsync<VisualTreeAsset>(TUTORIAL_OVERLAY_ADDRESSABLE).Task;

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


        highlightBox = UtilityUIBinding.QRequired<VisualElement>(tutorialOverlayPanel, HIGHLIGHT_BOX);
        tutorialPopupContainer = UtilityUIBinding.QRequired<VisualElement>(tutorialOverlayPanel, TUTORIAL_POPUP_CONTAINER);
        tutorialPopupContent = UtilityUIBinding.QRequired<VisualElement>(tutorialOverlayPanel, TUTORIAL_POPUP_CONTENT);
        img = UtilityUIBinding.QRequired<VisualElement>(tutorialOverlayPanel, IMG);
        heading = UtilityUIBinding.QRequired<Label>(tutorialOverlayPanel, LABEL_HEADING);
        description = UtilityUIBinding.QRequired<Label>(tutorialOverlayPanel, LABEL_DESCRIPTION);
        btnContainer = UtilityUIBinding.QRequired<VisualElement>(tutorialOverlayPanel, BTN_CONTAINER);
        cta = UtilityUIBinding.QRequired<Button>(tutorialOverlayPanel, BTN_CTA);

        tutorialContainer = UtilityUIBinding.QRequired<VisualElement>(tutorialOverlayPanel, TUTORIAL_CONTAINER);

        topBlocker = UtilityUIBinding.QRequired<VisualElement>(tutorialOverlayPanel, TOP_BLOCKER);
        rightBlocker = UtilityUIBinding.QRequired<VisualElement>(tutorialOverlayPanel, RIGHT_BLOCKER);
        bottomBlocker = UtilityUIBinding.QRequired<VisualElement>(tutorialOverlayPanel, BOTTOM_BLOCKER);
        leftBlocker = UtilityUIBinding.QRequired<VisualElement>(tutorialOverlayPanel, LEFT_BLOCKER);

        tutorialPopupContainer.style.display = DisplayStyle.None;
        highlightBox.style.display = DisplayStyle.None;

        tutorialOverlayContainer.style.display = DisplayStyle.None;

        Hide();
    }

    public void ExecuteTutorialStep(TutorialPopupData popupData, TutorialStep tutorialStep = null)
    {
        VisualElement targetVE = tutorialStep.target != null ? GetVisualElement(tutorialStep.target) : null;

        Show();
        ShowPopup(popupData, tutorialStep);

        HighlightElement(targetVE);

        tutorialPopupContainer.schedule.Execute(() =>
        {
            PositionPopup(targetVE);
        });
    }

    public void HighlightElement(VisualElement targetElement = null)
    {
        if (targetElement == null)
        {
            tutorialContainer.pickingMode = PickingMode.Position;
            highlightBox.style.display = DisplayStyle.None;
            return;
        }

        tutorialContainer.pickingMode = PickingMode.Ignore;

        Rect targetBounds = targetElement.worldBound;

        Vector2 topLeft = tutorialContainer.WorldToLocal(targetBounds.min);
        Vector2 bottomRight = tutorialContainer.WorldToLocal(targetBounds.max);

        float targetWidth = bottomRight.x - topLeft.x;
        float targetHeight = bottomRight.y - topLeft.y;

        float containerWidth = tutorialContainer.layout.width;
        float containerHeight = tutorialContainer.layout.height;

        topBlocker.style.left = 0;
        topBlocker.style.top = 0;
        topBlocker.style.width = containerWidth;
        topBlocker.style.height = topLeft.y;

        bottomBlocker.style.left = 0;
        bottomBlocker.style.top = bottomRight.y;
        bottomBlocker.style.width = containerWidth;
        bottomBlocker.style.height = containerHeight - bottomRight.y;

        leftBlocker.style.left = 0;
        leftBlocker.style.top = topLeft.y;
        leftBlocker.style.width = topLeft.x;
        leftBlocker.style.height = targetHeight;

        rightBlocker.style.left = bottomRight.x;
        rightBlocker.style.top = topLeft.y;
        rightBlocker.style.width = containerWidth - bottomRight.x;
        rightBlocker.style.height = targetHeight;


        highlightBox.style.left = topLeft.x - 5;
        highlightBox.style.top = topLeft.y - 5;
        highlightBox.style.width = targetWidth + 10;
        highlightBox.style.height = targetHeight + 10;

        highlightBox.style.display = DisplayStyle.Flex;
    }

    private void PositionPopup(VisualElement target = null)
    {
        const float spacing = 20f;
        const float edgePadding = 25f;

        if (target == null)
        {
            tutorialPopupContainer.style.left = 0;
            tutorialPopupContainer.style.top = 0;
            tutorialContainer.style.justifyContent = Justify.Center;
            return;
        }

        tutorialContainer.style.justifyContent = Justify.FlexStart;

        Rect overlayBounds = tutorialContainer.worldBound;

        float popupHeight = tutorialPopupContainer.resolvedStyle.height;
        float y;


        Rect targetBounds = target.worldBound;


        float spaceAbove = targetBounds.yMin - overlayBounds.yMin;
        float spaceBelow = overlayBounds.yMax - targetBounds.yMax;


        if (spaceBelow >= spaceAbove)
        {
            y = targetBounds.yMax + spacing;
        }
        else
        {
            y = targetBounds.yMin - popupHeight - spacing;
        }

        Vector2 localPosition = tutorialContainer.WorldToLocal(
            new Vector2(0, y)
        );

        float maxY = tutorialContainer.layout.height
            - popupHeight
            - edgePadding;

        localPosition.y = Mathf.Clamp(
            localPosition.y,
            edgePadding,
            maxY
        );

        tutorialPopupContainer.style.left = 0;
        tutorialPopupContainer.style.top = localPosition.y;
    }

    public void ShowPopup(TutorialPopupData popupData, TutorialStep tutorialStep)
    {
        if (!string.IsNullOrEmpty(popupData.imgAddress))
        {
            img.style.display = DisplayStyle.Flex;
            UtilityLoadAddressable.LoadAddressableIcon(popupData.imgAddress, img);
        }
        else
        {
            img.style.display = DisplayStyle.None;
        }

        if (!string.IsNullOrEmpty(popupData.heading))
        {
            heading.style.display = DisplayStyle.Flex;
            heading.text = popupData.heading;
        }
        else
        {
            heading.style.display = DisplayStyle.None;
        }

        if (!string.IsNullOrEmpty(popupData.description))
        {
            description.style.display = DisplayStyle.Flex;
            description.text = popupData.description;
        }
        else
        {
            description.style.display = DisplayStyle.None;
        }

        if (tutorialStep.onComplete != null)
        {
            if (oldActionRef != null)
            {
                cta.clicked -= oldActionRef;
            }

            oldActionRef = tutorialStep.onComplete;
            cta.clicked += oldActionRef;

            btnContainer.style.display = DisplayStyle.Flex;
        }
        else
        {
            if (oldActionRef != null)
            {
                cta.clicked -= oldActionRef;
                oldActionRef = null;
            }

            btnContainer.style.display = DisplayStyle.None;
        }

        tutorialPopupContainer.style.display = DisplayStyle.Flex;
    }

    private VisualElement GetVisualElement(string target)
    {
        return UtilityUIBinding.QRequired<VisualElement>(root, target);
    }

    public void Show()
    {
        tutorialContainer.style.display = DisplayStyle.Flex;
        tutorialOverlayContainer.style.display = DisplayStyle.Flex;
    }

    public void Hide()
    {
        tutorialContainer.style.display = DisplayStyle.None;
        tutorialOverlayContainer.style.display = DisplayStyle.None;
    }

    public void ResetOverlay()
    {
        if (oldActionRef != null && cta != null)
        {
            cta.clicked -= oldActionRef;
            oldActionRef = null;
        }

        if (tutorialOverlayContainer != null)
        {
            tutorialOverlayContainer.RemoveFromHierarchy();
        }

        tutorialOverlayPanel = null;
        tutorialContainer = null;
        tutorialOverlayContainer = null;
        cta = null;
    }
}
