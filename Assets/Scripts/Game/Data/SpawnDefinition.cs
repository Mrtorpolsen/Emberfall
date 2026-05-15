using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(menuName = "Game/Spawn Definition")]
public class SpawnDefinition : ScriptableObject
{
    [Header("UI")]
    [SerializeField] private string displayName;
    [SerializeField] private AssetReference icon;
    [SerializeField] private int cooldown;

    [Header("Gameplay")]
    [SerializeField] private GameObject unitPrefab;
    [SerializeField] private UnitStatsDefinition stats;

    public string DisplayName => displayName;
    public float Cost => stats.cost;
    public float Cooldown => cooldown;
    public AssetReference Icon => icon;
    public GameObject UnitPrefab => unitPrefab;
    public SpawnType Type;
}
