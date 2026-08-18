using System.Collections.Generic;
using UnityEngine;

public class WaveDefinition
{
    public List<EnemyGroup> enemiesToSpawn;
}

public class EnemyGroup
{
    public GameObject prefab;
    public GameObject bossPrefab;
    public int count;
    public float spawnDelay;

    public EnemyGroup(GameObject prefab, int count, float spawnDelay, GameObject bossPrefab = null)
    {
        this.prefab = prefab;
        this.bossPrefab = bossPrefab;
        this.count = count;
        this.spawnDelay = spawnDelay;
    }
}
