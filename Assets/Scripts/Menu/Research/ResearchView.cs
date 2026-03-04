using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

    private IEnumerable<ResearchCategory> categories;

    public event Action<ResearchCategory> OnCategorySelected;

    public async Task InitializeAsync(VisualElement root)
    {
        ResearchCategoryListPanel = UtilityUIBinding.QRequired<VisualElement>(root, "ResearchCategoryListPanel");
        ResearchListPanel = UtilityUIBinding.QRequired<VisualElement>(root, "ResearchListPanel");
        researchHeading = UtilityUIBinding.QRequired<Label>(root, "Label_ResearchHeading");

        categories = ResearchService.Instance.playerResearchTree.GetCategories();
        if (categories == null)
        {
            throw new InvalidOperationException("Failed to load categories.");
        }
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

    public void RenderResearchCategories()
    {
        ClearPanel(researchCategoryListContainer);

        foreach (ResearchCategory category in categories)
        {
            var visualNode = new ResearchCategoryNodeElement(researchCategoryNode);
            visualNode.Bind(category);

            visualNode.OnClick += category =>
            {
                OnCategorySelected?.Invoke(category);
            };

            researchCategoryListContainer.Add(visualNode);
        }
        ShowCategoryList();
    }

    public void RenderResearchList(List<ResearchNodeDefinition> researchNodes)
    {
        ClearPanel(researchListContainer);

        foreach (var node in researchNodes)
        {
            var visualNode = new ResearchNodeElement(researchNode);
            visualNode.Bind(node);

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
        Cleanup();
        panel.Clear();
    }

    public void Cleanup()
    {
        CleanupUnbindables(researchCategoryListContainer);
        CleanupUnbindables(researchListContainer);
    }

    private void CleanupUnbindables(VisualElement container)
    {
        foreach (var element in container.Children().OfType<IUnbindable>())
        {
            element.Unbind();
        }
    }
}
