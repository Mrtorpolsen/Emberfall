using System.Collections.Generic;
using UnityEngine;

public class AbilitySystem : MonoBehaviour
{
    public static AbilitySystem Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    
}
