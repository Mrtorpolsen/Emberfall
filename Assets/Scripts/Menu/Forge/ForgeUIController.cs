using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ForgeUIController : IUIScreenController
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

    public List<TalentNodeDefinition> GenerateTalentNodes(string treeToGenerate)
    {
        List<TalentNodeDefinition> talentNodes = new List<TalentNodeDefinition>();

        foreach (var talent in TalentService.Instance.playerTalentTree.GetTalentsByClass(treeToGenerate))
        {
            talentNodes.Add(BuildTalentNode(talent));
        }

        return talentNodes;
    }

    private TalentNodeDefinition BuildTalentNode(TalentDefinition talent)
    {
        var node = new TalentNodeDefinition();

        int purchased = TalentService.Instance.GetPurchasedTalent(talent.Id);
        int max = talent.Purchase.MaxPurchases;

        node.img = talent.IconId;
        node.heading = talent.Name;
        node.description = talent.Description;
        node.purchased = $"{purchased}/{max}";
        node.tier = talent.Tier;
        node.cost = talent.GetCurrentCost();

        node.onClick = () =>
        {
            int purchasedNow = TalentService.Instance.GetPurchasedTalent(talent.Id);
            bool canPurchase = purchasedNow < max;

            bool prerequisitsMet = TalentUnlockManager.Instance.ArePrerequisitesMet(talent.Id.Split("_")[0]
                .ToLowerInvariant(), talent.Prerequisites);

            int currentCost = talent.GetCurrentCost();
            bool hasEnoughCurrency = currentCost < CurrencyManager.Instance.Get(CurrencyTypes.Cinders);

            var popupBtn = new PopupButtonDefinition
            {
                //Todo need to reconstruct this to handle multiple prerequisites and different eg achievement
                LabelText = prerequisitsMet ? $"{purchasedNow}/{max}" : $"Requires {talent.Prerequisites[0].RequiredPointsInTier} points in Tier {talent.Prerequisites[0].RequiredTier}",
                BtnText = currentCost.ToString(),
                BtnIconPath = "UI/cinder_icon",

                OnClick = () =>
                {
                    //Repull live state
                    int purchasedAfter = TalentService.Instance.GetPurchasedTalent(talent.Id);
                    if (purchasedAfter >= max)
                        return;

                    int talentCost = talent.GetCurrentCost();
                    //TODO add the currency logic
                    bool succes = CurrencyManager.Instance.Spend(CurrencyTypes.Cinders, talentCost);

                    if(!succes)
                    {
                        Debug.LogWarning("Failed to spend currency / buy talent");
                        return;
                    }
                    //Save an add points to talent req
                    TalentService.Instance.SaveTalent(talent.Id);

                    var talentState = SaveService.Instance.Current.Talents;

                    if (!talentState.CurrencySpent.TryGetValue(CurrencyTypes.Cinders, out int spent)) 
                    {
                        talentState.CurrencySpent[CurrencyTypes.Cinders] = talentCost;
                    }
                    else
                    {
                        talentState.CurrencySpent[CurrencyTypes.Cinders] = spent + talentCost;
                    }

                    TalentUnlockManager.Instance
                        .AddPoints(talent.Id.Split("_")[0].ToLowerInvariant(), talent.Tier, 1);

                    int updated = TalentService.Instance.GetPurchasedTalent(talent.Id);

                    bool stillCanPurchase = updated < max && talentCost <= CurrencyManager.Instance.Get(CurrencyTypes.Cinders);
                    string purchasedTextNow = $"{updated}/{max}";

                    //update label to match new purchase
                    PopupManager.Instance.UpdateButtonLabel(purchasedTextNow);
                    PopupManager.Instance.ButtonIsActive(stillCanPurchase);

                    node.UpdatePurchasedText?.Invoke(updated, max);

                    SaveService.Instance.Save();
                }
            };
            PopupManager.Instance.OpenPopup(talent.IconId, talent.Name, talent.Description, popupBtn);
            PopupManager.Instance.ButtonIsActive(canPurchase && prerequisitsMet && hasEnoughCurrency);

            if(talent.Type != TalentType.StatModifier)
            {
                PopupManager.Instance.ButtonIsActive(false);
            }
        };
        return node;
    }

    public void RefundTalents()
    {
        foreach(var currency in SaveService.Instance.Current.Talents.CurrencySpent)
        {
            CurrencyManager.Instance.Add(currency.Key, currency.Value);
        }
        SaveService.Instance.Current.Talents.Purchases.Clear();
        SaveService.Instance.Current.Talents.CurrencySpent.Clear();
        TalentUnlockManager.Instance.ResetAll();
        SaveService.Instance.Save();
    }
}
