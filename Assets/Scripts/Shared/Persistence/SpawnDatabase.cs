using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpawnDatabase : MonoBehaviour
{
    public static SpawnDatabase Instance { get; private set; }

    [SerializeField] private List<SpawnDefinition> spawns;

    private Dictionary<string, SpawnDefinition> spawnMap;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);            
            return;
        }

        Instance = this;

        Initialize();

        Debug.Log($"Loaded {spawns.Count} spawns into SpawnDatabase.");

        DontDestroyOnLoad(gameObject);
    }
    //For testing purposes, we can initialize the database with a list of spawns
    public void Initialize(List<SpawnDefinition> definitions)
    {
        spawns = definitions;
        Initialize();
    }

    private void Initialize()
    {
        spawnMap = spawns.ToDictionary(x => x.Id);
    }

    public SpawnDefinition GetSpawn(string id) => id == null ? null : spawnMap[id];
    public List<SpawnDefinition> GetAllSpawns() => spawns;
}
