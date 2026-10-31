using UnityEngine;

[CreateAssetMenu(menuName = "Game/Spawn Definition")]
public class SpawnDefinition : LoadoutDefinition
{
    [Header("UI")]
    [SerializeField] private int cooldown;

    [Header("Gameplay")]
    [SerializeField] private GameObject unitPrefab;
    [SerializeField] private UnitStatsDefinition stats;
    [Tooltip("Never in the player's loadout (bosses, enemy-only units)")]
    [SerializeField] private bool enemyOnly;

    public UnitStatsDefinition Stats => stats;
    public bool EnemyOnly => enemyOnly;
    public float Cost => stats.cost;
    public float Cooldown => cooldown;
    public GameObject UnitPrefab => unitPrefab;
}
