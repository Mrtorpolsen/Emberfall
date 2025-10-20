using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WaveDefinition
{
    public List<EnemyGroup> enemiesToSpawn;
}

[System.Serializable]
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
