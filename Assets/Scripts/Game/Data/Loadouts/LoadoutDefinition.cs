using UnityEngine;
using UnityEngine.AddressableAssets;

public abstract class LoadoutDefinition : ScriptableObject
{
    [Header("UI")]
    [SerializeField] private string displayName;
    [SerializeField] private AssetReference icon;
    // Shown under the stats in the loadout popups (units and towers; abilities use their research description)
    [Tooltip("Player-facing loadout text; leave empty on enemy-only units (enemy text will go in the encyclopedia)")]
    [SerializeField, TextArea(3, 6)] private string loadoutDescription;

    [SerializeField] private string id;
    [SerializeField] private string unlockId;
    [SerializeField] private bool unlockedByDefault;
    [SerializeField] private DefinitionCategory slotType;

    public string Id => id;
    public string UnlockId => unlockId;
    public bool UnlockedByDefault => unlockedByDefault;
    public string DisplayName => displayName;
    public AssetReference Icon => icon;
    public string LoadoutDescription => loadoutDescription;
    public DefinitionCategory SlotType => slotType;
}