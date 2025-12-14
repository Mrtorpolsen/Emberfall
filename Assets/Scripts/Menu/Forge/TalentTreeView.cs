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

        LoadPlaceholderIcon();
        talentNodeContainer = _root.Q<VisualElement>(TALENT_NODE_CONTAINER);
        ClearTalentRows();
    }

    public void Cleanup()
    {
        foreach (var handle in iconHandles.Values)
        {
            Addressables.Release(handle);
        }
        iconHandles.Clear();
    }

    public void RenderTalentTree(List<TalentNodeDefinition> talentNodes)
    {
        ClearTalentRows();

        foreach (var node in talentNodes)
        {
            var visualNode = talentNode.Instantiate();

            var LabelCost = visualNode.Q<Label>("Label_Cost");
            var imgTalent = visualNode.Q<VisualElement>("Img_Talent");
            var labelUnlocked = visualNode.Q<Label>("Label_Unlocked");

            LabelCost.text = node.cost.ToString();
            labelUnlocked.text = node.unlocked;

            LoadTalentIcon(node.img, imgTalent);

            talentNodeContainer
                .Q<VisualElement>($"Row_T{node.tier}")
                .Add(visualNode);
        }
    }

    private void LoadTalentIcon(string address, VisualElement target)
    {
        // Check cache for icon
        if (iconHandles.TryGetValue(address, out var cachedHandle))
        {
            if (cachedHandle.Status == AsyncOperationStatus.Succeeded)
            {
                target.style.backgroundImage = new StyleBackground(cachedHandle.Result);
            }
            else
            {
                target.style.backgroundImage = new StyleBackground(placeholderIcon);
            }
            return;
        }

        // Load new handle and store it
        var handle = Addressables.LoadAssetAsync<Sprite>(address);
        iconHandles[address] = handle;

        handle.Completed += op =>
        {
            if (target.panel == null)
                return;

            if (op.Status == AsyncOperationStatus.Succeeded)
            {
                target.style.backgroundImage = new StyleBackground(op.Result);
            }
            else
            {
                target.style.backgroundImage = new StyleBackground(placeholderIcon);
            }
        };
    }

    private void LoadPlaceholderIcon()
    {
        Addressables.LoadAssetAsync<Sprite>("place_holder_icon")
            .Completed += handle =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    placeholderIcon = handle.Result;
                }
                else
                {
                    Debug.LogError("Failed to load placeholder icon");
                }
            };
    }

    private void ClearTalentRows()
    {
        talentNodeContainer.Query<VisualElement>(className: "talentTreeRow")
            .ForEach(row => row.Clear());
    }

}
