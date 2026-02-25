using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

public interface IUIScreenView
{
    void Initialize(VisualElement root);
}

public interface IUIScreenEvents
{
    void BindEvents(VisualElement root, IUIScreenController manager = null, IUIScreenView view = null);
    void Cleanup();
}

public interface IUIScreenController
{
    void Initialize(VisualElement root);
    void Cleanup();
}
