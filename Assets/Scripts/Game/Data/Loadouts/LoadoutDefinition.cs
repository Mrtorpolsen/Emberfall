using UnityEngine;
using UnityEngine.AddressableAssets;

public abstract class LoadoutDefinition : ScriptableObject
{
    [Header("UI")]
    [SerializeField] private string displayName;
    [SerializeField] private AssetReference icon;

    [SerializeField] private string id;
    [SerializeField] private string unlockId;
    [SerializeField] private bool unlockedByDefault;
    [SerializeField] private DefinitionCategory slotType;
    
    public string Id => id;
    public string UnlockId => unlockId;
    public bool UnlockedByDefault => unlockedByDefault;
    public string DisplayName => displayName;
    public AssetReference Icon => icon;
    public DefinitionCategory SlotType => slotType;
}