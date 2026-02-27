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

        talentNodeContainer = root.Q<VisualElement>(TALENT_NODE_CONTAINER);
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

            var labelCost = visualNode.Q<Label>("Label_Cost");
            var imgTalent = visualNode.Q<VisualElement>("Img_Talent");
            var labelUnlocked = visualNode.Q<Label>("Label_Unlocked");

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

            BindClick(visualNode, node.onClick);

            talentNodeContainer
                .Q<VisualElement>($"Row_T{node.tier}")
                .Add(visualNode);
        }
    }

    private void BindClick(VisualElement element, Action handler)
    {
        EventCallback<ClickEvent> callback = _ => handler?.Invoke();
        element.RegisterCallback(callback);
        clickHandlers.Add((element, callback));
    }

    public void CleanupClicks()
    {
        foreach (var (element, handler) in clickHandlers)
        {
            element.UnregisterCallback(handler);
        }
        clickHandlers.Clear();
    }

    public void ClearTalentRows()
    {
        CleanupClicks();
        talentNodeContainer.Query<VisualElement>(className: "talentTreeRow")
            .ForEach(row => row.Clear());
    }

}
