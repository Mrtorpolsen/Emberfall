using System;
using UnityEngine.UIElements;

public struct ScreenDefinition
{
    public VisualTreeAsset vta;
    public Func<IUIScreen> createView;
    public Func<IUIScreenEvents> createEvents;
    public Func<IUIScreenManager> createManager; //optional

    public ScreenDefinition(
        VisualTreeAsset vta,
        Func<IUIScreen> createView,
        Func<IUIScreenEvents> createEvents,
        Func<IUIScreenManager> createManager = null)
    {
        this.vta = vta;
        this.createView = createView;
        this.createEvents = createEvents;
        this.createManager = createManager;
    }
}
