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
        { "Btn_Upg_Fighter", nameof(Btn_Upg_FighterClicked) },
        { "Btn_Upg_Ranger", nameof(Btn_Upg_RangerClicked) },
        { "Btn_Upg_Cavalier", nameof(Btn_Upg_CavalierClicked) },
        { "Btn_Upg_Income", nameof(Btn_Upg_IncomeClicked) },
        { "Btn_Refund_Talents", nameof(Btn_Refund_Talents) },
    };

    public void BindEvents(VisualElement root, IUIScreenController controller = null, IUIScreenView view = null)
    {
        this.controller = controller as ForgeUIController;

        this.view = view as ForgeView;

        this.controller.SetTalentTreeView(this.view.GetTalentTreeView());

        this.controller.Initialize(root);

        UtilityUIBinding.BindEvents(root, this, bindings);
    }

    public void Cleanup()
    {
        UtilityUIBinding.Cleanup(this);
    }

    private void Btn_ReturnCLicked()
    {
        controller.BackToForge();
    }

    private void Btn_Upg_FighterClicked()
    {
        controller.OpenTalentTree("fighter");
    }
    private void Btn_Upg_RangerClicked()
    {
        controller.OpenTalentTree("ranger");
    }

    private void Btn_Upg_CavalierClicked()
    {
        controller.OpenTalentTree("cavalier");
    }
    private void Btn_Upg_IncomeClicked()
    {
        Debug.Log("Clicked " + nameof(Btn_Upg_IncomeClicked));
    }

    private void Btn_Refund_Talents()
    {
        controller.RefundTalents();
    }
}