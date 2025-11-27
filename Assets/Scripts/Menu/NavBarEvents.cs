using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

public class NavBarEvents : MonoBehaviour
{
    [Header("References")]
    public UIDocument uiDocument;

    private VisualElement root;
    private List<Button> navButtons = new List<Button>();

    void Awake()
    {
        SetupNavBar();
    }

    void SetupNavBar()
    {
        root = uiDocument.rootVisualElement;

        navButtons.Clear();
        navButtons.AddRange(root.Query<Button>().ToList());

        foreach (var btn in navButtons)
        {
            btn.clicked += () => OnButtonClicked(btn);
        }
    }

    private void OnButtonClicked(Button btn)
    {
        switch (btn.name)
        {
            case "Btn_Main":
                MenuManager.main.LoadScreen("MainMenu");
                break;
            case "Btn_Leaderboard":
                MenuManager.main.LoadScreen("Leaderboard");
                break;
        }
    }

    void OnDestroy()
    {
        foreach (var btn in navButtons)
        {
            btn.clicked -= () => OnButtonClicked(btn);
        }
    }
}

