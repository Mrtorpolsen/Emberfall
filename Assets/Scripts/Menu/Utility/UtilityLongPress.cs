using System;
using UnityEngine;
using UnityEngine.UIElements;

public static class UtilityLongPress
{
    private const int DEFAULT_LONG_PRESS_MS = 250;

    public static void Register(
        VisualElement element,
        Action onLongPress,
        int durationMs = DEFAULT_LONG_PRESS_MS)
    {
        if (element == null || onLongPress == null)
        {
            Debug.Log($"UtilityLongPress: {element?.name ?? "null"} or onLongPress cannot be null.");
            return;
        }

        bool pointerDown = false;
        bool longPressTriggered = false;

        IVisualElementScheduledItem scheduledItem = null;

        element.RegisterCallback<PointerDownEvent>(_ =>
        {
            pointerDown = true;
            longPressTriggered = false;

            scheduledItem?.Pause();

            scheduledItem = element.schedule.Execute(() =>
            {
                if (!pointerDown || longPressTriggered)
                    return;

                longPressTriggered = true;

                onLongPress.Invoke();

            }).StartingIn(durationMs);
        });

        element.RegisterCallback<PointerUpEvent>(_ =>
        {
            pointerDown = false;

            scheduledItem?.Pause();
        });

        element.RegisterCallback<PointerLeaveEvent>(_ =>
        {
            pointerDown = false;

            scheduledItem?.Pause();
        });
    }
}