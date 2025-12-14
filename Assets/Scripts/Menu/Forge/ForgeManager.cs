using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ForgeManager : IUIScreenManager
{
    private VisualElement forgePanel;
    private VisualElement talentTreePanel;

    private TalentTreeView talentTreeView;

    public void Initialize(VisualElement root)
    {
        forgePanel = root.Q<VisualElement>("ForgePanel");
        talentTreePanel = root.Q<VisualElement>("TalentTreePanel");

        //Enforce correct state
        talentTreePanel.style.display = DisplayStyle.None;
        forgePanel.style.display = DisplayStyle.Flex;
    }

    public void SetTalentTreeView(TalentTreeView view)
    {
        talentTreeView = view;
    }

    public void OpenTalentTree(string treeToGenerate)
    {
        var talentNodes = GenerateTalentNodes(treeToGenerate);
        talentTreeView.RenderTalentTree(talentNodes);

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
        talentTreeView.Cleanup();
    }

    public void OpenPopup()
    {
        PopupButtonDefinition buttonBaby = new PopupButtonDefinition
        {
            BtnText = "100",
            LabelText = "0/5",
            BtnIconPath = "UI/Images/Talents/cinder_icon",
            OnClick = () => Debug.Log("Button clicked!")
        };
        PopupManager.main.OpenPopup("UI/Images/Talents/place_holder_icon", "Strike Training", "Increases fighter's attack damage by 1% per purchase", buttonBaby);
        //Todo if needed
    }

    public List<TalentNodeDefinition> GenerateTalentNodes(string treeToGenerate)
    {
        List<TalentNodeDefinition> talentNodes = new List<TalentNodeDefinition>();

        foreach (var talent in TalentManager.main.playerTalentTree.GetTalents(treeToGenerate))
        {
            talentNodes.Add(BuildTalentNode(talent));
        }

        return talentNodes;
    }

    private TalentNodeDefinition BuildTalentNode(TalentDefinition talent)
    {
        return new TalentNodeDefinition
        {
            img = talent.IconId,
            heading = talent.Name,
            description = talent.Description,
            unlocked = $"{talent.Purchase.Purchased}/{talent.Purchase.MaxPurchases}",
            tier = talent.Tier,
            cost = talent.GetCurrentCost(),

            onClick = () =>
            {
                var popupBtn = new PopupButtonDefinition
                {
                    LabelText = $"{talent.Purchase.Purchased}/{talent.Purchase.MaxPurchases}",
                    BtnText = talent.GetCurrentCost().ToString(),
                    BtnIconPath = "UI/Images/Talents/cinder_icon",

                    OnClick = () =>
                    {
                        Debug.Log($"{talent.Id} cost {talent.GetCurrentCost()}");
                        //TODO: add purchase func
                    }
                };
            }
        };
    }

}
