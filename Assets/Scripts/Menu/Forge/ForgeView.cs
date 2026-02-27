using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

public class ForgeView : IUIScreenView
{
    public VisualElement ForgePanel { get; private set; }
    public VisualElement TalentTreePanel { get; private set; }

    private TalentTreeView talentTreeView;

    public Task InitializeAsync(VisualElement root)
    {
        ForgePanel = root.Q<VisualElement>("ForgePanel");
        TalentTreePanel = root.Q<VisualElement>("TalentTreePanel");

        if (ForgePanel == null)
        {
            Debug.LogWarning("ForgePanel no found");
            return Task.CompletedTask;
        }
        if (TalentTreePanel == null)
        {
            Debug.LogWarning("TalentTreePanel no found");
            return Task.CompletedTask;
        }

        talentTreeView = new TalentTreeView();
        talentTreeView.Initialize(TalentTreePanel);

        TalentTreePanel.style.display = DisplayStyle.None;
        ForgePanel.style.display = DisplayStyle.Flex;

        return Task.CompletedTask;
    }

    public void Cleanup()
    {
        talentTreeView.CleanupClicks();
    }

    public TalentTreeView GetTalentTreeView() => talentTreeView;
    public VisualElement GetForgePanel() => ForgePanel;
    public VisualElement GetTalentTreePanel() => TalentTreePanel;
}
