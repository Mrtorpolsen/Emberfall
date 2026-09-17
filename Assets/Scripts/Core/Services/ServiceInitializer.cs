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
    }
}
