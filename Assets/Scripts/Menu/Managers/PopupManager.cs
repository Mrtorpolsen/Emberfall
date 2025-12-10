using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

public class PopupManager : MonoBehaviour
{
    public static PopupManager main { get; private set; }

    [SerializeField] private UIDocument uIDocument;

    private VisualElement root;

    private VisualElement blocker;
    private VisualElement container;
    private VisualElement img;
    private Label heading;
    private Label description;

    private VisualElement btnContainer;
    private Label btnLabel;
    private Button btn;

    private Action confirmAction;

    private const string BLOCKER_NAME = "PopupBlocker";
    private const string CONTAINER_NAME = "PopupContainer";
    private const string IMG_NAME = "Img";
    private const string HEADING_NAME = "Label_Heading";
    private const string DESCRIPTION_NAME = "Label_Description";

    private const string BTNCONTAINER_NAME = "BtnContainer";
    private const string BTNLABEL_NAME = "Label_Btn";
    private const string CTA_NAME = "Btn_CTA";

    private void Awake()
    {
        if (main != null && main != this)
        {
            Destroy(gameObject);
            return;
        }

        main = this;
    }

    public void Initialize(VisualElement popupRoot)
    {
        root = popupRoot;

        blocker = root.Q<VisualElement>(BLOCKER_NAME);
        container = root.Q<VisualElement>(CONTAINER_NAME);

        img = root.Q<VisualElement>(IMG_NAME);
        heading = root.Q<Label>(HEADING_NAME);
        description = root.Q<Label>(DESCRIPTION_NAME);
        btnContainer = root.Q<VisualElement>(BTNCONTAINER_NAME);
        btnLabel = root.Q<Label>(BTNLABEL_NAME);
        btn = root.Q<Button>(CTA_NAME);

        blocker.RegisterCallback<ClickEvent>(OnBackgroundClicked);
        // Stop clicks from bubbling up from the popup container
        container.RegisterCallback<ClickEvent>(evt => evt.StopPropagation());

        blocker.style.display = DisplayStyle.None;
    }

    private void OnBackgroundClicked(ClickEvent evt)
    {
        Debug.Log("Background clicked");
        ClosePopup();
    }


    public void OpenPopup(
        string imagePath = null,
        string title = null,
        string desc = null,
        PopupButtonDefinition buttonDefinition = null)
    {
        blocker.style.display = DisplayStyle.Flex;

        // Load image from path
        if (!string.IsNullOrEmpty(imagePath))
        {
            Sprite sprite = Resources.Load<Sprite>(imagePath);

            if (sprite != null)
            {
                img.style.display = DisplayStyle.Flex;
                img.style.backgroundImage = new StyleBackground(sprite.texture);
            }
            else
            {
                img.style.display = DisplayStyle.None;
                Debug.LogWarning("imgPath is there but img not found");
            }
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
        ClosePopup();
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
}
