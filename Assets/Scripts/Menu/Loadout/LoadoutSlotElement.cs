using System;
using UnityEngine;
using UnityEngine.UIElements;

public class LoadoutSlotElement : IUnbindable
{
    public VisualElement Root { get; }

    private VisualElement emptyContainer;
    private VisualElement occupiedContainer;

    private Label labelName;

    private VisualElement imgOccupied;

    public bool isEmpty;

    private EventCallback<ClickEvent> boundCallback;

    public LoadoutSlotElement(VisualTreeAsset loadoutSlot)
    {
        Root = UtilityUIBinding.InstantiateRoot(loadoutSlot);

        emptyContainer = UtilityUIBinding.QRequired<VisualElement>(Root, "EmptyContainer");
        occupiedContainer = UtilityUIBinding.QRequired<VisualElement>(Root, "OccupiedContainer");

        labelName = UtilityUIBinding.QRequired<Label>(Root, "Label_Name");

        imgOccupied = UtilityUIBinding.QRequired<VisualElement>(Root, "ImgOccupied");
    }

    public void Bind(LoadoutSlotViewModel loadout)
    {
        Unbind();

        isEmpty = loadout.isEmpty;

        if (isEmpty)
        {
            emptyContainer.style.display = DisplayStyle.Flex;
            occupiedContainer.style.display = DisplayStyle.None;
        }
        else
        {
            emptyContainer.style.display = DisplayStyle.None;
            occupiedContainer.style.display = DisplayStyle.Flex;
            labelName.text = loadout.label;

            UtilityLoadAddressable.LoadAddressableIcon(loadout.icon, imgOccupied);
            UtilityLongClick.Register(Root, loadout.onLongPress);
        }

        if (loadout.isSelected)
        {
            Root.AddToClassList("selected");
        }
        else
        {
            Root.RemoveFromClassList("selected");
        }

        boundCallback = evt => loadout.onClick?.Invoke();
        Root.RegisterCallback(boundCallback);
    }

    public void Unbind()
    {
        if (boundCallback != null)
        {
            Root.UnregisterCallback(boundCallback);
            boundCallback = null;
        }
    }
}
