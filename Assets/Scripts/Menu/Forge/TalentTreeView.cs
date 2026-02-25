using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UIElements;

public class TalentTreeView
{
    public VisualTreeAsset talentNode;

    private VisualElement _root;
    private VisualElement talentNodeContainer;

    private Dictionary<string, AsyncOperationHandle<Sprite>> iconHandles = new();

    private const string TALENT_NODE_CONTAINER = "TalentNodesContainer";

    public async void Initialize(VisualElement root)
    {
        _root = root;
        talentNode = await Addressables.LoadAssetAsync<VisualTreeAsset>("UI/TalentNode").Task;

        if (talentNode == null)
        {
            Debug.LogError("Talent node template not loaded!");
            return;
        }

        talentNodeContainer = _root.Q<VisualElement>(TALENT_NODE_CONTAINER);
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

            var LabelCost = visualNode.Q<Label>("Label_Cost");
            var imgTalent = visualNode.Q<VisualElement>("Img_Talent");
            var labelUnlocked = visualNode.Q<Label>("Label_Unlocked");

            LabelCost.text = node.cost.ToString();
            labelUnlocked.text = node.purchased;

            node.purchasedLabel = labelUnlocked;

            //update the label
            node.UpdatePurchasedText = (current, max) =>
            {
                if (node.purchasedLabel != null)
                    node.purchasedLabel.text = $"{current}/{max}";
            };

            UtilityLoadAdressable.LoadAdressableIcon(node.img, imgTalent);

            visualNode.RegisterCallback<ClickEvent>(_ => node.onClick.Invoke());

            talentNodeContainer
                .Q<VisualElement>($"Row_T{node.tier}")
                .Add(visualNode);
        }
    }

    private void ClearTalentRows()
    {
        talentNodeContainer.Query<VisualElement>(className: "talentTreeRow")
            .ForEach(row => row.Clear());
    }

}
