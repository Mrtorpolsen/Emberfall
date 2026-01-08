using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ServiceInitializer : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        InitializeServices();

        SceneManager.LoadScene("Login");
    }

    private void InitializeServices()
    {
        IdentityService.Create();
        SaveService.Create();

        Debug.Log("All services initialized");
    }

}

