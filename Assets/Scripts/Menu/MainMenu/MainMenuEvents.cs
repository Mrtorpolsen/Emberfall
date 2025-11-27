using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainMenuEvents : IUIScreenEvents
{
    [SerializeField] private UIDocument uIDocument;

    private VisualElement root;

    //USE NAMING CONVENTION OF BTN --- Btn_xxx so it can add Clicked behind
    private Dictionary<Button, Action> buttonActions = new Dictionary<Button, Action>();

    //Maybe move to utility for easier use in every eventhandler
    public void BindEvents(VisualElement root)
    {
        this.root = root;

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

    public void Cleanup()
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

    private void Btn_PlayClicked()
    {
        Debug.Log("Play clicked loading Game...");
        SceneManager.LoadScene("Game");
    }

    private void Btn_Offer3Clicked()
    {
        Debug.Log("Btn_Offer3 clicked...");
    }
}
