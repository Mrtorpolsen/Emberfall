using System.Collections.Generic;
using UnityEngine;

public class EffectSystem : MonoBehaviour
{
    public static EffectSystem Instance;

    private readonly List<ActiveEffect> activeEffects = new();
    
    private readonly Dictionary<(BaseUnitStats, EffectId), ActiveEffect> activeEffectsLookup = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Update()
    {
        Tick(Time.deltaTime);
    }

    public void ApplyEffect(BaseUnitStats target, IEffect effect, float duration)
    {
        var key = (target, effect.Id);

        if (activeEffectsLookup.TryGetValue(key, out var existingEffect))
        {
            existingEffect.RemainingDuration = duration;

            return;
        }

        var instance = new ActiveEffect(target, effect, duration);

        effect.OnApply(target);

        activeEffects.Add(instance);
        activeEffectsLookup[key] = instance;

        target.ActiveEffects.Add(instance);
    }

    private void Tick(float deltaTime)
    {
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            var activeEffect = activeEffects[i];

            activeEffect.Effect.Tick(activeEffect.Target, deltaTime);
            activeEffect.RemainingDuration -= deltaTime;

            if(activeEffect.RemainingDuration <= 0f)
            {
                activeEffect.Effect.OnExpire(activeEffect.Target);

                activeEffectsLookup.Remove((activeEffect.Target, activeEffect.Id));
                activeEffect.Target.ActiveEffects.Remove(activeEffect);

                activeEffects.RemoveAt(i);
            }
        }
    }

    public void RemoveAllEffects(BaseUnitStats target)
    {
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            var activeEffect = activeEffects[i];

            if(activeEffect.Target != target)
                continue;
            
            activeEffect.Effect.OnExpire(activeEffect.Target);

            activeEffectsLookup.Remove((activeEffect.Target, activeEffect.Id));
            activeEffect.Target.ActiveEffects.Remove(activeEffect);

            activeEffects.RemoveAt(i);
        }
    }
}
