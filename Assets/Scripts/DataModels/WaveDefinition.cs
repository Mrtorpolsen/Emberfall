using System.Collections.Generic;
using UnityEngine;

public class WaveDefinition
{
    public List<EnemyGroup> enemiesToSpawn;
}

public class EnemyGroup
{
    public GameObject prefab;
    public bool isBoss;
    public int count;
    public float spawnDelay;

    public EnemyGroup(GameObject prefab, int count, float spawnDelay, bool isBoss = false)
    {
        this.prefab = prefab;
        this.isBoss = isBoss;
        this.count = count;
        this.spawnDelay = spawnDelay;
    }
}
