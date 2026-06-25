using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

public class NavBarEvents : MonoBehaviour
{
    [Header("References")]
    public UIDocument uIDocument;

    private VisualElement root;

    private readonly Dictionary<string, string> bindings = new()
    {
        { "Btn_Main", nameof(Btn_Main) },
        { "Btn_Leaderboard", nameof(Btn_Leaderboard) },
        { "Btn_Armory", nameof(Btn_Armory) },
        { "Btn_Research", nameof(Btn_Research) },
    };

    void Awake()
    {
        root = UtilityUIBinding.QRequired<VisualElement>(uIDocument.rootVisualElement, "NavMenuContainer");

        UtilityUIBinding.BindEvents(root, this, bindings);
    }

    void OnDestroy()
    {
        UtilityUIBinding.CleanupEvents(this);
    }

    private async void Btn_Main()
    {
        await UIScreenRouter.Instance.LoadScreen("MainMenu");
    }
    private async void Btn_Leaderboard()
    {
        await UIScreenRouter.Instance.LoadScreen("Leaderboard");
    }
    private async void Btn_Armory()
    {
        await UIScreenRouter.Instance.LoadScreen("Armory");
    }
    private async void Btn_Research()
    {
        await UIScreenRouter.Instance.LoadScreen("Research");
    }
}


