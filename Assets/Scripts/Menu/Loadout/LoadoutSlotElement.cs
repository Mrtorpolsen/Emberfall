using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UIElements;

public class LoadoutSlotElement : VisualElement, IUnbindable
{
    private VisualElement emptyContainer;
    private VisualElement occupiedContainer;

    private Label labelName;

    private VisualElement imgOccupied;

    public LoadoutSlotElement(VisualTreeAsset loadoutSlot)
    {
        var visualNode = loadoutSlot.CloneTree();
        this.Add(visualNode);

        emptyContainer = UtilityUIBinding.QRequired<VisualElement>(visualNode, "EmptyContainer");
        occupiedContainer = UtilityUIBinding.QRequired<VisualElement>(visualNode, "OccupiedContainer");

        labelName = UtilityUIBinding.QRequired<Label>(visualNode, "Label_Name");

        imgOccupied = UtilityUIBinding.QRequired<VisualElement>(visualNode, "ImgOccupied");
    }

    public void Bind(LoadoutDefinition loadout)
    {
        Unbind();

        if (loadout == null)
        {
            emptyContainer.style.display = DisplayStyle.Flex;
            occupiedContainer.style.display = DisplayStyle.None;
        }
        else
        {
            emptyContainer.style.display = DisplayStyle.None;
            occupiedContainer.style.display = DisplayStyle.Flex;
            labelName.text = loadout.DisplayName;

            loadout.Icon.LoadAssetAsync<Sprite>().Completed += handle =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    imgOccupied.style.backgroundImage = new StyleBackground(handle.Result);
                }
            };
        }
    }

    public void Unbind()
    {
        Debug.Log("Unbinding LoadoutSlotElement");
    }
}
