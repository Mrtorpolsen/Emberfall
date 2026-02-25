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
        { "Btn_Forge", nameof(Btn_Forge) },
        { "Btn_Research", nameof(Btn_Research) },
    };

    void Awake()
    {
        root = uIDocument.rootVisualElement.Q<VisualElement>("NavMenuContainer");

        UtilityUIBinding.BindEvents(root, this, bindings);
    }

    void OnDestroy()
    {
        UtilityUIBinding.Cleanup(this);
    }

    private void Btn_Main()
    {
        UIScreenRouter.Instance.LoadScreen("MainMenu");
    }
    private void Btn_Leaderboard()
    {
        UIScreenRouter.Instance.LoadScreen("Leaderboard");
    }
    private void Btn_Forge()
    {
        UIScreenRouter.Instance.LoadScreen("Forge");
    }
    private void Btn_Research()
    {
        UIScreenRouter.Instance.LoadScreen("Research");
    }
}


