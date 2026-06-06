using UnityEngine;
using UnityEngine.UIElements;

public class LoadoutCardElement : IUnbindable
{
    public VisualElement Root { get; }

    private VisualElement contentContainer;

    private VisualElement img;

    private Label labelName;

    public bool isUnlocked;

    public LoadoutCardElement(VisualTreeAsset loadoutCard)
    {
        Root = UtilityUIBinding.InstantiateRoot(loadoutCard);

        contentContainer = UtilityUIBinding.QRequired<VisualElement>(Root, "ContentContainer");
        img = UtilityUIBinding.QRequired<VisualElement>(Root, "Img");
        labelName = UtilityUIBinding.QRequired<Label>(Root, "Label_Name");
    }

    public void Bind(LoadoutCardViewModel loadout)
    {
        Unbind();

        labelName.text = loadout.label;
        UtilityLoadAddressable.LoadAddressableIcon(loadout.icon, img);

        UtilityLongPress.Register(Root, loadout.onLongPress);
        //TODO locked
    }

    public void Unbind()
    {
        Debug.Log("Unbinding LoadoutCardElement");
    }
}
