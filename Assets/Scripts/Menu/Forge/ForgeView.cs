using UnityEngine;
using UnityEngine.UIElements;

public class ForgeView : IUIScreenView
{
    private VisualElement forgePanel;
    private VisualElement talentTreePanel;

    private TalentTreeView talentTreeView;

    public void Initialize(VisualElement root)
    {
        forgePanel = root.Q<VisualElement>("ForgePanel");
        talentTreePanel = root.Q<VisualElement>("TalentTreePanel");

        if (forgePanel == null)
        {
            Debug.LogWarning("ForgePanel no found");
            return;
        }
        if (talentTreePanel == null)
        {
            Debug.LogWarning("TalentTreePanel no found");
            return;
        }

        talentTreeView = new TalentTreeView();
        talentTreeView.Initialize(talentTreePanel);

        talentTreePanel.style.display = DisplayStyle.None;
        forgePanel.style.display = DisplayStyle.Flex;
    }
    public TalentTreeView GetTalentTreeView() => talentTreeView;
    public VisualElement GetForgePanel() => forgePanel;
    public VisualElement GetTalentTreePanel() => talentTreePanel;
}
