using System;
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

        researchView.RenderResearchCategories(GenerateResearchCategories());
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
        node.researchLevel = $"{currentLevel} -> {currentLevel + 1}";
        node.description = research.Description;
        node.researchTime = TimeFormatter.FormatTimeSeconds(GetValueAtLevel(research.TimeScaling, currentLevel));
        node.cost = GetValueAtLevel(research.CostScaling, currentLevel).ToString();

        node.onClick = () =>
        {
            //TODO purchase logic
            Debug.Log("Yaaay purchasing all over");
        };

        return node;
    }
    //expo
    private float GetValueAtLevel(ResearchScaling scaling, int level)
    {
        return scaling.BaseValue * Mathf.Pow(scaling.MultiplierPerLevel, level - 1);
    }

    private float GetValueAtLevelLinear(ResearchScaling scaling, int level)
    {
        return scaling.BaseValue + scaling.MultiplierPerLevel * (level - 1);
    }

    public void Cleanup()
    {
    }
}
