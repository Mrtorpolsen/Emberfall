using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

// A message that drops down from the top of the screen and hides itself. Each UI Toolkit scene calls Attach
// with its root once; Show uses the most recently attached root. A message shown while no root is attached
// (e.g. right before a scene change) waits for the next Attach. Built in code so it needs no UXML/USS.
public static class Toast
{
    private const long VisibleMs = 3000;
    private const float SlideSeconds = 0.25f;

    private static VisualElement banner;
    private static Label label;
    private static IVisualElementScheduledItem hideTask;
    private static string pendingMessage;

    public static void Attach(VisualElement root)
    {
        banner = new VisualElement { name = "Toast", pickingMode = PickingMode.Ignore };
        banner.style.position = Position.Absolute;
        banner.style.top = 0;
        banner.style.left = 0;
        banner.style.right = 0;
        banner.style.paddingTop = 80;
        banner.style.paddingBottom = 40;
        banner.style.paddingLeft = 60;
        banner.style.paddingRight = 60;
        banner.style.backgroundColor = new Color(0.08f, 0.05f, 0.04f, 0.92f);
        banner.style.borderBottomWidth = 4;
        banner.style.borderBottomColor = new Color(1f, 0.45f, 0.15f);
        banner.style.transitionProperty = new List<StylePropertyName> { "translate" };
        banner.style.transitionDuration = new List<TimeValue> { new TimeValue(SlideSeconds) };

        label = new Label { pickingMode = PickingMode.Ignore };
        label.style.fontSize = 48;
        label.style.color = new Color(1f, 0.92f, 0.88f);
        label.style.whiteSpace = WhiteSpace.Normal;
        label.style.unityTextAlign = TextAnchor.MiddleCenter;
        banner.Add(label);

        SetVisible(false);
        root.Add(banner);

        if (pendingMessage != null)
        {
            string message = pendingMessage;
            pendingMessage = null;

            // Next frame, so the banner slides in from its hidden position
            banner.schedule.Execute(() => Show(message));
        }
    }

    public static void Show(string message)
    {
        if (banner?.panel == null)
        {
            pendingMessage = message;
            return;
        }

        Debug.Log("Toast: " + message);

        label.text = message;
        banner.BringToFront();
        SetVisible(true);

        hideTask?.Pause();
        hideTask = banner.schedule.Execute(() => SetVisible(false)).StartingIn(VisibleMs);
    }

    // For a message that should outlive the current scene (e.g. right before a restart)
    public static void ShowOnNextScreen(string message)
    {
        pendingMessage = message;
    }

    private static void SetVisible(bool visible)
    {
        banner.style.translate = new Translate(0, Length.Percent(visible ? 0 : -100));
    }
}
