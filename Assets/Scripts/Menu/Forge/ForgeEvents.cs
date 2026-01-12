using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ForgeEvents : IUIScreenEvents
{
    private ForgeManager _manager;
    private ForgeView _view;

    private readonly Dictionary<string, string> bindings = new()
    {
        { "Btn_Return", nameof(Btn_ReturnCLicked) },
        { "Btn_Upg_Fighter", nameof(Btn_Upg_FighterClicked) },
        { "Btn_Upg_Ranger", nameof(Btn_Upg_RangerClicked) },
        { "Btn_Upg_Cavalier", nameof(Btn_Upg_CavalierClicked) },
        { "Btn_Upg_Income", nameof(Btn_Upg_IncomeClicked) },
    };

    public void BindEvents(VisualElement root, IUIScreenManager manager = null, IUIScreenView view = null)
    {
        _manager = manager as ForgeManager;

        _view = view as ForgeView;

        _manager.SetTalentTreeView(_view.GetTalentTreeView());

        _manager.Initialize(root);

        UtilityUIBinding.BindEvents(root, this, bindings);
    }

    public void Cleanup()
    {
        UtilityUIBinding.Cleanup(this);
    }

    private void Btn_ReturnCLicked()
    {
        _manager.BackToForge();
    }

    private void Btn_Upg_FighterClicked()
    {
        _manager.OpenTalentTree("fighter");
    }
    private void Btn_Upg_RangerClicked()
    {
        _manager.OpenTalentTree("ranger");
    }

    private void Btn_Upg_CavalierClicked()
    {
        _manager.OpenTalentTree("cavalier");
    }
    private void Btn_Upg_IncomeClicked()
    {
        Debug.Log("Clicked " + nameof(Btn_Upg_IncomeClicked));
    }
}