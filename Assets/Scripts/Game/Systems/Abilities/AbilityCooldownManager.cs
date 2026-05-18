using System;
using System.Collections.Generic;
using UnityEngine;

public class AbilityCooldownManager : MonoBehaviour
{
    public static AbilityCooldownManager Instance { get; private set; }

    private Dictionary<string, float> lastUseTime = new();

    public event Action<string> OnCooldownTriggered;

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
        if (!lastUseTime.TryGetValue(ability.DisplayName, out float last))
            return true;

        return Time.time >= last + ability.cooldown;
    }

    public float GetRemainingCooldown(string abilityName, float cooldown)
    {
        if (!lastUseTime.TryGetValue(abilityName, out float last))
            return 0;

        return Mathf.Max(0, last + cooldown - Time.time);
    }

    public void TriggerCooldown(string abilityName)
    {
        lastUseTime[abilityName] = Time.time;
        OnCooldownTriggered?.Invoke(abilityName);
    }
}