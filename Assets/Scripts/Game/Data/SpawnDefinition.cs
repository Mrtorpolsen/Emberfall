using UnityEngine;
using UnityEngine.AddressableAssets;


[CreateAssetMenu(menuName = "Game/Spawn Definition")]
public class SpawnDefinition : ScriptableObject
{
    [Header("UI")]
    [SerializeField] private string displayName;
    [SerializeField] private AssetReference icon;

    [Header("Gameplay")]
    [SerializeField] private GameObject unitPrefab;
    [SerializeField] private UnitStatsDefinition stats;

    public string DisplayName => displayName;
    public float Cost => stats.cost;
    public AssetReference Icon => icon;
    public GameObject UnitPrefab => unitPrefab;
    public SpawnType Type;
}
