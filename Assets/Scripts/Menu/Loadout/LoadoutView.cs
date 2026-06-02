using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;

public class LoadoutView : IUIScreenView
{
    private VisualElement utilityRowContainer;
    private VisualElement towerRowContainer;
    private VisualElement unitRowContainer;
    private VisualElement loadoutCardContainer;

    private Label currentLoadoutHeading;

    private VisualTreeAsset loadoutSelectNode;
    private VisualTreeAsset loadoutCard;

    private const string LOADOUT_SELECT_NODE_ADDRESSABLE = "UI/LoadoutSelectNode";
    private const string LOADOUT_CARD_ADDRESSABLE = "UI/LoadoutCard";


    public async Task InitializeAsync(VisualElement root)
    {
        utilityRowContainer = UtilityUIBinding.QRequired<VisualElement>(root, "UtilityRowContainer");
        towerRowContainer = UtilityUIBinding.QRequired<VisualElement>(root, "TowerRowContainer");
        unitRowContainer = UtilityUIBinding.QRequired<VisualElement>(root, "UnitRowContainer");
        loadoutCardContainer = UtilityUIBinding.QRequired<VisualElement>(root, "LoadoutCardContainer");

        currentLoadoutHeading = UtilityUIBinding.QRequired<Label>(root, "Label_CurrentLoadout");

        loadoutSelectNode = await Addressables.LoadAssetAsync<VisualTreeAsset>(LOADOUT_SELECT_NODE_ADDRESSABLE).Task;
        loadoutCard = await Addressables.LoadAssetAsync<VisualTreeAsset>(LOADOUT_CARD_ADDRESSABLE).Task;

        if(loadoutSelectNode == null)
        {
            throw new InvalidOperationException($"Failed to load {LOADOUT_SELECT_NODE_ADDRESSABLE}.");
        }
    }

    public void RenderLoadouts(List<LoadoutSlotViewModel> loadouts)
    {
        ClearContainer(utilityRowContainer);
        ClearContainer(towerRowContainer);
        ClearContainer(unitRowContainer );

        foreach (var loadout in loadouts)
        {
            var visualNode = new LoadoutSlotElement(loadoutSelectNode);

            visualNode.Bind(loadout);
            if (loadout.Type == DefinitionCategory.Utility)
            {
                utilityRowContainer.Add(visualNode.Root);
            }
            else if (loadout.Type == DefinitionCategory.Tower)
            {
                towerRowContainer.Add(visualNode.Root);
            }
            else if (loadout.Type == DefinitionCategory.Unit)
            {

                unitRowContainer.Add(visualNode.Root);
            }
        }
    }

    public void RenderLoadoutCards(List<LoadoutCardViewModel> loadoutCards)
    {
        ClearContainer(loadoutCardContainer);

        foreach (var loadoutCard in loadoutCards)
        {
            var visualNode = new LoadoutCardElement(this.loadoutCard);

            visualNode.Bind(loadoutCard);
            loadoutCardContainer.Add(visualNode.Root);
        }
    }

    public void SetLoadoutHeading(string heading)
    {
        currentLoadoutHeading.text = heading;
    }

    private void ClearContainer(VisualElement container)
    {
        container.Clear();
    }
}