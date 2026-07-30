using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpawnDatabase : MonoBehaviour
{
    public static SpawnDatabase Instance { get; private set; }

    [SerializeField] private List<SpawnDefinition> spawns;

    public Dictionary<string, SpawnDefinition> spawnMap;

    public void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        spawnMap = spawns.ToDictionary(x => x.Id);

        Debug.Log($"Loaded {spawns.Count} spawns into SpawnDatabase.");

        DontDestroyOnLoad(gameObject);
    }

    public SpawnDefinition GetSpawn(string id) => id == null ? null : spawnMap[id];
    public List<SpawnDefinition> GetAllSpawns() => spawns;
}
