using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public static class UtilityUIBinding
{
    //USE NAMING CONVENTION OF BTN --- Btn_xxx so it can add Clicked behind
    private static readonly Dictionary<object, Dictionary<Button, Action>> instanceActions
        = new Dictionary<object, Dictionary<Button, Action>>();

    public static void BindEvents(VisualElement root, object target, Dictionary<string, string> bindings)
    {
        if (!instanceActions.ContainsKey(target))
        {
            instanceActions[target] = new Dictionary<Button, Action>();
        }

        var actions = instanceActions[target];

        var buttons = root.Query<Button>().ToList();

        foreach (var button in buttons)
        {
            if (!bindings.TryGetValue(button.name, out string methodName))
            {
                Debug.LogWarning($"[UIBinding] No binding for {button.name}");
                continue;
            }

            MethodInfo method = target.GetType()
                .GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (method == null)
            {
                Debug.LogError($"[UIBinding] Method '{methodName}' not found on {target.GetType().Name}");
                continue;
            }
            // Wrap the method so it can unregister later

            Action action = () => method.Invoke(target, null);

            // Register callback
            button.clicked += action;

            // Store for unregistration
            actions[button] = action;

            Debug.Log($"[UIBinding] Bound {button.name} → {methodName}");
        }
    }

    public static void Cleanup(object target)
    {
        if (!instanceActions.TryGetValue(target, out var actions))
        {
            return;
        }

        foreach (var kvp in actions)
        {
            if (kvp.Key != null && kvp.Value != null)
            {
                kvp.Key.clicked -= kvp.Value;
            }
        }

        actions.Clear();
        instanceActions.Remove(target);
    }
}