using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ForgeEvents : IUIScreenEvents
{
    private ForgeUIController controller;
    private ForgeView view;

    private readonly Dictionary<string, string> bindings = new()
    {
        { "Btn_Return", nameof(Btn_ReturnCLicked) },
        { "Btn_Refund_Talents", nameof(Btn_Refund_Talents) },
    };

    public void BindEvents(VisualElement root, IUIScreenController controller = null, IUIScreenView view = null)
    {
        this.controller = controller as ForgeUIController;

        this.view = view as ForgeView;

        this.controller.SetTalentTreeView(this.view.GetTalentTreeView());

        UtilityUIBinding.BindEvents(root, this, bindings);
    }

    public void Cleanup()
    {
        UtilityUIBinding.CleanupEvents(this);
    }

    private void Btn_ReturnCLicked()
    {
        controller.BackToForge();
    }

    private async void Btn_Refund_Talents()
    {
        try
        {
            await controller.RefundTalentsAsync();
        }
        catch (Exception e)
        {
            Debug.LogError($"Error refunding talents: {e}");
        }
    }
}