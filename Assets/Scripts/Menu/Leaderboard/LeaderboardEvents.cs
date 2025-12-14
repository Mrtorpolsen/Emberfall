using UnityEngine;
using UnityEngine.UIElements;

public class LeaderboardEvents : IUIScreenEvents
{
    private VisualElement root;

    public void BindEvents(VisualElement root, IUIScreenManager manager = null, IUIScreenView view = null)
    {
        this.root = root;
    }

    public void Cleanup()
    {
       
    }
}
