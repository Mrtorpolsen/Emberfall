using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;

public class ResearchView : IUIScreenView
{
    public VisualElement ResearchCategoryListPanel { get; private set; }
    public VisualElement ResearchListPanel { get; private set; }

    private VisualElement researchCategoryListContainer;
    private VisualElement researchListContainer;

    public Label researchHeading;

    private VisualTreeAsset researchCategoryNode;
    private VisualTreeAsset researchNode;

    private const string RESEARCH_CATEGORYNODE_ADDRESSABLE = "UI/CategoryNode";
    private const string RESEARCH_RESEARCHNODE_ADDRESSABLE = "UI/ResearchNode";

    private readonly List<(Button button, Action handler)> boundButtons = new();

    public async Task InitializeAsync(VisualElement root)
    {
        ResearchCategoryListPanel = root.Q<VisualElement>("ResearchCategoryListPanel");
        ResearchListPanel = root.Q<VisualElement>("ResearchListPanel");
        researchHeading = root.Q<Label>("Label_ResearchHeading");

        if (ResearchCategoryListPanel == null)
        {
            Debug.LogWarning("ResearchCategoryListPanel not found");
            return;
        }
        if (ResearchListPanel == null)
        {
            Debug.LogWarning("ResearchListPanel not found");
            return;
        }

        ShowCategoryList();

        researchCategoryListContainer = root.Q<VisualElement>("ResearchCategoryListContainer");
        //HERE
        researchListContainer = root.Q<VisualElement>("ResearchListContainer");

        researchCategoryNode = await Addressables.LoadAssetAsync<VisualTreeAsset>(RESEARCH_CATEGORYNODE_ADDRESSABLE).Task;
        researchNode = await Addressables.LoadAssetAsync<VisualTreeAsset>(RESEARCH_RESEARCHNODE_ADDRESSABLE).Task;

        if (researchCategoryNode == null)
        {
            Debug.LogError("Research category node template not loaded!");
            return;
        }
    }

    public void RenderResearchCategories(List<ResearchCategoryNodeDefinition> categoryNodes)
    {
        ClearPanel(researchCategoryListContainer);

        foreach (var node in categoryNodes)
        {
            //Extract the actual root
            var nodeTemplate = researchCategoryNode.Instantiate();
            VisualElement visualNode = nodeTemplate[0];
            nodeTemplate.RemoveAt(0);

            var inactiveContainer = visualNode.Q<VisualElement>("InactiveContainer");
            var activeContainer = visualNode.Q<VisualElement>("ActiveContainer");

            if (inactiveContainer == null)
            {
                Debug.LogWarning("No inactiveContainer found in category node template!");
                return;
            }
            if (activeContainer == null)
            {
                Debug.LogWarning("No activeContainer found in category node template!");
                return;
            }

            if (!node.isResearchActive)
            {
                inactiveContainer.style.display = DisplayStyle.Flex;
                activeContainer.style.display = DisplayStyle.None;

                var labelCategoryName = visualNode.Q<Label>("Label_CategoryName");
                if (labelCategoryName != null)
                {
                    labelCategoryName.text = node.categoryName;
                }
                else
                {
                    Debug.LogWarning("No labelCategoryName found in category node template!");
                }
            }
            else
            {
                inactiveContainer.style.display = DisplayStyle.None;
                activeContainer.style.display = DisplayStyle.Flex;

                var labelResearchName = visualNode.Q<Label>("Label_ResearchName");
                var labelResearchRank = visualNode.Q<Label>("Label_ResearchRank");
                var labelResearchTimeLeft = visualNode.Q<Label>("Label_ResearchTimeLeft");

                if (labelResearchName != null)

                {
                    labelResearchName.text = node.researchName;
                }
                else
                {
                    Debug.LogWarning("No labelResearchName found in category node template!");
                }

                if (labelResearchRank != null)

                {
                    labelResearchRank.text = node.researchRank;
                }
                else
                {
                    Debug.LogWarning("No labelResearchRank found in category node template!");
                }

                if (labelResearchTimeLeft != null)

                {
                    labelResearchTimeLeft.text = node.researchTimeLeft;
                }
                else
                {
                    Debug.LogWarning("No labelResearchTimeLeft found in category node template!");
                }
            }

            var buttonNode = visualNode.Q<Button>("Button_CategoryContainer");
            if (buttonNode != null)
            {
                UtilityUIBinding.BindButtonClick(buttonNode, node.onClick, boundButtons);
            }
            else
            {
                Debug.LogWarning("No Button found in category node template!");
            }

            researchCategoryListContainer.Add(visualNode);
        }
        ShowCategoryList();
    }

    public void RenderResearchList(List<ResearchNodeDefinition> researchNodes)
    {
        ClearPanel(researchListContainer);

        foreach (var node in researchNodes)
        {
            var nodeTemplate = researchNode.Instantiate();
            VisualElement visualNode = nodeTemplate[0];
            nodeTemplate.RemoveAt(0);

            var labelResearchName = visualNode.Q<Label>("Label_ResearchName");
            var labelResearchLevelCurrent = visualNode.Q<Label>("Label_ResearchLevelCurrent");
            var labelResearchLevelNext = visualNode.Q<Label>("Label_ResearchLevelNext");
            var labelResearchDescription = visualNode.Q<Label>("Label_ResearchDescription");
            var labelResearchTime = visualNode.Q<Label>("Label_ResearchTime");

            labelResearchName.text = node.name;
            labelResearchLevelCurrent.text = node.researchLevelCurrent;
            labelResearchLevelNext.text = node.researchLevelNext;
            labelResearchDescription.text = node.description;
            labelResearchTime.text = node.researchTime;

            var buttonPurchaseResearch = visualNode.Q<Button>("Button_PurchaseResearch");
            if (buttonPurchaseResearch != null)
            {
                buttonPurchaseResearch.text = node.cost;

                Sprite sprite = Resources.Load<Sprite>("UI/cinder_icon");

                if (sprite != null)
                {
                    buttonPurchaseResearch.iconImage = sprite.texture;
                }

                UtilityUIBinding.BindButtonClick(buttonPurchaseResearch, node.onClick, boundButtons);
            }
            else
            {
                Debug.LogWarning("No Button found in category node template!");
            }

            researchListContainer.Add(visualNode);
        }
        ShowResearchPanel();
    }
    
    public void ShowCategoryList()
    {
        ResearchListPanel.style.display = DisplayStyle.None;
        ResearchCategoryListPanel.style.display = DisplayStyle.Flex;
    }

    public void ShowResearchPanel()
    {
        ResearchListPanel.style.display = DisplayStyle.Flex;
        ResearchCategoryListPanel.style.display = DisplayStyle.None;
    }

    private void ClearPanel(VisualElement panel)
    {
        CleanupButtons();
        panel.Clear();
    }

    public void CleanupButtons()
    {
        UtilityUIBinding.CleanupButtonClicks(boundButtons);
    }
}
