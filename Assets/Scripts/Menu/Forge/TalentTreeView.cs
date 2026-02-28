using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UIElements;

public class TalentTreeView
{
    public VisualTreeAsset talentNode;

    private VisualElement talentNodeContainer;

    private const string TALENT_NODE_CONTAINER = "TalentNodesContainer";
    private const string FORGE_TALENTNODE_ADDRESSABLE = "UI/TalentNode";

    private readonly List<(VisualElement element, EventCallback<ClickEvent> handler)> clickHandlers = new();

    public async void Initialize(VisualElement root)
    {
        talentNode = await Addressables.LoadAssetAsync<VisualTreeAsset>(FORGE_TALENTNODE_ADDRESSABLE).Task;

        if (talentNode == null)
        {
            Debug.LogError("Talent node template not loaded!");
            return;
        }
        
        talentNodeContainer = UtilityUIBinding.QRequired<VisualElement>(root, TALENT_NODE_CONTAINER);
        ClearTalentRows();
    }

    public void RenderTalentTree(List<TalentNodeDefinition> talentNodes)
    {
        ClearTalentRows();

        foreach (var node in talentNodes)
        {
            //Extract the actual root
            var nodeTemplate = talentNode.Instantiate();
            VisualElement visualNode = nodeTemplate[0];
            nodeTemplate.RemoveAt(0);
            
            var labelCost = UtilityUIBinding.QRequired<Label>(visualNode, "Label_Cost");
            var imgTalent = UtilityUIBinding.QRequired<VisualElement>(visualNode, "Img_Talent");
            var labelUnlocked = UtilityUIBinding.QRequired<Label>(visualNode, "Label_Unlocked");

            labelCost.text = node.cost.ToString();
            labelUnlocked.text = node.purchased;

            node.purchasedLabel = labelUnlocked;

            //update the label
            node.UpdatePurchasedText = (current, max) =>
            {
                if (node.purchasedLabel != null)
                    node.purchasedLabel.text = $"{current}/{max}";
            };

            UtilityLoadAdressable.LoadAdressableIcon(node.img, imgTalent);

            UtilityUIBinding.BindVEClick(visualNode, node.onClick, clickHandlers);

            talentNodeContainer
                .Q<VisualElement>($"Row_T{node.tier}")
                .Add(visualNode);
        }
    }

    public void ClearTalentRows()
    {
        CleanupClicks();
        talentNodeContainer.Query<VisualElement>(className: "talentTreeRow")
            .ForEach(row => row.Clear());
    }

    public void CleanupClicks()
    {
        UtilityUIBinding.CleanupVEClicks(clickHandlers);
    }
}
