using System.Collections.Generic;
using UnityEngine;

public class AbilityCooldownManager : MonoBehaviour
{
    public static AbilityCooldownManager Instance;

    private Dictionary<AbilityDefinition, float> lastUseTime = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public bool CanUse(AbilityDefinition ability)
    {
        if (!lastUseTime.TryGetValue(ability, out float last))
            return true;

        return Time.time >= last + ability.cooldown;
    }

    public float GetRemainingCooldown(AbilityDefinition ability)
    {
        if (!lastUseTime.TryGetValue(ability, out float last))
            return 0;

        return Mathf.Max(0, last + ability.cooldown - Time.time);
    }

    public void TriggerCooldown(AbilityDefinition ability)
    {
        lastUseTime[ability] = Time.time;
    }
}