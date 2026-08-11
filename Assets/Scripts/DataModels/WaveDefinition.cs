using System.Collections.Generic;
using UnityEngine;

public class WaveDefinition
{
    public List<EnemyGroup> enemiesToSpawn;
}

public class EnemyGroup
{
    public GameObject prefab;
    public int count;
    public float spawnDelay;

    public EnemyGroup(GameObject prefab, int count, float spawnDelay)
    {
        this.prefab = prefab;
        this.count = count;
        this.spawnDelay = spawnDelay;
    }
}
