using UnityEngine;
using UnityEngine.AddressableAssets;

public abstract class LoadoutDefinition : ScriptableObject
{
    [SerializeField] private string id;
    public string Id => id;

    [Header("UI")]
    [SerializeField] private string displayName;
    [SerializeField] private AssetReference icon;
    public string DisplayName => displayName;
    public AssetReference Icon => icon;

}