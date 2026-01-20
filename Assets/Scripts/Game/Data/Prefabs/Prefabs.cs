using UnityEngine;

public static class Prefabs
{
    private static PrefabDatabase database;

    private static void EnsureDatabaseLoaded()
    {
        if (database == null)
        {
            // Load from Resources folder
            database = Resources.Load<PrefabDatabase>("ScriptableObjects/PrefabDatabase");

            if (database == null)
            {
                Debug.LogError("PrefabDatabase not found in Resources!");
            }
        }
    }

    public static GameObject fighterPrefab
    {
        get
        {
            EnsureDatabaseLoaded();
            return database.fighterPrefab;
        }
    }

    public static GameObject rangerPrefab
    {
        get
        {
            EnsureDatabaseLoaded();
            return database.rangerPrefab;
        }
    }

    public static GameObject cavalierPrefab
    {
        get
        {
            EnsureDatabaseLoaded();
            return database.cavalierPrefab;
        }
    }
    public static GameObject towerPrefab
    {
        get
        {
            EnsureDatabaseLoaded();
            return database.towerPrefab;
        }
    }

    public static GameObject giantPrefab
    {
        get
        {
            EnsureDatabaseLoaded();
            return database.giantPrefab;
        }
    }
    
    public static GameObject eliteFighterPrefab
    {
        get
        {
            EnsureDatabaseLoaded();
            return database.eliteFighterPrefab;
        }
    }
    public static GameObject eliteCavalierPrefab
    {
        get
        {
            EnsureDatabaseLoaded();
            return database.eliteCavalierPrefab;
        }
    }

    public static GameObject assasinPrefab
    {
        get
        {
            EnsureDatabaseLoaded();
            return database.assasinPrefab;
        }
    }

    public static GameObject gatePrefab
    {
        get
        {
            EnsureDatabaseLoaded();
            return database.gatePrefab;
        }
    }
}
