using System;
using UnityEngine.UIElements;

public struct ScreenDefinition
{
    public VisualTreeAsset vta;
    public Func<IUIScreenView> createView;
    public Func<IUIScreenEvents> createEvents;
    public Func<IUIScreenController> createManager; //optional

    public ScreenDefinition(
        VisualTreeAsset vta,
        Func<IUIScreenView> createView,
        Func<IUIScreenEvents> createEvents,
        Func<IUIScreenController> createManager = null)
    {
        this.vta = vta;
        this.createView = createView;
        this.createEvents = createEvents;
        this.createManager = createManager;
    }
}
