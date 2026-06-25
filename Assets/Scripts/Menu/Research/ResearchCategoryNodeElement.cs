using System;
using UnityEngine;
using UnityEngine.UIElements;

public class ResearchCategoryNodeElement : IUnbindable
{
    private ResearchCategory category;

    private VisualElement inactiveContainer;
    private VisualElement activeContainer;

    private Label labelCategoryName;
    private Label labelResearchName;
    private Label labelResearchRank;
    private Label labelResearchTimeLeft;

    private Button button;

    private VisualElement progressOverlay;
    private VisualElement progressFill;

    private IVisualElementScheduledItem progressTask;

    public event Action<ResearchCategory> OnClick;
    private Action clickHandler;

    private ActiveResearch activeResearch;

    public VisualElement Root { get; }

    public ResearchCategoryNodeElement(VisualTreeAsset researchCategoryNode)
    {
        Root = researchCategoryNode.CloneTree();

        inactiveContainer = UtilityUIBinding.QRequired<VisualElement>(Root, "InactiveContainer");
        activeContainer = UtilityUIBinding.QRequired<VisualElement>(Root, "ActiveContainer");

        labelCategoryName = UtilityUIBinding.QRequired<Label>(Root, "Label_CategoryName");
        labelResearchName = UtilityUIBinding.QRequired<Label>(Root, "Label_ResearchName");
        labelResearchRank = UtilityUIBinding.QRequired<Label>(Root, "Label_ResearchRank");
        labelResearchTimeLeft = UtilityUIBinding.QRequired<Label>(Root, "Label_ResearchTimeLeft");

        button = UtilityUIBinding.QRequired<Button>(Root, "Button_CategoryContainer");

        progressOverlay = UtilityUIBinding.QRequired<VisualElement>(Root, "ProgressOverlay");
        progressFill = UtilityUIBinding.QRequired<VisualElement>(Root, "ProgressFill");
    }

    public void Bind(ResearchCategory category)
    {
        Unbind();

        this.category = category;

        ActiveResearch active = ResearchService.Instance.IsActiveCategory(category);

        if (active != null)
        {
            SetActiveState(active);
        }
        else
        {
            SetInactiveState();
        }

        labelCategoryName.text = category.ToString();

        if (category == ResearchCategory.GlobalAbility)
        {
            labelCategoryName.text = "Global Ability";
        }

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
        StopProgressAnimation();
        SetInactiveState();
    }

    private void HandleTimeUpdated(ResearchCategory cat, long seconds)
    {
        if (cat != category) return;
        labelResearchTimeLeft.text = TimeFormatter.FormatDaysTime(seconds);
    }

    private void SetActiveState(ActiveResearch activeResearch)
    {
        StopProgressAnimation();

        this.activeResearch = activeResearch;

        inactiveContainer.style.display = DisplayStyle.None;
        activeContainer.style.display = DisplayStyle.Flex;
        progressOverlay.style.display = DisplayStyle.Flex;

        long remaining = ResearchService.Instance.GetRemainingSeconds(activeResearch);

        labelResearchName.text = activeResearch.ResearchName;
        labelResearchRank.text = $"Level {activeResearch.TargetLevel.ToString()}";
        labelResearchTimeLeft.text = TimeFormatter.FormatDaysTime(remaining);

        float progress = ResearchService.Instance.GetProgress(activeResearch);
        progressFill.style.width = Length.Percent(progress * 100f);

        progressTask = Root.schedule.Execute(UpdateProgress).Every(32);
    }

    private void SetInactiveState()
    {
        inactiveContainer.style.display = DisplayStyle.Flex;
        activeContainer.style.display = DisplayStyle.None;

        progressOverlay.style.display = DisplayStyle.None;
        progressFill.style.width = Length.Percent(0);
    }

    private void UpdateProgress()
    {
        if (activeResearch == null)
        {
            Debug.LogWarning($"Active is null");
            StopProgressAnimation();
            return;
        }

        float progress = ResearchService.Instance.GetProgress(activeResearch);

        progressFill.style.width = Length.Percent(progress * 100f);
    }

    private void StopProgressAnimation()
    {
        progressTask?.Pause();
        progressTask = null;

        activeResearch = null;

        progressFill.style.width = Length.Percent(0);
    }

    public void Unbind()
    {
        if (clickHandler != null)
            button.clicked -= clickHandler;

        OnClick = null;

        StopProgressAnimation();

        if (ResearchService.Instance != null)
        {
            ResearchService.Instance.OnResearchCompleted -= HandleCompleted;
            ResearchService.Instance.OnResearchTimeUpdated -= HandleTimeUpdated;
        }
    }
}
