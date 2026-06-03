using System;
using UnityEngine.AddressableAssets;

public class LoadoutSlot
{
    public DefinitionCategory SlotType;
    public int Index;
    public LoadoutDefinition Definition;
}

public class LoadoutSlotViewModel
{
    public bool isEmpty;

    public DefinitionCategory SlotType;

    public string label;
    public AssetReference icon;   

    public Action onClick;
    public Action onLongPress;
}