using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class LoadoutEvents : IUIScreenEvents
{
    private LoadoutView view;

    private readonly Dictionary<string, string> bindings = new()
    {
        { "Btn_Settings", nameof(Btn_SettingsClicked) },
        { LoadoutView.LOADOUT_BTN_UNIT_TAB, nameof(Btn_Unit_TabClicked) },
        { LoadoutView.LOADOUT_BTN_TOWER_TAB, nameof(Btn_Tower_TabClicked) },
        { LoadoutView.LOADOUT_BTN_UTILITY_TAB, nameof(Btn_Utility_TabClicked) },
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

    private void Btn_SettingsClicked()
    {
        Debug.Log("Settings settings clicked");
    }
    private void Btn_Unit_TabClicked()
    {
        view.ShowCardContainer(DefinitionCategory.Unit);
    }

    private void Btn_Tower_TabClicked()
    {
        view.ShowCardContainer(DefinitionCategory.Tower);
    }

    private void Btn_Utility_TabClicked()
    {
        view.ShowCardContainer(DefinitionCategory.Utility);
    }
}
