using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ServiceInitializer : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        InitializeServices();

        InitializeDatabases();

        SceneManager.LoadScene("Login");
    }

    private void InitializeServices()
    {
        IdentityService.Create();
        SaveService.Create();
        CurrencyManager.Create();
        UnlockService.Create();

        Debug.Log("All services initialized");
    }

    private void InitializeDatabases()
    {
        SpawnDatabase.Instance.Initialize();
        AbilityDatabase.Instance.Initialize();

        // The databases are plain lookups (so tests can use them without services); default unlocks are applied here
        UnlockDefaults(SpawnDatabase.Instance.GetAllSpawns());
        UnlockDefaults(AbilityDatabase.Instance.GetAllAbilities());
    }

    private static void UnlockDefaults(IEnumerable<LoadoutDefinition> definitions)
    {
        foreach (var definition in definitions)
        {
            if (definition.UnlockedByDefault)
            {
                UnlockService.Instance.Unlock(definition.UnlockId);
            }
        }
    }
}
