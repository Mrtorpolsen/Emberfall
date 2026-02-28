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

    private static readonly List<(VisualElement element, EventCallback<ClickEvent> handler)> clickHandlers = new();

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

    public static void CleanupEvents(object target)
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

    public static void BindVEClick(VisualElement element, Action handler, List<(VisualElement element, EventCallback<ClickEvent> handler)> clickHandlers)
    {
        EventCallback<ClickEvent> callback = _ => handler?.Invoke();
        element.RegisterCallback(callback);
        clickHandlers.Add((element, callback));
    }

    public static void CleanupVEClicks(List<(VisualElement element, EventCallback<ClickEvent> handler)> clickHandlers)
    {
        foreach (var (element, handler) in clickHandlers)
        {
            element.UnregisterCallback(handler);
        }
        clickHandlers.Clear();
    }

    public static void BindButtonClick(Button button, Action handler, List<(Button button, Action handler)> boundButtons)
    {
        if (button == null || handler == null) return;

        button.clicked += handler;
        boundButtons.Add((button, handler));
    }

    public static void CleanupButtonClicks(List<(Button button, Action handler)> boundButtons)
    {
        foreach (var (button, handler) in boundButtons)
        {
            if (button != null && handler != null)
            {
                button.clicked -= handler;
            }
        }
        boundButtons.Clear();
    }

    public static T QRequired<T>(VisualElement root, string name)
    where T : VisualElement
    {
        var element = root.Q<T>(name);
        if (element == null)
            throw new InvalidOperationException(
                $"UI contract violation: '{name}' ({typeof(T).Name}) missing in '{root.name}'."
            );

        return element;
    }

    public static VisualElement InstantiateRoot(VisualTreeAsset asset)
    {
        var container = asset.Instantiate();
        var root = container[0];
        container.RemoveAt(0);
        return root;
    }
}