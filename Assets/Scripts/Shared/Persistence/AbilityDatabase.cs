using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AbilityDatabase : MonoBehaviour
{
    public static AbilityDatabase Instance { get; private set; }

    [SerializeField] private List<AbilityDefinition> abilities;
    public Dictionary<string, AbilityDefinition> abilityMap;

    public void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        Debug.Log($"Loaded {abilities.Count} spawns into AbilityDatabase.");

        DontDestroyOnLoad(gameObject);
    }

    //For testing purposes, we can initialize the database with a list of spawns
    public void InitializeForTesting(List<AbilityDefinition> definitions)
    {
        abilities = definitions;
        Initialize();
    }

    public void Initialize()
    {
        abilityMap = abilities.ToDictionary(x =>
        {
            if (x.UnlockedByDefault)
            {
                UnlockService.Instance.Unlock(x.UnlockId);
            }

            return x.Id;
        });
    }

    public AbilityDefinition GetAbility(string id) => id == null ? null : abilityMap[id];
    public List<AbilityDefinition> GetAllAbilities() => abilities;
}
