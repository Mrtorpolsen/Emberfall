using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
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

        //Get prerequisits from talents
        TalentUnlockManager.Instance.InitializeFromForge();

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
        PopupManager.Instance.OpenPopup("place_holder_icon", "Strike Training", "Increases fighter's attack damage by 1% per purchase", buttonBaby);
        //Todo if needed
    }

    public List<TalentNodeDefinition> GenerateTalentNodes(string treeToGenerate)
    {
        List<TalentNodeDefinition> talentNodes = new List<TalentNodeDefinition>();

        foreach (var talent in TalentManager.Instance.playerTalentTree.GetTalentsByClass(treeToGenerate))
        {
            talentNodes.Add(BuildTalentNode(talent));
        }

        return talentNodes;
    }

    private TalentNodeDefinition BuildTalentNode(TalentDefinition talent)
    {
        var node = new TalentNodeDefinition();

        int purchased = SaveService.Instance.GetPurchases(talent.Id);
        int max = talent.Purchase.MaxPurchases;

        node.img = talent.IconId;
        node.heading = talent.Name;
        node.description = talent.Description;
        node.purchased = $"{purchased}/{max}";
        node.tier = talent.Tier;
        node.cost = talent.GetCurrentCost();

        node.onClick = () =>
        {
            int purchasedNow = SaveService.Instance.GetPurchases(talent.Id);
            bool canPurchase = purchased < max;

            bool prerequisitsMet = TalentUnlockManager.Instance.ArePrerequisitesMet(talent.Id.Split("_")[0]
                .ToLowerInvariant(), talent.Prerequisites);

            var popupBtn = new PopupButtonDefinition
            {
                //Todo need to reconstruct this to handle multiple prerequisites and different eg achievement
                LabelText = prerequisitsMet ? $"{purchasedNow}/{max}" : $"Requires {talent.Prerequisites[0].RequiredPointsInTier} points in Tier {talent.Prerequisites[0].RequiredTier}",
                BtnText = talent.GetCurrentCost().ToString(),
                BtnIconPath = "UI/Images/Talents/cinder_icon",

                OnClick = () =>
                {
                    //TODO add the currency logic

                    //Save an add points to talent req
                    SaveService.Instance.AddToSave(talent.Id);
                    TalentUnlockManager.Instance
                        .AddPoints(talent.Id.Split("_")[0].ToLowerInvariant(), talent.Tier, 1);

                    int updated = SaveService.Instance.GetPurchases(talent.Id);
                    bool stillCanPurchase = updated < max;
                    string purchasedTextNow = $"{updated}/{max}";

                    //update label to match new purchase
                    PopupManager.Instance.UpdateButtonLabel(purchasedTextNow);
                    PopupManager.Instance.ButtonIsActive(stillCanPurchase);

                    node.UpdatePurchasedText?.Invoke(updated, max);
                    SaveService.Instance.Save();
                }
            };
            PopupManager.Instance.OpenPopup(talent.IconId, talent.Name, talent.Description, popupBtn);

            PopupManager.Instance.ButtonIsActive(canPurchase && prerequisitsMet);

            if(talent.Type != TalentType.StatModifier)
            {
                PopupManager.Instance.ButtonIsActive(false);
            }
        };
        return node;
    }
}
