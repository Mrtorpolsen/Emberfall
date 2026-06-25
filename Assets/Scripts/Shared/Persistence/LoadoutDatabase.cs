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
    private Dictionary<string, LoadoutDefinition> definitionMap;

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

        definitionMap = abilities
            .Cast<LoadoutDefinition>()
            .Concat(spawns)
            .ToDictionary(x => x.Id);

        Debug.Log($"Loaded {abilities.Count} abilities, {spawns.Count} spawns, and {definitionMap.Count} definitions into LoadoutDatabase.");

        DontDestroyOnLoad(gameObject);
    }

    public List<LoadoutDefinition> GetAllDefinitions() => definitionMap.Values.ToList();
    public LoadoutDefinition GetDefinition(string id) => id == null ? null : definitionMap[id];

    public List<AbilityDefinition> GetAllAbilities() => abilities;
    public AbilityDefinition GetAbility(string id) => id == null ? null : abilityMap[id];

    public SpawnDefinition GetSpawn(string id) => id == null ? null : spawnMap[id];
    public List<SpawnDefinition> GetAllSpawns() => spawns;
}