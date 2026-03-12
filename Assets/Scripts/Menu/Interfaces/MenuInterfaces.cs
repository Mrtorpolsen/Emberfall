using System.Threading.Tasks;
using UnityEngine.UIElements;

public interface IUIScreenView
{
    Task InitializeAsync(VisualElement root);
}

public interface IUIScreenEvents
{
    void BindEvents(VisualElement root, IUIScreenController manager = null, IUIScreenView view = null);
    void Cleanup();
}

public interface IUIScreenController
{
    void Initialize(IUIScreenView view);
    void Cleanup();
}
