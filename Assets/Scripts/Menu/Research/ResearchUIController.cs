using System.Collections.Generic;
using UnityEngine;

public class ResearchUIController : IUIScreenController
{
    private ResearchView view;

    public void Initialize(IUIScreenView screenView)
    {
        if (screenView is not ResearchView researchView)
        {
            Debug.LogError("ResearchUIController received the wrong view type");
            return;
        }

        view = researchView;
        view.OnCategorySelected += HandleCategorySelected;
        ResearchService.Instance.OnResearchCompleted += HandleResearchUpdate;

        ToCategories();
    }

    public void ToCategories()
    {
        view.RenderResearchCategories();
        view.ShowCategoryPanel();
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

        int targetLevel = currentLevel + 1;

        node.name = research.Name;
        node.researchLevelCurrent = currentLevel;
        node.researchLevelNext = targetLevel;
        node.maxLevel = research.MaxLevel;
        node.description = research.Description;
        node.researchTime = TimeFormatter.FormatCondensedTime(ResearchRules.GetDurationSeconds(research, targetLevel));
        node.category = research.Category;
        node.cost = ResearchRules.GetCost(research, targetLevel);

        // Checks before paying, so a refused start never costs Cinders
        node.onClick = () => ResearchService.Instance.TryStartResearchAsync(research.Id);

        return node;
    }

    private void HandleCategorySelected(ResearchCategory category)
    {
        view.RenderResearchList(GenerateResearchNodes(category));
        view.researchHeading.text = $"{category} Upgrades";
        view.ShowResearchPanel();
    }

    private void HandleResearchUpdate(ResearchCategory category)
    {
        view.RenderResearchList(GenerateResearchNodes(category));
    }

    public void Cleanup()
    {
        if (view != null)
        {
            view.OnCategorySelected -= HandleCategorySelected;
            view.Cleanup();
            view = null;
        }

        if (ResearchService.Instance != null)
            ResearchService.Instance.OnResearchCompleted -= HandleResearchUpdate;

    }
}
