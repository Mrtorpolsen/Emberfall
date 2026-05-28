using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

public class LoadoutView : IUIScreenView
{
    public Task InitializeAsync(VisualElement root)
    {
        Debug.Log("Yay view");
        return Task.CompletedTask;
    }
}