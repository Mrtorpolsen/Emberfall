using UnityEngine;
using UnityEngine.UIElements;

public class ForgeManager : IUIScreenManager
{
    private VisualElement forgePanel;
    private VisualElement talentTreePanel;

    public void Initialize(VisualElement root)
    {
        forgePanel = root.Q<VisualElement>("ForgePanel");
        talentTreePanel = root.Q<VisualElement>("TalentTreePanel");

        talentTreePanel.style.display = DisplayStyle.None;
    }

    public void OpenTalentTree()
    {
        forgePanel.style.display = DisplayStyle.None;
        talentTreePanel.style.display = DisplayStyle.Flex;

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
}
