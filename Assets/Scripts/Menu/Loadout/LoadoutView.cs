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

    private Label currentLoadoutHeading;

    private VisualTreeAsset loadoutNode;

    private const string LOADOUT_NODE_ADDRESSABLE = "UI/LoadoutNode";

    public async Task InitializeAsync(VisualElement root)
    {
        utilityRowContainer = UtilityUIBinding.QRequired<VisualElement>(root, "UtilityRowContainer");
        towerRowContainer = UtilityUIBinding.QRequired<VisualElement>(root, "TowerRowContainer");
        unitRowContainer = UtilityUIBinding.QRequired<VisualElement>(root, "UnitRowContainer");

        currentLoadoutHeading = UtilityUIBinding.QRequired<Label>(root, "Label_CurrentLoadout");

        loadoutNode = await Addressables.LoadAssetAsync<VisualTreeAsset>(LOADOUT_NODE_ADDRESSABLE).Task;

        if(loadoutNode == null)
        {
            throw new InvalidOperationException($"Failed to load {LOADOUT_NODE_ADDRESSABLE}.");
        }
    }

    public void RenderLoadouts(List<LoadoutSlotViewModel> loadouts)
    {
        ClearContainer(utilityRowContainer);
        ClearContainer(towerRowContainer);
        ClearContainer(unitRowContainer );

        foreach (var loadout in loadouts)
        {
            var visualNode = new LoadoutSlotElement(loadoutNode);

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

    public void SetLoadoutHeading(string heading)
    {
        currentLoadoutHeading.text = heading;
    }

    private void ClearContainer(VisualElement container)
    {
        container.Clear();
    }
}