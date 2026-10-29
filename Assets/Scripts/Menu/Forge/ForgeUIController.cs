using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

public class ForgeUIController : IUIScreenController
{
    private VisualElement forgePanel;
    private VisualElement talentTreePanel;

    private ForgeView view;
    private TalentTreeView talentTreeView;

    private bool isDirty = false;

    public void Initialize(IUIScreenView screenView)
    {
        if (screenView is not ForgeView forgeView)
        {
            Debug.LogError("ForgeUIController received wrong view type.");
            return;
        }

        view = forgeView;

        forgePanel = forgeView.ForgePanel;
        talentTreePanel = forgeView.TalentTreePanel;

        //Get prerequisites from talents
        TalentUnlockManager.Instance.InitializeFromForge();

        //Enforce correct state
        talentTreePanel.style.display = DisplayStyle.None;
        forgePanel.style.display = DisplayStyle.Flex;

        view.RenderUnitContainers(GenerateUnitContainers());
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
        if (isDirty)
        {
            view.RenderUnitContainers(GenerateUnitContainers());
            isDirty = false;
        }

        forgePanel.style.display = DisplayStyle.Flex;
        talentTreePanel.style.display = DisplayStyle.None;

        talentTreeView.ClearTalentRows();
    }

    public void Cleanup()
    {
        view.Cleanup();
    }

    public List<UnitTalentContainerDefinition> GenerateUnitContainers()
    {
        List<UnitTalentContainerDefinition> unitContainers = new List<UnitTalentContainerDefinition>();
        if (TalentService.Instance.playerTalentTree == null)
        {
            throw new InvalidOperationException("Player Talent Tree is null. Cannot generate unit containers.");
        }

        foreach (var unit in TalentService.Instance.playerTalentTree.UnitDefinitions)
        {
            unitContainers.Add(BuildUnitContainer(unit.Key, unit.Value.IconId));
        }

        return unitContainers;
    }

    public UnitTalentContainerDefinition BuildUnitContainer(string unitId, string iconId)
    {
        var container = new UnitTalentContainerDefinition
        {
            id = unitId,
            img = iconId,
            onClick = () => OpenTalentTree(unitId),
            isUnlocked = IsUnlocked(unitId)
        };
        return container;
    }

    public List<TalentNodeDefinition> GenerateTalentNodes(string treeToGenerate)
    {
        List<TalentNodeDefinition> talentNodes = new List<TalentNodeDefinition>();

        foreach (var talent in TalentService.Instance.playerTalentTree.GetTalentsByClass(treeToGenerate))
        {
            talentNodes.Add(BuildTalentNode(talent, treeToGenerate));
        }

        return talentNodes;
    }

    private TalentNodeDefinition BuildTalentNode(Talent talent, string unitName)
    {
        var node = new TalentNodeDefinition();

        int purchased = TalentService.Instance.GetPurchasedTalent(unitName, talent.Id);
        int max = talent.Purchase.MaxPurchases;

        node.img = talent.IconId;
        node.heading = talent.Name;
        node.description = talent.Description;
        node.purchased = $"{purchased}/{max}";
        node.tier = talent.Tier;
        node.cost = talent.GetCurrentCost();

        node.onClick = () =>
        {
            int purchasedNow = TalentService.Instance.GetPurchasedTalent(unitName, talent.Id);
            bool canPurchase = purchasedNow < max;

            bool prerequisitesMet = TalentUnlockManager.Instance.ArePrerequisitesMet(unitName.ToLowerInvariant()
                .ToLowerInvariant(), talent.Prerequisites);

            int currentCost = talent.GetCurrentCost();
            bool hasEnoughCurrency = TalentService.Instance.CanAfford(currentCost);

            var popupBtn = new PopupButtonDefinition
            {
                //Todo need to reconstruct this to handle multiple prerequisites and different eg achievement
                LabelText = prerequisitesMet ? $"{purchasedNow}/{max}" : $"Requires {talent.Prerequisites[0].RequiredPointsInTier} point{(talent.Prerequisites[0].RequiredPointsInTier == 1 ? "" : "s")} in Tier {talent.Prerequisites[0].RequiredTier}",
                BtnText = currentCost.ToString(),
                BtnIconPath = "UI/cinder_icon",

                OnClick = async () =>
                {
                    var result = await TalentService.Instance.TryPurchaseAsync(unitName, talent);
                    if (!result.Success)
                        return;

                    //update label to match new purchase
                    PopupManager.Instance.UpdateButtonLabel($"{result.Owned}/{max}");
                    PopupManager.Instance.ButtonIsActive(result.CanBuyMore);

                    node.UpdatePurchasedText?.Invoke(result.Owned, max);

                    if (talent.IsUnlockType)
                    {
                        isDirty = true;
                    }
                }
            };
            PopupManager.Instance.OpenPopup_ImgDescBtn(talent.IconId, talent.Name, talent.Description, popupBtn);
            PopupManager.Instance.ButtonIsActive(canPurchase && prerequisitesMet && hasEnoughCurrency);

            if (talent.Type != TalentType.StatModifier && talent.Type != TalentType.UnitUnlock)
            {
                PopupManager.Instance.ButtonIsActive(false);
            }
        };
        return node;
    }

    public async Task RefundTalentsAsync()
    {
        foreach (var currency in SaveService.Instance.Current.Talents.CurrencySpent)
        {
            CurrencyManager.Instance.Add(currency.Key, currency.Value);
        }
        SaveService.Instance.Current.Talents.Purchases.Clear();
        SaveService.Instance.Current.Talents.CurrencySpent.Clear();
        TalentUnlockManager.Instance.ResetAll();
        await SaveService.Instance.SaveAsync();
    }

    private bool IsUnlocked(string unitName)
    {
        return UnlockService.Instance.IsUnlocked("unlock_unit_" + unitName.ToLowerInvariant());
    }
}
