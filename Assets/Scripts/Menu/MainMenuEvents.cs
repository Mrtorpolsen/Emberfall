using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainMenuEvents : MonoBehaviour, IUIScreen
{
    [SerializeField] private MainMenuView menuView;

    //USE NAMING CONVENTION OF BTN --- Btn_xxx so it can add Clicked behind
    private Dictionary<Button, Action> buttonActions = new Dictionary<Button, Action>();

    private void OnDisable()
    {
        foreach (var kvp in buttonActions)
        {
            var button = kvp.Key;
            var action = kvp.Value;

            if (button != null && action != null)
                button.clicked -= action;
        }

        buttonActions.Clear();
    }

    //Maybe move to utility for easier use in every eventhandler
    public void Initialize(UIDocument document)
    {
        menuView.Initialize();

        var root = document.rootVisualElement;

        var menuButtons = root.Query<Button>().ToList();

        foreach (var button in menuButtons)
        {
            string methodName = button.name + "Clicked";
            MethodInfo method = GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);

            if (method != null)
            {
                // Wrap the method so it can unregister later
                Action action = () => method.Invoke(this, null);

                // Register callback
                button.clicked += action;

                // Store for unregistration
                buttonActions[button] = action;

                Debug.Log($"Button {button.name} wired to method {methodName}");
            }
        }
    }

    private void Btn_PlayClicked()
    {
        Debug.Log("Play clicked loading Game...");
        SceneManager.LoadScene("Game");
    }

    private void Btn_Offer3Clicked()
    {
        Debug.Log("Btn_Offer3 clicked...");
    }
    private void Btn_ShopClicked()
    {
        Debug.Log("Btn_ShopClicked clicked...");
    }
    private void Btn_ForgeClicked()
    {
        Debug.Log("Btn_ForgeClicked clicked...");
    }
    private void Btn_MainClicked()
    {
        Debug.Log("Btn_MainClicked clicked...");
    }
    private void Btn_ResearchClicked()
    {
        Debug.Log("Btn_ResearchClicked clicked...");
    }
    private void Btn_LeaderboardClicked()
    {
        Debug.Log("Btn_LeaderboardClicked clicked...");
        SceneManager.LoadScene("Leaderboard");
    }
}
