using System.Threading.Tasks;
using UnityEngine.UIElements;

public class ArmoryView : IUIScreenView
{
    public Task InitializeAsync(VisualElement root)
    {
        TutorialFlow.Instance.ArmoryReady();
        return Task.CompletedTask;
    }
}