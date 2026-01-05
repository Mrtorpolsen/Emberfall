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
    private Sprite placeholderIcon;

    private Dictionary<string, AsyncOperationHandle<Sprite>> iconHandles = new();

    private const string TALENT_NODE_CONTAINER = "TalentNodesContainer";

    public void Initialize(VisualElement root)
    {
        _root = root;
        talentNode = Resources.Load<VisualTreeAsset>("UI/Forge/TalentNode");

        talentNodeContainer = _root.Q<VisualElement>(TALENT_NODE_CONTAINER);
        ClearTalentRows();
    }

    public void RenderTalentTree(List<TalentNodeDefinition> talentNodes)
    {
        ClearTalentRows();

        foreach (var node in talentNodes)
        {
            //Extract the actuale root
            var nodeTemplate = talentNode.Instantiate();
            VisualElement visualNode = nodeTemplate[0];
            nodeTemplate.RemoveAt(0);

            var LabelCost = visualNode.Q<Label>("Label_Cost");
            var imgTalent = visualNode.Q<VisualElement>("Img_Talent");
            var labelUnlocked = visualNode.Q<Label>("Label_Unlocked");

            LabelCost.text = node.cost.ToString();
            labelUnlocked.text = node.unlocked;

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
