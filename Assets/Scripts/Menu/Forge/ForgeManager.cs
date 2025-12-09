using UnityEngine;
using UnityEngine.UIElements;

public class ForgeManager : IUIScreenManager
{
    private VisualElement forgePanel;
    private VisualElement talentTreePanel;
    private VisualElement fullScreenTalentPopupContainer;

    public void Initialize(VisualElement root)
    {
        forgePanel = root.Q<VisualElement>("ForgePanel");
        talentTreePanel = root.Q<VisualElement>("TalentTreePanel");
        fullScreenTalentPopupContainer = root.Q<VisualElement>("FullScreenTalentPopupContainer");

        //Enforce correct state
        talentTreePanel.style.display = DisplayStyle.None;
        fullScreenTalentPopupContainer.style.display = DisplayStyle.None;
        forgePanel.style.display = DisplayStyle.Flex;
    }

    public void OpenTalentTree()
    {
        forgePanel.style.display = DisplayStyle.None;
        talentTreePanel.style.display = DisplayStyle.Flex;
        fullScreenTalentPopupContainer.style.display = DisplayStyle.Flex;

        //Setup for talentTree
    }

    public void BackToForge()
    {
        forgePanel.style.display = DisplayStyle.Flex;
        talentTreePanel.style.display = DisplayStyle.None;

        //Cleanup?
    }

    public void Cleanup()
    {
        //Todo if needed
    }

    public void OpenPopup()
    {
        PopupButtonDefinition buttonBaby = new PopupButtonDefinition
        {
            BtnText = "Confirm",
            LabelText = "Extra info",
            BtnIconPath = "UI/Talents/cinder_icon",
            OnClick = () => Debug.Log("Button clicked!")
        };
        PopupManager.main.OpenPopup("UI/Talsdsts/place_holder_icon", "Test baby", "This is the description baby", buttonBaby);
        //Todo if needed
    }
}
