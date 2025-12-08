using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

public interface IUIScreen
{
    void Initialize(VisualElement root);
}

public interface IUIScreenEvents
{
    void BindEvents(VisualElement root);
    void Cleanup();
}

public interface IUIScreenManager
{
    void Initialize(VisualElement root);
}
