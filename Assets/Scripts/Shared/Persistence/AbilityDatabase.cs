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

        abilityMap = abilities.ToDictionary(x => x.Id);

        Debug.Log($"Loaded {abilities.Count} spawns into AbilityDatabase.");

        DontDestroyOnLoad(gameObject);
    }

    public AbilityDefinition GetAbility(string id) => id == null ? null : abilityMap[id];
    public List<AbilityDefinition> GetAllAbilities() => abilities;
}
