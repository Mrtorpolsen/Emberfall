using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;

public class LoadoutView : IUIScreenView
{
    private VisualElement unitRowContainer;
    private VisualElement towerRowContainer;
    private VisualElement utilityRowContainer;

    private VisualElement loadoutUnitCardContainer;
    private VisualElement loadoutTowerCardContainer;
    private VisualElement loadoutUtilityCardContainer;
    private Dictionary<DefinitionCategory, VisualElement> cardContainers;

    private Button loadoutUnitTab;
    private Button loadoutUtilityTab;
    private Button loadoutTowerTab;
    private Dictionary<DefinitionCategory, Button> tabButtons;

    private Label currentLoadoutHeading;

    private VisualTreeAsset loadoutSelectNode;
    private VisualTreeAsset loadoutCard;

    private const string LOADOUT_SELECT_NODE_ADDRESSABLE = "UI/LoadoutSelectNode";
    private const string LOADOUT_CARD_ADDRESSABLE = "UI/LoadoutCard";

    public async Task InitializeAsync(VisualElement root)
    {
        currentLoadoutHeading = UtilityUIBinding.QRequired<Label>(root, "Label_CurrentLoadout");

        unitRowContainer = UtilityUIBinding.QRequired<VisualElement>(root, "UnitRowContainer");
        towerRowContainer = UtilityUIBinding.QRequired<VisualElement>(root, "TowerRowContainer");
        utilityRowContainer = UtilityUIBinding.QRequired<VisualElement>(root, "UtilityRowContainer");

        loadoutUnitCardContainer = UtilityUIBinding.QRequired<VisualElement>(root, "LoadoutUnitCardContainer");
        loadoutTowerCardContainer = UtilityUIBinding.QRequired<VisualElement>(root, "LoadoutTowerCardContainer");
        loadoutUtilityCardContainer = UtilityUIBinding.QRequired<VisualElement>(root, "LoadoutUtilityCardContainer");

        loadoutUnitTab = UtilityUIBinding.QRequired<Button>(root, "Button_Unit_Tab");
        loadoutTowerTab = UtilityUIBinding.QRequired<Button>(root, "Button_Tower_Tab");
        loadoutUtilityTab = UtilityUIBinding.QRequired<Button>(root, "Button_Utility_Tab");

        cardContainers = new()
        {
            { DefinitionCategory.Unit, loadoutUnitCardContainer },
            { DefinitionCategory.Tower, loadoutTowerCardContainer },
            { DefinitionCategory.Utility, loadoutUtilityCardContainer }
        };

        tabButtons = new()
        {
            { DefinitionCategory.Unit, loadoutUnitTab },
            { DefinitionCategory.Tower, loadoutTowerTab },
            { DefinitionCategory.Utility, loadoutUtilityTab }
        };

        loadoutSelectNode = await Addressables.LoadAssetAsync<VisualTreeAsset>(LOADOUT_SELECT_NODE_ADDRESSABLE).Task;
        loadoutCard = await Addressables.LoadAssetAsync<VisualTreeAsset>(LOADOUT_CARD_ADDRESSABLE).Task;

        if(loadoutSelectNode == null)
        {
            throw new InvalidOperationException($"Failed to load {LOADOUT_SELECT_NODE_ADDRESSABLE}.");
        }

        //Default view to units
        ShowCardContainer(DefinitionCategory.Unit);
    }

    public void RenderLoadouts(List<LoadoutSlotViewModel> loadouts)
    {
        ClearContainer(unitRowContainer );
        ClearContainer(towerRowContainer);
        ClearContainer(utilityRowContainer);

        foreach (var loadout in loadouts)
        {
            var visualNode = new LoadoutSlotElement(loadoutSelectNode);

            visualNode.Bind(loadout);
            if (loadout.SlotType == DefinitionCategory.Utility)
            {
                utilityRowContainer.Add(visualNode.Root);
            }
            else if (loadout.SlotType == DefinitionCategory.Tower)
            {
                towerRowContainer.Add(visualNode.Root);
            }
            else if (loadout.SlotType == DefinitionCategory.Unit)
            {
                unitRowContainer.Add(visualNode.Root);
            }
        }
    }

    public void RenderLoadoutCards(List<LoadoutCardViewModel> loadoutCards)
    {
        ClearContainer(loadoutUnitCardContainer);
        ClearContainer(loadoutTowerCardContainer);
        ClearContainer(loadoutUtilityCardContainer);

        foreach (var loadoutCard in loadoutCards)
        {
            var visualNode = new LoadoutCardElement(this.loadoutCard);

            visualNode.Bind(loadoutCard);

            if (loadoutCard.Type == DefinitionCategory.Unit)
            {
                loadoutUnitCardContainer.Add(visualNode.Root);
            }
            else if (loadoutCard.Type == DefinitionCategory.Tower)
            {
                loadoutTowerCardContainer.Add(visualNode.Root);
            }
            else if (loadoutCard.Type == DefinitionCategory.Utility)
            {
                loadoutUtilityCardContainer.Add(visualNode.Root);
            }
        }
    }

    public void SetLoadoutHeading(string heading)
    {
        currentLoadoutHeading.text = heading;
    }

    public void ShowCardContainer(DefinitionCategory category)
    {
        foreach (var kvp in cardContainers)
        {
            kvp.Value.style.display =
                kvp.Key == category
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;
        }
        foreach (var kvp in tabButtons)
        {
            if(kvp.Key == category)
            {
                kvp.Value.AddToClassList("active");
            }
            else
            {
                kvp.Value.RemoveFromClassList("active");
            }
        }

    }

    private void ClearContainer(VisualElement container)
    {
        container.Clear();
    }
}