using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ResearchEvents : IUIScreenEvents
{
    private ResearchUIController controller;
    private ResearchView view;

    private readonly Dictionary<string, string> bindings = new()
    {
        { "Btn_Return", nameof(Btn_ReturnCLicked) },

    };
    public void BindEvents(VisualElement root, IUIScreenController controller = null, IUIScreenView view = null)
    {
        this.controller = controller as ResearchUIController;

        this.view = view as ResearchView;

        UtilityUIBinding.BindEvents(root, this, bindings);
    }

    public void Cleanup()
    {
        UtilityUIBinding.CleanupEvents(this);
    }

    private void Btn_ReturnCLicked()
    {
        controller.ToCategories();
    }
}
