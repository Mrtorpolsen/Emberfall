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
    [SerializeField] private float cost;

    public string DisplayName => displayName;
    public float Cost => cost;
    public AssetReference Icon => icon;
    public GameObject UnitPrefab => unitPrefab;
    public SpawnType Type;
}
