using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public static class UtilityLongClick
{
    private class LongPressRegistration
    {
        public EventCallback<PointerDownEvent> PointerDown;
        public EventCallback<PointerUpEvent> PointerUp;
        public EventCallback<PointerLeaveEvent> PointerLeave;

        public IVisualElementScheduledItem ScheduledItem;
        public Action AppendedAction;
    }

    private static readonly Dictionary<VisualElement, Action> longPressActions = new();
    private static readonly Dictionary<VisualElement, LongPressRegistration> registrations = new();

    private const int DEFAULT_LONG_PRESS_MS = 400;

    public static void Register(
        VisualElement element,
        Action onLongPress,
        int durationMs = DEFAULT_LONG_PRESS_MS)
    {
        if (element == null || onLongPress == null)
        {
            Debug.Log(
                $"UtilityLongPress: {element?.name ?? "null"} or onLongPress cannot be null."
            );
            return;
        }

        Unregister(element);

        var registration = new LongPressRegistration();

        longPressActions[element] = onLongPress;
        registrations[element] = registration;

        bool pointerDown = false;
        bool longPressTriggered = false;

        registration.PointerDown = evt =>
        {
            pointerDown = true;
            longPressTriggered = false;

            registration.ScheduledItem?.Pause();

            registration.ScheduledItem = element.schedule.Execute(() =>
            {
                // This registration is no longer active.
                if (!registrations.TryGetValue(element, out var currentRegistration) ||
                    !ReferenceEquals(currentRegistration, registration))
                {
                    return;
                }

                if (!pointerDown || longPressTriggered)
                    return;

                longPressTriggered = true;

                if (longPressActions.TryGetValue(element, out var action))
                    action?.Invoke();

            }).StartingIn(durationMs);
        };

        registration.PointerUp = evt =>
        {
            pointerDown = false;
            registration.ScheduledItem?.Pause();
        };

        registration.PointerLeave = evt =>
        {
            pointerDown = false;
            registration.ScheduledItem?.Pause();
        };

        element.RegisterCallback(registration.PointerDown);
        element.RegisterCallback(registration.PointerUp);
        element.RegisterCallback(registration.PointerLeave);
    }

    public static void Append(
        VisualElement element,
        Action action)
    {
        if (element == null || action == null)
            return;

        if (!registrations.TryGetValue(element, out var registration))
            return;

        if (!longPressActions.ContainsKey(element))
            return;

        if (registration.AppendedAction != null)
        {
            longPressActions[element] -= registration.AppendedAction;
        }

        longPressActions[element] += action;

        registration.AppendedAction = action;
    }

    public static void Unregister(VisualElement element)
    {
        if (element == null)
            return;

        if (registrations.TryGetValue(element, out var registration))
        {
            registration.ScheduledItem?.Pause();
            registration.ScheduledItem = null;

            element.UnregisterCallback(registration.PointerDown);
            element.UnregisterCallback(registration.PointerUp);
            element.UnregisterCallback(registration.PointerLeave);

            registration.PointerDown = null;
            registration.PointerUp = null;
            registration.PointerLeave = null;
            registration.AppendedAction = null;

            registrations.Remove(element);
        }

        longPressActions.Remove(element);
    }
}