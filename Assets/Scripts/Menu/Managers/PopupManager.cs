using System;
using UnityEngine;
using UnityEngine.UIElements;

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

    private VisualElement btnImgDescBtnContainer;
    private Label btnImgDescBtnLabel;
    private Button btnImgDescBtn;

    private Action btnImgDescBtnAction;

    private VisualElement contentChoice;
    private VisualElement btnContainer1;
    private VisualElement btnContainer2;
    private VisualElement btnContainer3;
    private VisualElement btnContainer4;
    private Button btn1;
    private Button btn2;
    private Button btn3;
    private Button btn4;

    private Action btnAction1;
    private Action btnAction2;
    private Action btnAction3;
    private Action btnAction4;

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

    private const string CONTENT_CHOICE = "PopupContentChoice";
    private const string BTN_CONTAINER1 = "BtnContainer1";
    private const string BTN_CTA1 = "Btn_CTA1";
    private const string BTN_CONTAINER2 = "BtnContainer2";
    private const string BTN_CTA2 = "Btn_CTA2";
    private const string BTN_CONTAINER3 = "BtnContainer3";
    private const string BTN_CTA3 = "Btn_CTA3";
    private const string BTN_CONTAINER4 = "BtnContainer4";
    private const string BTN_CTA4 = "Btn_CTA4";


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
        btnImgDescBtnContainer = UtilityUIBinding.QRequired<VisualElement>(root, BTNCONTAINER);
        btnImgDescBtnLabel = UtilityUIBinding.QRequired<Label>(root, BTNLABEL);
        btnImgDescBtn = UtilityUIBinding.QRequired<Button>(root, BTN_CTA);

        contentChoice = UtilityUIBinding.QRequired<VisualElement>(root, CONTENT_CHOICE);
        btnContainer1 = UtilityUIBinding.QRequired<VisualElement>(root, BTN_CONTAINER1);
        btn1 = UtilityUIBinding.QRequired<Button>(root, BTN_CTA1);
        btnContainer2 = UtilityUIBinding.QRequired<VisualElement>(root, BTN_CONTAINER2);
        btn2 = UtilityUIBinding.QRequired<Button>(root, BTN_CTA2);
        btnContainer3 = UtilityUIBinding.QRequired<VisualElement>(root, BTN_CONTAINER3);
        btn3 = UtilityUIBinding.QRequired<Button>(root, BTN_CTA3);
        btnContainer4 = UtilityUIBinding.QRequired<VisualElement>(root, BTN_CONTAINER4);
        btn4 = UtilityUIBinding.QRequired<Button>(root, BTN_CTA4);

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

    public void OpenPopup_ImgDescBtn(
        string imgAddress = null,
        string heading = null,
        string description = null,
        PopupButtonDefinition buttonDefinition = null)
    {
        blocker.style.display = DisplayStyle.Flex;
        contentImgDescBtn.style.display = DisplayStyle.Flex;

        contentChoice.style.display = DisplayStyle.None;

        // Load image from path
        if (!string.IsNullOrEmpty(imgAddress))
        {
            img.style.display = DisplayStyle.Flex;
            UtilityLoadAddressable.LoadAddressableIcon(imgAddress, img);
        }
        else
        {
            img.style.display = DisplayStyle.None;
        }

        if (!string.IsNullOrEmpty(heading))
        {
            this.heading.style.display = DisplayStyle.Flex;
            this.heading.text = heading;
        }
        else
        {
            this.heading.style.display = DisplayStyle.None;
        }

        if (!string.IsNullOrEmpty(description))
        {
            this.description.style.display = DisplayStyle.Flex;
            this.description.text = description;
        }
        else
        {
            this.description.style.display = DisplayStyle.None;
        }

        // Configure button
        if (buttonDefinition != null && buttonDefinition.OnClick != null)
        {
            ConfigureButton(buttonDefinition, btnImgDescBtnContainer, btnImgDescBtnLabel, btnImgDescBtn, ref btnImgDescBtnAction);
        }
        else
        {
            btnImgDescBtnContainer.style.display = DisplayStyle.None;
        }
    }

    public void OpenChoicePopup(PopupButtonDefinition buttonDefinition1 = null, PopupButtonDefinition buttonDefinition2 = null,
        PopupButtonDefinition buttonDefinition3 = null, PopupButtonDefinition buttonDefinition4 = null)
    {
        blocker.style.display = DisplayStyle.Flex;
        contentChoice.style.display = DisplayStyle.Flex;
        contentImgDescBtn.style.display = DisplayStyle.None;

        if (buttonDefinition1 != null && buttonDefinition1.OnClick != null)
        {
            ConfigureButton(buttonDefinition1, btnContainer1, null, btn1, ref btnAction1);
        }
        else
        {
            btnContainer1.style.display = DisplayStyle.None;
        }

        if (buttonDefinition2 != null && buttonDefinition2.OnClick != null)
        {
            ConfigureButton(buttonDefinition2, btnContainer2, null, btn2, ref btnAction2);
        }
        else
        {
            btnContainer2.style.display = DisplayStyle.None;
        }

        if (buttonDefinition3 != null && buttonDefinition3.OnClick != null)
        {
            ConfigureButton(buttonDefinition3, btnContainer3, null, btn3, ref btnAction3);
        }
        else
        {
            btnContainer3.style.display = DisplayStyle.None;
        }

        if (buttonDefinition4 != null && buttonDefinition4.OnClick != null)
        {
            ConfigureButton(buttonDefinition4, btnContainer4, null, btn4, ref btnAction4);
        }
        else
        {
            btnContainer4.style.display = DisplayStyle.None;
        }
    }

    public void ClosePopup()
    {
        blocker.style.display = DisplayStyle.None;

        img.style.display = DisplayStyle.None;
        img.style.backgroundImage = null;
        heading.text = "";
        description.text = "";

        btnImgDescBtnContainer.style.display = DisplayStyle.None;
        btnImgDescBtnLabel.text = "";

        btnImgDescBtn.iconImage = null;
        btnImgDescBtn.text = "";
        btnImgDescBtn.SetEnabled(false);

        if (btnImgDescBtnAction != null)
        {
            btnImgDescBtn.clicked -= btnImgDescBtnAction;
        }

        btnImgDescBtnAction = null;

        btnContainer1.style.display = DisplayStyle.None;
        btnContainer2.style.display = DisplayStyle.None;
        btnContainer3.style.display = DisplayStyle.None;

        btn1.SetEnabled(false);
        btn2.SetEnabled(false);
        btn3.SetEnabled(false);

        if (btnAction1 != null)
        {
            btn1.clicked -= btnAction1;
        }
        if (btnAction2 != null)
        {
            btn2.clicked -= btnAction2;
        }
        if (btnAction3 != null)
        {
            btn3.clicked -= btnAction3;
        }
        if (btnAction4 != null)
        {
            btn4.clicked -= btnAction4;
        }

        btnAction1 = null;
        btnAction2 = null;
        btnAction3 = null;
        btnAction4 = null;
    }

    private void ConfigureButton(PopupButtonDefinition buttonDefinition,
        VisualElement btnContainer,
        Label btnLabel, Button btn,
        ref Action currentAction)
    {
        btnContainer.style.display = DisplayStyle.Flex;

        if (currentAction != null)
        {
            btn.clicked -= currentAction;
            currentAction = null;
        }

        if (btnLabel != null)
        {
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

        currentAction = buttonDefinition.OnClick;
        btn.clicked += currentAction;
        btn.SetEnabled(true);
    }

    public void ButtonIsActive(bool active)
    {
        btnImgDescBtn.SetEnabled(active);
        //btn.style.opacity = active ? 1f : 0.5f; //gray out when disabled
    }

    public void UpdateButtonLabel(string text)
    {
        if (btnImgDescBtnLabel != null)
        {
            btnImgDescBtnLabel.text = text;
        }
    }
}
