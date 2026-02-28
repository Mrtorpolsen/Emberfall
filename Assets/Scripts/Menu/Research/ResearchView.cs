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
        ResearchCategoryListPanel = UtilityUIBinding.QRequired<VisualElement>(root, "ResearchCategoryListPanel");
        ResearchListPanel = UtilityUIBinding.QRequired<VisualElement>(root, "ResearchListPanel");
        researchHeading = UtilityUIBinding.QRequired<Label>(root, "Label_ResearchHeading");

        ShowCategoryList();

        researchCategoryListContainer = UtilityUIBinding.QRequired<VisualElement>(root, "ResearchCategoryListContainer");
        researchListContainer = UtilityUIBinding.QRequired<VisualElement>(root, "ResearchListContainer");

        researchCategoryNode = await Addressables.LoadAssetAsync<VisualTreeAsset>(RESEARCH_CATEGORYNODE_ADDRESSABLE).Task;
        researchNode = await Addressables.LoadAssetAsync<VisualTreeAsset>(RESEARCH_RESEARCHNODE_ADDRESSABLE).Task;

        if (researchCategoryNode == null)
        {
            throw new InvalidOperationException("Failed to load UI/CategoryNode.");
        }
        if (researchNode == null)
        {
            throw new InvalidOperationException("Failed to load UI/CategoryNode.");
        }
    }

    public void RenderResearchCategories(List<ResearchCategoryNodeDefinition> categoryNodes)
    {
        ClearPanel(researchCategoryListContainer);

        foreach (var node in categoryNodes)
        {
            VisualElement visualNode = UtilityUIBinding.InstantiateRoot(researchCategoryNode);

            var inactiveContainer = UtilityUIBinding.QRequired<VisualElement>(visualNode, "InactiveContainer");
            var activeContainer = UtilityUIBinding.QRequired<VisualElement>(visualNode, "ActiveContainer");

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

                var labelResearchName = UtilityUIBinding.QRequired<Label>(visualNode, "Label_ResearchName");
                var labelResearchRank = UtilityUIBinding.QRequired<Label>(visualNode, "Label_ResearchRank");
                var labelResearchTimeLeft = UtilityUIBinding.QRequired<Label>(visualNode, "Label_ResearchTimeLeft");

                labelResearchName.text = node.researchName;
                labelResearchRank.text = $"Researching rank: {node.researchRank}";
                labelResearchTimeLeft.text = node.researchTimeLeft;
            }

            var buttonNode = UtilityUIBinding.QRequired<Button>(visualNode, "Button_CategoryContainer");

            UtilityUIBinding.BindButtonClick(buttonNode, node.onClick, boundButtons);

            researchCategoryListContainer.Add(visualNode);
        }
        ShowCategoryList();
    }

    public void RenderResearchList(List<ResearchNodeDefinition> researchNodes)
    {
        ClearPanel(researchListContainer);

        foreach (var node in researchNodes)
        {
            VisualElement visualNode = UtilityUIBinding.InstantiateRoot(researchNode);

            var labelResearchName = UtilityUIBinding.QRequired<Label>(visualNode, "Label_ResearchName");
            var labelResearchLevelCurrent = UtilityUIBinding.QRequired<Label>(visualNode, "Label_ResearchLevelCurrent");
            var labelResearchLevelNext = UtilityUIBinding.QRequired<Label>(visualNode, "Label_ResearchLevelNext");
            var labelResearchDescription = UtilityUIBinding.QRequired<Label>(visualNode, "Label_ResearchDescription");
            var labelResearchTime = UtilityUIBinding.QRequired<Label>(visualNode, "Label_ResearchTime");

            labelResearchName.text = node.name;
            labelResearchLevelCurrent.text = node.researchLevelCurrent;
            labelResearchLevelNext.text = node.researchLevelNext;
            labelResearchDescription.text = node.description;
            labelResearchTime.text = node.researchTime;

            var buttonPurchaseResearch = UtilityUIBinding.QRequired<Button>(visualNode, "Button_PurchaseResearch");

            buttonPurchaseResearch.text = node.cost;

            Sprite sprite = Resources.Load<Sprite>("UI/cinder_icon");

            if (sprite != null)
            {
                buttonPurchaseResearch.iconImage = sprite.texture;
            }

            UtilityUIBinding.BindButtonClick(buttonPurchaseResearch, node.onClick, boundButtons);

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
