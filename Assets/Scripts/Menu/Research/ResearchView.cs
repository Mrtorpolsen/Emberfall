using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;

public class ResearchView : IUIScreenView
{
    public VisualElement ResearchCategoryListPanel { get; private set; }
    public VisualElement ResearchListPanel { get; private set; }

    private VisualElement researchCategoryListContainer;
    private VisualElement researchListContainer;

    private VisualTreeAsset researchCategoryNode;

    private const string RESEARCH_CATEGORYNODE_ADDRESSABLE = "UI/CategoryNode";

    public async Task InitializeAsync(VisualElement root)
    {
        ResearchCategoryListPanel = root.Q<VisualElement>("ResearchCategoryListPanel");
        ResearchListPanel = root.Q<VisualElement>("ResearchListPanel");

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

        if (researchCategoryNode == null)
        {
            Debug.LogError("Research category node template not loaded!");
            return;
        }
    }

    public void RenderResearchCategories(List<ResearchCategoryNodeDefinition> categoryNodes)
    {
        researchCategoryListContainer.Clear();

        foreach (var node in categoryNodes)
        {
            //Extract the actual root
            var nodeTemplate = researchCategoryNode.Instantiate();
            VisualElement visualNode = nodeTemplate[0];
            nodeTemplate.RemoveAt(0);

            var labelCategoryName = visualNode.Q<Label>("Label_CategoryName");
            if (labelCategoryName != null)
            {
                labelCategoryName.text = node.categoryName;
            }
            else
            {
                Debug.LogWarning("No labelCategoryName found in category node template!");
            }

            var buttonNode = visualNode.Q<Button>("Button_CategoryContainer");
            if (buttonNode != null)
            {
                buttonNode.clicked += node.onClick;
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
        researchListContainer.Clear();

        foreach (var node in researchNodes)
        {
            var nodeTemplate = researchCategoryNode.Instantiate();
            VisualElement visualNode = nodeTemplate[0];
            nodeTemplate.RemoveAt(0);

            var labelResearchName = visualNode.Q<Label>("Label_ResearchName");
            var labelResearchLevel = visualNode.Q<Label>("Label_ResearchLevel");
            var labelResearchDescription = visualNode.Q<Label>("Label_ResearchDescription");
            var labelResearchTime = visualNode.Q<Label>("Label_ResearchTime");
            var labelResearchCost = visualNode.Q<Label>("Label_ResearchCost");

            labelResearchName.text = node.name;
            labelResearchLevel.text = node.researchLevel;
            labelResearchDescription.text = node.description;
            labelResearchTime.text = node.researchTime;
            labelResearchCost.text = node.cost;

            var buttonPurchaseResearch = visualNode.Q<Button>("Button_PurchaseResearch");
            if (buttonPurchaseResearch != null)
            {
                buttonPurchaseResearch.clicked += node.onClick;
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
}
