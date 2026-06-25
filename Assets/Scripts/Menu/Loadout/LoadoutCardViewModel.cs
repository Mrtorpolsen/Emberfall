using System;
using UnityEngine.AddressableAssets;

public class LoadoutCardViewModel
{
    public bool isSelectable;

    public DefinitionCategory Type;

    public string label;
    public AssetReference icon;

    public Action onClick;
    public Action onLongPress;
}