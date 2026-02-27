using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class ResearchUIController : IUIScreenController
{
    private ResearchView view;

    public void Initialize(IUIScreenView screenView)
    {
        if (screenView is not ResearchView researchView)
        {
            Debug.LogError("ResearchUIController recieved the wrong view type");
            return;
        }

        view = researchView;

        ToCategories();
    }

    public void ToCategories()
    {
        view.RenderResearchCategories(GenerateResearchCategories());
    }

    public List<ResearchCategoryNodeDefinition> GenerateResearchCategories()
    {
        var categoryNodes = new List<ResearchCategoryNodeDefinition>();

        foreach (var category in ResearchService.Instance.playerResearchTree.GetCategories())
        {
            categoryNodes.Add(BuildResearchCategory(category));
        }

        return categoryNodes;
    }

    private ResearchCategoryNodeDefinition BuildResearchCategory(string categoryName)
    {
        var node = new ResearchCategoryNodeDefinition();

        node.categoryName = categoryName.FirstCharacterToUpper();
        node.onClick = () =>
        {
            view.RenderResearchList(GenerateResearchNodes(categoryName));
            view.researchHeading.text = $"{categoryName} Upgrades";
        };
        
        return node;
    }

    public List<ResearchNodeDefinition> GenerateResearchNodes(string categoryName)
    {
        var researchNodes = new List<ResearchNodeDefinition>();

        foreach (var research in ResearchService.Instance.playerResearchTree.GetResearchByCategory(categoryName))
        {
            researchNodes.Add(BuildResearchNode(research));
        }

        return researchNodes;
    }

    private ResearchNodeDefinition BuildResearchNode(ResearchDefinition research)
    {
        var node = new ResearchNodeDefinition();

        int currentLevel = 0;

        node.name = research.Name;
        node.researchLevelCurrent = currentLevel.ToString();
        node.researchLevelNext = (currentLevel + 1).ToString();
        node.description = research.Description;
        node.researchTime = TimeFormatter.FormatTimeSeconds(GetCostForNextLevelLinear(research.TimeScaling, currentLevel));
        node.cost = GetCostForNextLevelLinear(research.CostScaling, currentLevel).ToString();

        node.onClick = () =>
        {
            //TODO purchase logic
            Debug.Log("Yaaay purchasing all over");
        };

        return node;
    }

    //expo
    private int GetCostForNextLevel(ResearchScaling scaling, int level)
    {
        float value = scaling.BaseValue * Mathf.Pow(scaling.MultiplierPerLevel, level);
        return Mathf.RoundToInt(value);
    }

    private int GetCostForNextLevelLinear(ResearchScaling scaling, int level)
    {
        float value = scaling.BaseValue + scaling.MultiplierPerLevel * (level);
        return Mathf.RoundToInt(value);
    }

    public void Cleanup()
    {
        view.CleanupButtons();
    }
}
