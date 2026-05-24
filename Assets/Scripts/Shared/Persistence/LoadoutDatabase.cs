using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LoadoutDatabase : MonoBehaviour
{
    public static LoadoutDatabase Instance { get; private set; }

    [SerializeField] private List<AbilityDefinition> abilities;
    [SerializeField] private List<SpawnDefinition> spawns;

    private Dictionary<string, AbilityDefinition> abilityMap;
    private Dictionary<string, SpawnDefinition> spawnMap;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        abilityMap = abilities.ToDictionary(x => x.Id);
        spawnMap = spawns.ToDictionary(x => x.Id);

        Debug.Log($"Loaded {abilities.Count} abilities and {spawns.Count} spawns into LoadoutDatabase.");

        DontDestroyOnLoad(gameObject);
    }

    public AbilityDefinition GetAbility(string id) => id == null ? null : abilityMap[id];
    public List<AbilityDefinition> GetAllAbilities() => abilities;
    public SpawnDefinition GetSpawn(string id) => id == null ? null : spawnMap[id];
    public List<SpawnDefinition> GetAllSpawns() => spawns;
}