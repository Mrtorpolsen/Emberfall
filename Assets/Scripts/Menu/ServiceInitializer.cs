using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ServiceInitializer : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        
        var _ = IdentityService.main;
        var __ = SaveService.main;

        SceneManager.LoadScene("Login");
    }

}

