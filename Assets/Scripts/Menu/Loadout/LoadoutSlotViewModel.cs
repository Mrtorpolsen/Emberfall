using System;
using UnityEngine.AddressableAssets;

public class LoadoutSlot
{
    public DefinitionCategory Type;
    public int Index;
    public LoadoutDefinition Definition;
}

public class LoadoutSlotViewModel
{
    public bool isEmpty;

    public DefinitionCategory Type;

    public string label;
    public AssetReference icon;   

    public Action onClick;
    public Action onLongPress;
}