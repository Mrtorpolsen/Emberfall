using System;
using UnityEngine.UIElements;

public class ResearchCategoryNodeElement : VisualElement, IUnbindable
{
    private ResearchCategory category;

    private VisualElement inactiveContainer;
    private VisualElement activeContainer;

    private Label labelCategoryName;
    private Label labelResearchName;
    private Label labelResearchRank;
    private Label labelResearchTimeLeft;

    private Button button;

    public event Action<ResearchCategory> OnClick;
    private Action clickHandler;

    public ResearchCategoryNodeElement(VisualTreeAsset researchCategoryNode)
    {
        var visualNode = researchCategoryNode.CloneTree();
        this.Add(visualNode);

        inactiveContainer = UtilityUIBinding.QRequired<VisualElement>(visualNode, "InactiveContainer");
        activeContainer = UtilityUIBinding.QRequired<VisualElement>(visualNode, "ActiveContainer");

        labelCategoryName = UtilityUIBinding.QRequired<Label>(visualNode, "Label_CategoryName");
        labelResearchName = UtilityUIBinding.QRequired<Label>(visualNode, "Label_ResearchName");
        labelResearchRank = UtilityUIBinding.QRequired<Label>(visualNode, "Label_ResearchRank");
        labelResearchTimeLeft = UtilityUIBinding.QRequired<Label>(visualNode, "Label_ResearchTimeLeft");

        button = UtilityUIBinding.QRequired<Button>(visualNode, "Button_CategoryContainer");
    }

    public void Bind(ResearchCategory category)
    {
        Unbind();

        this.category = category;

        ActiveResearch activeResearch = ResearchService.Instance.IsActiveCategory(category);

        if (activeResearch != null)
        {
            SetActiveState(activeResearch);
        }
        else
        {
            SetInactiveState();
        }

        labelCategoryName.text = category.ToString();

        clickHandler = () => OnClick?.Invoke(category);
        button.clicked += clickHandler;

        if (ResearchService.Instance != null)
        {
            ResearchService.Instance.OnResearchCompleted -= HandleCompleted;
            ResearchService.Instance.OnResearchCompleted += HandleCompleted;

            ResearchService.Instance.OnResearchTimeUpdated -= HandleTimeUpdated;
            ResearchService.Instance.OnResearchTimeUpdated += HandleTimeUpdated;
        }
    }

    private void HandleCompleted(ResearchCategory cat)
    {
        if (cat != category) return;
        SetInactiveState();
    }

    private void HandleTimeUpdated(ResearchCategory cat, long seconds)
    {
        if (cat != category) return;
        labelResearchTimeLeft.text = TimeFormatter.FormatDaysTime(seconds);
    }

    private void SetActiveState(ActiveResearch activeResearch)
    {
        inactiveContainer.style.display = DisplayStyle.None;
        activeContainer.style.display = DisplayStyle.Flex;

        labelResearchName.text = activeResearch.ResearchName;
        labelResearchRank.text = $"Level {activeResearch.TargetLevel.ToString()}";
    }

    private void SetInactiveState()
    {
        inactiveContainer.style.display = DisplayStyle.Flex;
        activeContainer.style.display = DisplayStyle.None;
    }

    public void Unbind()
    {
        if (clickHandler != null)
            button.clicked -= clickHandler;

        OnClick = null;

        ResearchService.Instance.OnResearchCompleted -= HandleCompleted;
        ResearchService.Instance.OnResearchTimeUpdated -= HandleTimeUpdated;
    }
}
