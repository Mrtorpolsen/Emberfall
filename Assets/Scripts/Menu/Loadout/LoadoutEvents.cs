using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class LoadoutEvents : IUIScreenEvents
{
    private LoadoutView view;

    private readonly Dictionary<string, string> bindings = new()
    {
        { "Button_Settings", nameof(Button_SettingsClicked) },
        { "Button_Unit_Tab", nameof(Button_Unit_TabClicked) },
        { "Button_Tower_Tab", nameof(Button_Tower_TabClicked) },
        { "Button_Utility_Tab", nameof(Button_Utility_TabClicked) },
    };

    public void BindEvents(VisualElement root, IUIScreenController controller = null, IUIScreenView view = null)
    {
        this.view = view as LoadoutView;

        UtilityUIBinding.BindEvents(root, this, bindings);
    }

    public void Cleanup()
    {
        UtilityUIBinding.CleanupEvents(this);
    }

    private void Button_SettingsClicked()
    {
        Debug.Log("Settings settings clicked");
    }
    private void Button_Unit_TabClicked()
    {
        view.ShowCardContainer(DefinitionCategory.Unit);
    }

    private void Button_Tower_TabClicked()
    {
        view.ShowCardContainer(DefinitionCategory.Tower);
    }

    private void Button_Utility_TabClicked()
    {
        view.ShowCardContainer(DefinitionCategory.Utility);
    }
}
