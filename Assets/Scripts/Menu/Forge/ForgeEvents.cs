using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ForgeEvents : IUIScreenEvents
{
    private ForgeManager _manager;

    private readonly Dictionary<string, string> bindings = new()
    {
        { "Btn_Return", nameof(Btn_ReturnCLicked) },
        { "Btn_Upg_Fighter", nameof(Btn_Upg_FighterClicked) },
        { "Btn_Upg_Ranger", nameof(Btn_Upg_RangerClicked) },
        { "Btn_Upg_Cavalier", nameof(Btn_Upg_CavalierClicked) },
        { "Btn_Upg_Income", nameof(Btn_Upg_IncomeClicked) },
    };

    public void BindEvents(VisualElement root, IUIScreenManager manager = null)
    {
        _manager = manager as ForgeManager;
        _manager.Initialize(root);

        UtilityUIBinding.BindEvents(root, this, bindings);
    }

    public void Cleanup()
    {
        UtilityUIBinding.Cleanup(this);
    }

    private void Btn_ReturnCLicked()
    {
        Debug.Log("Clicked " + nameof(Btn_ReturnCLicked));
        _manager.BackToForge();
    }

    private void Btn_Upg_FighterClicked()
    {
        Debug.Log("Clicked " + nameof(Btn_Upg_FighterClicked));
        _manager.OpenTalentTree();
    }

    private void Btn_Upg_IncomeClicked()
    {
        Debug.Log("Clicked " + nameof(Btn_Upg_IncomeClicked));
    }

    private void Btn_Upg_CavalierClicked()
    {
        Debug.Log("Clicked " + nameof(Btn_Upg_CavalierClicked));
    }

    private void Btn_Upg_RangerClicked()
    {
        Debug.Log("Clicked " + nameof(Btn_Upg_RangerClicked));
    }
}