using System;
using UnityEngine;
using UnityEngine.UIElements;

public class ResearchNodeElement : VisualElement, IUnbindable
{
    private ResearchCategory category;

    private Label labelResearchName;
    private Label labelResearchLevelCurrent;
    private Label labelResearchLevelNext;
    private Label labelResearchDescription;
    private Label labelResearchTime;

    private Button buttonPurchaseResearch;

    private Action clickHandler;

    private int cost;
    private int maxLevel;
    private int currentLevel;

    public ResearchNodeElement(VisualTreeAsset researchNode)
    {
        var visualNode = researchNode.CloneTree();
        this.Add(visualNode);

        labelResearchName = UtilityUIBinding.QRequired<Label>(visualNode, "Label_ResearchName");
        labelResearchLevelCurrent = UtilityUIBinding.QRequired<Label>(visualNode, "Label_ResearchLevelCurrent");
        labelResearchLevelNext = UtilityUIBinding.QRequired<Label>(visualNode, "Label_ResearchLevelNext");
        labelResearchDescription = UtilityUIBinding.QRequired<Label>(visualNode, "Label_ResearchDescription");
        labelResearchTime = UtilityUIBinding.QRequired<Label>(visualNode, "Label_ResearchTime");

        buttonPurchaseResearch = UtilityUIBinding.QRequired<Button>(visualNode, "Button_PurchaseResearch");
    }

    public void Bind(ResearchNodeDefinition node)
    {
        Unbind();

        category = node.category;

        labelResearchName.text = node.name;
        labelResearchLevelCurrent.text = node.researchLevelCurrent.ToString();
        labelResearchLevelNext.text = node.researchLevelNext.ToString();
        labelResearchDescription.text = node.description;
        labelResearchTime.text = node.researchTime;

        cost = node.cost;
        maxLevel = node.maxLevel;
        currentLevel = node.researchLevelCurrent;

        clickHandler = async () =>
        {
            if (node.onClick != null)
            {
                await node.onClick.Invoke();
            }
        };

        buttonPurchaseResearch.clicked += clickHandler;

        buttonPurchaseResearch.text = node.cost.ToString();
        if (maxLevel == currentLevel)
        {
            buttonPurchaseResearch.text = "Maxed";
        }
        else
        {
            Sprite sprite = Resources.Load<Sprite>("UI/cinder_icon");

            if (sprite != null)
            {
                buttonPurchaseResearch.iconImage = sprite.texture;
            }
        }

        buttonPurchaseResearch.SetEnabled(IsButtonEnabled());

        if (ResearchService.Instance != null)
        {
            ResearchService.Instance.OnResearchStarted -= LockResearchPurchase;
            ResearchService.Instance.OnResearchStarted += LockResearchPurchase;

            ResearchService.Instance.OnResearchCompleted -= UnlockResearchPurchase;
            ResearchService.Instance.OnResearchCompleted += UnlockResearchPurchase;
        }

        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnPlayerCurrencyChanged += RefreshButtonState;
        }

        if (ResearchService.Instance.IsActiveCategory(category) != null)
        {
            LockResearchPurchase(category);
        }
        else
        {
            UnlockResearchPurchase(category);
        }
    }

    private bool IsButtonEnabled()
    {
        if (currentLevel == maxLevel || CurrencyManager.Instance.Get(CurrencyTypes.Cinders) <  cost)
        {
            return false;
        }

        return true;
    }

    private void LockResearchPurchase(ResearchCategory cat)
    {
        if (cat != category) return;
        buttonPurchaseResearch.SetEnabled(false);
    }

    private void UnlockResearchPurchase(ResearchCategory cat)
    {
        if (cat != category) return;
        buttonPurchaseResearch.SetEnabled(IsButtonEnabled());
    }

    private void RefreshButtonState(CurrencyTypes type)
    {
        if (type != CurrencyTypes.Cinders)
            return;

        if (ResearchService.Instance.IsActiveCategory(category) != null)
            return;

        buttonPurchaseResearch.SetEnabled(IsButtonEnabled());
    }

    public void Unbind()
    {
        if (clickHandler != null)
            buttonPurchaseResearch.clicked -= clickHandler;

        if (ResearchService.Instance != null)
        {
            ResearchService.Instance.OnResearchStarted -= LockResearchPurchase;
            ResearchService.Instance.OnResearchCompleted -= UnlockResearchPurchase;
        }
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnPlayerCurrencyChanged -= RefreshButtonState;
        }
    }
}
