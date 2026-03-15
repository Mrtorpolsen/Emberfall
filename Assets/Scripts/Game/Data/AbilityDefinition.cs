using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(menuName = "Game/Ability Definition")]
public class AbilityDefinition : ScriptableObject
{
    [Header("UI")]
    [SerializeField] private string displayName;
    [SerializeField] private AssetReference icon;
    [SerializeField] private int cost;

    public string DisplayName => displayName;
    public float Cost => cost;
    public AssetReference Icon => icon;

    public StatEffect[] effects;
    public int duration; // In seconds, 0 for instant effects
    public float cooldown; // In seconds, 0 for no cooldown
}
