using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

public class PopupManager : MonoBehaviour
{
    public static PopupManager Instance { get; private set; }

    [SerializeField] private UIDocument uIDocument;

    private VisualElement root;
    private VisualElement blocker;
    private VisualElement container;

    private VisualElement contentImgDescBtn;
    private VisualElement img;
    private Label heading;
    private Label description;

    private VisualElement btnContainer;
    private Label btnLabel;
    private Button btn;

    private Action confirmAction;

    private EventCallback<ClickEvent> stopPropagationCallback;

    private const string BLOCKER = "PopupBlocker";
    private const string CONTAINER = "PopupContainer";

    private const string CONTENT_IMG_DESC_BTN = "PopupContentImgDescBtn";
    private const string IMG = "Img";
    private const string HEADING = "Label_Heading";
    private const string DESCRIPTION = "Label_Description";

    private const string BTNCONTAINER = "BtnContainer";
    private const string BTNLABEL = "Label_Btn";
    private const string BTN_CTA = "Btn_CTA";

    private const string CONTENT_BTN_BTN_BTN = "PopupContent3Img";
    private const string BTN_CONTAINER1 = "BtnContainer1";
    private const string BTN_CTA1 = "Btn_CTA1";
    private const string BTN_CONTAINER2 = "BtnContainer2";
    private const string BTN_CTA2 = "Btn_CTA2";
    private const string BTN_CONTAINER3 = "BtnContainer3";
    private const string BTN_CTA3 = "Btn_CTA3";


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void Initialize(VisualElement popupRoot)
    {
        // Unbind old callbacks if we are re-initializing
        if (blocker != null)
        {
            blocker.UnregisterCallback<ClickEvent>(OnBackgroundClicked);
        }

        if (container != null && stopPropagationCallback != null)
        {
            container.UnregisterCallback(stopPropagationCallback);
        }

        root = popupRoot;

        blocker = UtilityUIBinding.QRequired<VisualElement>(root, BLOCKER);
        container = UtilityUIBinding.QRequired<VisualElement>(root, CONTAINER);

        contentImgDescBtn = UtilityUIBinding.QRequired<VisualElement>(root, CONTENT_IMG_DESC_BTN);
        img = UtilityUIBinding.QRequired<VisualElement>(root, IMG);
        heading = UtilityUIBinding.QRequired<Label>(root, HEADING);
        description = UtilityUIBinding.QRequired<Label>(root, DESCRIPTION);
        btnContainer = UtilityUIBinding.QRequired<VisualElement>(root, BTNCONTAINER);
        btnLabel = UtilityUIBinding.QRequired<Label>(root, BTNLABEL);
        btn = UtilityUIBinding.QRequired<Button>(root, BTN_CTA);



        // Register callbacks
        blocker.RegisterCallback<ClickEvent>(OnBackgroundClicked);

        stopPropagationCallback ??= evt => evt.StopPropagation();
        container.RegisterCallback(stopPropagationCallback);

        blocker.style.display = DisplayStyle.None;
    }


    private void OnBackgroundClicked(ClickEvent evt)
    {
        Debug.Log("Background clicked");
        ClosePopup();
    }


    public void OpenPopup(
        string imgAddress = null,
        string title = null,
        string desc = null,
        PopupButtonDefinition buttonDefinition = null)
    {
        blocker.style.display = DisplayStyle.Flex;
        contentImgDescBtn.style.display = DisplayStyle.Flex;
        // Load image from path
        if (!string.IsNullOrEmpty(imgAddress))
        {
            UtilityLoadAddressable.LoadAddressableIcon(imgAddress, img);
        }
        else
        {
            img.style.display = DisplayStyle.None;
        }

        if (!string.IsNullOrEmpty(title))
        {
            heading.style.display = DisplayStyle.Flex;
            heading.text = title;
        }
        else
        {
            heading.style.display = DisplayStyle.None;
        }

        if (!string.IsNullOrEmpty(desc))
        {
            description.style.display = DisplayStyle.Flex;
            description.text = desc;
        }
        else
        {
            description.style.display = DisplayStyle.None;
        }

        // Configure button
        if (buttonDefinition != null && buttonDefinition.OnClick != null)
        {
            btnContainer.style.display = DisplayStyle.Flex;

            if (!string.IsNullOrEmpty(buttonDefinition.LabelText))
            {
                btnLabel.style.display = DisplayStyle.Flex;
                btnLabel.text = buttonDefinition.LabelText;
            }
            else
            {
                Debug.LogWarning("btn icon path is there but img not found");
                btnLabel.style.display = DisplayStyle.None;
            }

            if (!string.IsNullOrEmpty(buttonDefinition.BtnText))
            {
                btn.text = buttonDefinition.BtnText;
            }
            else
            {
                btn.text = "";
            }

            btn.iconImage = null;

            if (!string.IsNullOrEmpty(buttonDefinition.BtnIconPath))
            {
                Sprite sprite = Resources.Load<Sprite>(buttonDefinition.BtnIconPath);

                if (sprite != null)
                {
                    btn.iconImage = sprite.texture;
                }
            }
            //remove old if any
            btn.clicked -= CTAWrapper;

            confirmAction = buttonDefinition.OnClick;
            btn.clicked += CTAWrapper;
            btn.SetEnabled(true);
        }
        else
        {
            btnContainer.style.display = DisplayStyle.None;
        }
    }

    private void CTAWrapper()
    {
        confirmAction?.Invoke();
    }

    public void ClosePopup()
    {
        blocker.style.display = DisplayStyle.None;

        img.style.backgroundImage = null;
        heading.text = "";
        description.text = "";

        btnContainer.style.display = DisplayStyle.None;
        btnLabel.text = "";

        btn.iconImage = null;
        btn.text = "";
        btn.SetEnabled(false);

        btn.clicked -= CTAWrapper;
        confirmAction = null;
    }

    public void ButtonIsActive(bool active)
    {
        btn.SetEnabled(active);
        //btn.style.opacity = active ? 1f : 0.5f; //gray out when disabled
    }

    public void UpdateButtonLabel(string text)
    {
        if (btnLabel != null)
        {
            btnLabel.text = text;
        }
    }
}
