using UnityEngine;
using UnityEngine.UIElements;

public class LeaderboardEvents : IUIScreenEvents
{
    private VisualElement root;

    public void BindEvents(VisualElement root)
    {
        this.root = root;
    }

    public void Cleanup()
    {
       
    }
}
