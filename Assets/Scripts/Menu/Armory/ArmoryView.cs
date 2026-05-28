using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.UIElements;

public class ArmoryView : IUIScreenView
{
    public Task InitializeAsync(VisualElement root)
    {
        Debug.Log("Yay view");
        return Task.CompletedTask;
    }
}