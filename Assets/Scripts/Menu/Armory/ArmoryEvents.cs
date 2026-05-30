using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ArmoryEvents : IUIScreenEvents
{
    private ArmoryView view;

    private readonly Dictionary<string, string> bindings = new()
    {
        { "Btn_ForgeNav", nameof(Btn_ForgeNavClicked) },
        { "Btn_LoadoutNav", nameof(Btn_LoadoutNavClicked) },

    };

    public void BindEvents(VisualElement root, IUIScreenController manager = null, IUIScreenView view = null)
    {
        this.view = view as ArmoryView;

        UtilityUIBinding.BindEvents(root, this, bindings);
    }

    public void Cleanup()
    {
        UtilityUIBinding.CleanupEvents(this);
    }

    private async void Btn_ForgeNavClicked()
    {
        await UIScreenRouter.Instance.LoadScreen("Forge");
    }

    private async void Btn_LoadoutNavClicked()
    {
        await UIScreenRouter.Instance.LoadScreen("Loadout");
    }
}