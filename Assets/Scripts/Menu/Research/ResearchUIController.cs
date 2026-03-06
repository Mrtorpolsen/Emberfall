using System.Collections.Generic;
using UnityEngine;

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
        view.OnCategorySelected += HandleCategorySelected;

        ToCategories();
    }

    public void ToCategories()
    {
        view.RenderResearchCategories();
    }

    public List<ResearchNodeDefinition> GenerateResearchNodes(ResearchCategory category)
    {
        var researchNodes = new List<ResearchNodeDefinition>();

        foreach (var research in ResearchService.Instance.playerResearchTree.GetResearchByCategory(category))
        {
            researchNodes.Add(BuildResearchNode(research));
        }

        return researchNodes;
    }

    private ResearchNodeDefinition BuildResearchNode(ResearchDefinition research)
    {
        var node = new ResearchNodeDefinition();

        int currentLevel = ResearchService.Instance.GetCurrentResearchLevel(research.Id);

        node.name = research.Name;
        node.researchLevelCurrent = currentLevel;
        node.researchLevelNext = (currentLevel + 1);
        node.maxLevel = research.MaxLevel;
        node.description = research.Description;
        node.researchTime = TimeFormatter.FormatCondensedTime(research.TimeScaling.GetAmountForNextLevelLinear(currentLevel));
        node.category = research.Category;
        node.cost = research.CostScaling.GetAmountForNextLevelLinear(currentLevel);

        node.onClick = async () =>
        {
            //TODO purchase logic
            await ResearchService.Instance.StartResearch(research.Id);
        };

        return node;
    }

    private void HandleCategorySelected(ResearchCategory category)
    {
        view.RenderResearchList(GenerateResearchNodes(category));
        view.researchHeading.text = $"{category} Upgrades";
    }

    public void Cleanup()
    {
        view.Cleanup();
    }
}
