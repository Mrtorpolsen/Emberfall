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

        UtilityLongClick.Register(Root, loadout.onLongPress);

        if (loadout.isSelectable)
        {
            Root.RegisterCallback<ClickEvent>(_ => loadout.onClick?.Invoke());
        }
        else
        {
            //TODO add a lock icon or something to indicate it's locked
            Root.style.opacity = 0.5f;
        }
    }

    public void Unbind()
    {
        Debug.Log("Unbinding LoadoutCardElement");
    }
}
