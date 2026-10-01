using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
    public const string LOADOUT_BTN_UNIT_TAB = "Btn_Unit_Tab";
    public const string LOADOUT_BTN_TOWER_TAB = "Btn_Tower_Tab";
    public const string LOADOUT_BTN_UTILITY_TAB = "Btn_Utility_Tab";

    public async Task InitializeAsync(VisualElement root)
    {
        currentLoadoutHeading = UtilityUIBinding.QRequired<Label>(root, "Label_CurrentLoadout");

        unitRowContainer = UtilityUIBinding.QRequired<VisualElement>(root, "UnitRowContainer");
        towerRowContainer = UtilityUIBinding.QRequired<VisualElement>(root, "TowerRowContainer");
        utilityRowContainer = UtilityUIBinding.QRequired<VisualElement>(root, "UtilityRowContainer");

        loadoutUnitCardContainer = UtilityUIBinding.QRequired<VisualElement>(root, "LoadoutUnitCardContainer");
        loadoutTowerCardContainer = UtilityUIBinding.QRequired<VisualElement>(root, "LoadoutTowerCardContainer");
        loadoutUtilityCardContainer = UtilityUIBinding.QRequired<VisualElement>(root, "LoadoutUtilityCardContainer");

        loadoutUnitTab = UtilityUIBinding.QRequired<Button>(root, LOADOUT_BTN_UNIT_TAB);
        loadoutTowerTab = UtilityUIBinding.QRequired<Button>(root, LOADOUT_BTN_TOWER_TAB);
        loadoutUtilityTab = UtilityUIBinding.QRequired<Button>(root, LOADOUT_BTN_UTILITY_TAB);

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

        if (loadoutSelectNode == null)
        {
            throw new InvalidOperationException($"Failed to load {LOADOUT_SELECT_NODE_ADDRESSABLE}.");
        }

        //Default view to units
        ShowCardContainer(DefinitionCategory.Unit);
    }

    public void RenderLoadouts(List<LoadoutSlotViewModel> loadouts)
    {
        ClearContainer(unitRowContainer);
        ClearContainer(towerRowContainer);
        ClearContainer(utilityRowContainer);

        //setting for tutorial
        int utilityIndex = 0;
        int towerIndex = 0;
        int unitIndex = 0;

        foreach (var loadout in loadouts)
        {
            var visualNode = new LoadoutSlotElement(loadoutSelectNode);

            visualNode.Bind(loadout);
            if (loadout.SlotType == DefinitionCategory.Utility)
            {
                utilityRowContainer.Add(visualNode.Root);
                visualNode.Root.name = "LoadoutSlot" + DefinitionCategory.Utility.ToString() + utilityIndex;
                utilityIndex++;
            }
            else if (loadout.SlotType == DefinitionCategory.Tower)
            {
                towerRowContainer.Add(visualNode.Root);
                visualNode.Root.name = "LoadoutSlot" + DefinitionCategory.Tower.ToString() + towerIndex;
                towerIndex++;
            }
            else if (loadout.SlotType == DefinitionCategory.Unit)
            {
                unitRowContainer.Add(visualNode.Root);
                visualNode.Root.name = "LoadoutSlot" + DefinitionCategory.Unit.ToString() + unitIndex;
                unitIndex++;
            }
        }
    }

    public void RenderLoadoutCards(List<LoadoutCardViewModel> loadoutCards)
    {
        ClearContainer(loadoutUnitCardContainer);
        ClearContainer(loadoutTowerCardContainer);
        ClearContainer(loadoutUtilityCardContainer);

        int utilityIndex = 0;
        int towerIndex = 0;
        int unitIndex = 0;

        foreach (var loadoutCard in loadoutCards)
        {
            var visualNode = new LoadoutCardElement(this.loadoutCard);

            visualNode.Bind(loadoutCard);

            if (loadoutCard.Type == DefinitionCategory.Unit)
            {
                loadoutUnitCardContainer.Add(visualNode.Root);
                visualNode.Root.name = "LoadoutCard" + DefinitionCategory.Unit.ToString() + unitIndex;
                unitIndex++;
            }
            else if (loadoutCard.Type == DefinitionCategory.Tower)
            {
                loadoutTowerCardContainer.Add(visualNode.Root);
                visualNode.Root.name = "LoadoutCard" + DefinitionCategory.Tower.ToString() + towerIndex;
                towerIndex++;
            }
            else if (loadoutCard.Type == DefinitionCategory.Utility)
            {
                loadoutUtilityCardContainer.Add(visualNode.Root);
                visualNode.Root.name = "LoadoutCard" + DefinitionCategory.Utility.ToString() + utilityIndex;
                utilityIndex++;
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
            if (kvp.Key == category)
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