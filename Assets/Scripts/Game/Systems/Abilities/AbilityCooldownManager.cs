using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AbilityCooldownManager : MonoBehaviour
{
    public static AbilityCooldownManager Instance { get; private set; }

    private Dictionary<string, float> readyTime = new();

    public event Action<string> OnCooldownTriggered;
    public event Action<string> OnCooldownComplete;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void LateUpdate()
    {
        if (readyTime.Count == 0)
            return;

        var time = Time.time;

        foreach (var cooldown in readyTime.ToList())
        {
            if (time >= cooldown.Value)
            {
                readyTime.Remove(cooldown.Key);
                OnCooldownComplete?.Invoke(cooldown.Key);
            }
        }
    }

    public bool CanUse(AbilityDefinition ability, BaseUnitStats caster = null)
    {
        string key = GetCooldownKey(ability, caster);

        if (!readyTime.TryGetValue(key, out float ready))
            return true;

        return Time.time >= ready;
    }

    public float GetRemainingCooldown(AbilityDefinition ability, BaseUnitStats caster = null)
    {
        string key = GetCooldownKey(ability, caster);

        if (!readyTime.TryGetValue(key, out float ready))
            return 0;

        return Mathf.Max(0, ready - Time.time);
    }

    public float GetRemainingCooldown(string coolDownKey)
    {
        if (!readyTime.TryGetValue(coolDownKey, out float ready))
            return 0;

        return Mathf.Max(0, ready - Time.time);
    }

    public void TriggerCooldown(AbilityDefinition ability, BaseUnitStats caster = null)
    {
        string key = GetCooldownKey(ability, caster);

        readyTime[key] = Time.time + ability.cooldown;
        OnCooldownTriggered?.Invoke(key);
    }

    public void TriggerCooldown(string key, float cooldown)
    {
        readyTime[key] = Time.time + cooldown;
        OnCooldownTriggered?.Invoke(key);
    }

    public string GetCooldownKey(AbilityDefinition ability, BaseUnitStats caster = null)
    {
        if (caster == null)
            return ability.Id;

        return $"{ability.Id}_{caster.GetInstanceID()}";
    }
}