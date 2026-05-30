using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(menuName = "Game/Spawn Definition")]
public class SpawnDefinition : LoadoutDefinition
{
    [Header("UI")]
    [SerializeField] private int cooldown;

    [Header("Gameplay")]
    [SerializeField] private GameObject unitPrefab;
    [SerializeField] private UnitStatsDefinition stats;

    public float Cost => stats.cost;
    public float Cooldown => cooldown;
    public GameObject UnitPrefab => unitPrefab;
    public SpawnType Type;
}
