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

    private Action clickHandler;

    public bool isEmpty;

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
            UtilityLongPress.Register(Root, loadout.onLongPress);
        }

    }

    private void HandleOccupiedClicked()
    {
        clickHandler?.Invoke();
    }
    private void HandleEmptyClicked()
    {
        clickHandler?.Invoke();
    }

    public void Unbind()
    {
        Debug.Log("Unbinding LoadoutSlotElement");
    }
}
