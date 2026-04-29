using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

[RequireComponent(typeof(UnitMetadata))]
public abstract class BaseUnitStats : MonoBehaviour, IUnit, ITargetable
{
    [Header("Reference")]
    [SerializeField] protected GameObject unit;
    [SerializeField] private UnitStatsDefinition baseStats;
    [SerializeField] private Collider2D collider;

    protected UnitStatsDefinition BaseStats => baseStats;

    public int currentHealth;
    protected UnitMetadata metadata;
    private RuntimeStats runtimeStats;


    public List<ActiveEffect> ActiveEffects = new ();

    private readonly Dictionary<StatType, List<StatModifier>> modifiersByStat = new();
    private readonly HashSet<StatType> dirtyStats = new();

    private FloatingHealthBar healthBar;
    public Vector3 healthBarScale;

    // IUnit
    public float AttackRange => runtimeStats.attackRange;
    public int AttackDamage => runtimeStats.attackDamage;
    public float AttackSpeed => runtimeStats.attackSpeed;
    public float MovementSpeed => runtimeStats.movementSpeed;
    public float CritChance => runtimeStats.critChance;
    public float CritDamage => runtimeStats.critDamage;
    public int MaxHealth => runtimeStats.maxHealth;
    public int Armor => runtimeStats.armor;

    // ITargetable
    public GameObject GameObject => gameObject;
    public Transform Transform => (this != null) ? transform : null;
    public float HitRadius => runtimeStats.hitRadius;
    public bool IsAlive => currentHealth > 0;
    public virtual ThreatLevel UnitPrio => runtimeStats.unitPrio;
    public bool IsTargetable => runtimeStats.isTargetable;

    // UnitMetadata
    public Team Team => metadata.Team;
    public float Cost => baseStats.cost;

    //Debug
#if UNITY_EDITOR
    [Header("Deubg Stats")]
    [SerializeField] private float debugCost;

    [SerializeField] private int debugMaxHealth;
    [SerializeField] private int debugCurrentHealth;
    [SerializeField] private int debugArmor;

    [SerializeField] private int debugAttackDamage;
    [SerializeField] private float debugAttackSpeed;
    [SerializeField] private float debugAttackRange;
    [SerializeField] private float debugHitRadius;

    [SerializeField] private float debugMovementSpeed;

    [SerializeField] private float debugCritChance;
    [SerializeField] private float debugCritDamage;

    [SerializeField] private ThreatLevel debugUnitPrio;
    [SerializeField] private bool debugIsTargetable;
#endif

    protected virtual void Awake()
    {
        runtimeStats = new RuntimeStats
        {
            maxHealth = baseStats.maxHealth,
            attackDamage = baseStats.attackDamage,
            armor = baseStats.armor,
            attackSpeed = baseStats.attackSpeed,
            movementSpeed = baseStats.movementSpeed,
            attackRange = baseStats.attackRange,
            cost = baseStats.cost,
            hitRadius = baseStats.hitRadius,
            critChance = baseStats.critChance,
            critDamage = baseStats.critDamage,
            unitPrio = baseStats.unitPrio,
            isTargetable = baseStats.isTargetable
        };

        metadata = GetComponent<UnitMetadata>();
        currentHealth = runtimeStats.maxHealth;

        healthBarScale = baseStats.healthbarScale;

        if (runtimeStats == null)
        {
            Debug.LogError($"{name} has no runtimeStats assigned.", this);
        }
#if UNITY_EDITOR
        SyncDebugStats();
#endif
    }

    private void LateUpdate()
    {
        if (dirtyStats.Count == 0) return;

        foreach (var stat in dirtyStats)
        {
            float baseValue = GetBaseStat(stat);
            float finalValue = CalculateStat(stat, baseValue);

            ApplyToRuntime(stat, finalValue);
        }

        dirtyStats.Clear();

#if UNITY_EDITOR
        SyncDebugStats();
#endif
    }

    public virtual void TakeDamage(int amount)
    {
        currentHealth -= ApplyArmorReduction(amount);

#if UNITY_EDITOR
        SyncDebugStats();
#endif

        if (HealthbarManager.Instance != null)
        {
            if (healthBar == null)
            {
                healthBar = HealthbarManager.Instance.RequestHealthBar(this);
            }

            healthBar.CancelFade();
            
            healthBar.UpdateValue(currentHealth, MaxHealth);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        if (!IsAlive) return;

#if UNITY_EDITOR
        SyncDebugStats();
#endif

        bool wasFull = currentHealth == MaxHealth;

        // set health to no more than max health
        currentHealth = Mathf.Min(MaxHealth, currentHealth + amount);

        if (HealthbarManager.Instance != null)
        {
            if (!wasFull &&healthBar != null)
            {
                healthBar.UpdateValue(currentHealth, MaxHealth);

                if (currentHealth >= MaxHealth)
                {
                    healthBar.TryStartFade();
                }
            }

        }
    }

    private int ApplyArmorReduction(int dmg)
    {
        float reductionFactor = 100f / (runtimeStats.armor + 100f);
        return Mathf.Max(1, Mathf.RoundToInt(dmg * reductionFactor));
    }

    public virtual void Die()
    {
        TargetRegistry.Instance.UnregisterUnit(this);

        if (healthBar != null)
        {
            HealthbarManager.Instance.ReturnHealthBarToPool(this);
        }

        Destroy(unit != null ? unit : gameObject);
    }

    public virtual void ApplyFinalStats(FinalStats finalStats)
    {
        runtimeStats.maxHealth = finalStats.maxHealth;
        currentHealth = finalStats.maxHealth;

        runtimeStats.attackDamage = finalStats.attackDamage;
        runtimeStats.attackSpeed = finalStats.attackSpeed;
        runtimeStats.attackRange = finalStats.attackRange;
        runtimeStats.movementSpeed = finalStats.movementSpeed;
        runtimeStats.hitRadius = finalStats.hitRadius;
        runtimeStats.cost = finalStats.cost;
        runtimeStats.armor = finalStats.armor;
        runtimeStats.critChance = finalStats.critChance;
        runtimeStats.critDamage = finalStats.critDamage;

#if UNITY_EDITOR
        SyncDebugStats();
#endif
    }

    private float GetBaseStat(StatType stat)
    {
        return stat switch
        {
            StatType.Health => baseStats.maxHealth,
            StatType.AttackDamage => baseStats.attackDamage,
            StatType.Armor => baseStats.armor,
            StatType.AttackSpeed => baseStats.attackSpeed,
            StatType.AttackRange => baseStats.attackRange,
            StatType.CritChance => baseStats.critChance,
            StatType.CritDamage => baseStats.critDamage,
            _ => 0f
        };
    }

    public int GetAttackDamage()
    {
        int dmg = runtimeStats.attackDamage;

        if(runtimeStats.critChance > 0)
        {
            if(RollCrit())
            {
                dmg = Mathf.RoundToInt(dmg * runtimeStats.critDamage);
                ShowCritFeedback(dmg);
            }
        }

        return dmg;
    }

    public void AddModifier(StatModifier modifier)
    {
        if (!modifiersByStat.TryGetValue(modifier.Stat, out var list))
        {
            list = new List<StatModifier>();
            modifiersByStat[modifier.Stat] = list;
        }

        list.Add(modifier);
        dirtyStats.Add(modifier.Stat);
    }

    public void RemoveModifier(StatModifier modifier)
    {
        if (modifiersByStat.TryGetValue(modifier.Stat, out var list))
        {
            list.Remove(modifier);

            if (list.Count == 0)
            {
                modifiersByStat.Remove(modifier.Stat);
            }

            dirtyStats.Add(modifier.Stat);
        }
    }

    private float CalculateStat(StatType stat, float baseValue)
    {
        if (!modifiersByStat.TryGetValue(stat, out var list))
            return baseValue;

        float value = baseValue;

        foreach (var mod in list)
        {
            if (mod.Type == ModifierType.Flat)
            {
                value += mod.Value;
            }
            else if (mod.Type == ModifierType.Percent)
            {
                value *= (1f + mod.Value); // multiplicative stacking
            }
        }

        return value;
    }

    private void ApplyToRuntime(StatType stat, float value)
    {
        switch (stat)
        {
            case StatType.Health:
                {
                    float percent = (float)currentHealth / runtimeStats.maxHealth;

                    runtimeStats.maxHealth = Mathf.RoundToInt(value);
                    currentHealth = Mathf.RoundToInt(runtimeStats.maxHealth * percent);

                    if (HealthbarManager.Instance != null)
                    {
                        if (healthBar != null)
                        {
                            healthBar.UpdateValue(currentHealth, MaxHealth);
                        }
                    }

                    break;
                }

            case StatType.AttackDamage:
                runtimeStats.attackDamage = Mathf.RoundToInt(value);
                break;

            case StatType.Armor:
                runtimeStats.armor = Mathf.RoundToInt(value);
                break;

            case StatType.AttackSpeed:
                runtimeStats.attackSpeed = value;
                break;


            case StatType.AttackRange:
                runtimeStats.attackRange = value;
                break;

            case StatType.CritChance:
                runtimeStats.critChance = value;
                break;

            case StatType.CritDamage:
                runtimeStats.critDamage = value;
                break;
        }
    }

    protected bool RollCrit()
    {
        return Random.value <= runtimeStats.critChance;
    }

    protected virtual void ShowCritFeedback(int dmg)
    {
        // Incase i wanna add something to the UI later
        Debug.Log($"{name} landed a CRIT for {dmg} damage!");
    }

    public Vector3 GetHeadPosition()
    {
        return collider.bounds.center + Vector3.up * collider.bounds.extents.y;
    }

    public void ClearHealthBarReference()
    {
        healthBar = null;
    }

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, baseStats.attackRange);
    }

    private void OnDestroy()
    {
        if (HealthbarManager.Instance != null)
        {
            HealthbarManager.Instance.ReturnHealthBarToPool(this);
        }

        if (EffectSystem.Instance != null)
        {
            EffectSystem.Instance.RemoveAllEffects(this);
        }
    }

#if !UNITY_EDITOR
    //TODO create world bounds instead.
    private void OnBecameInvisible()
    {
        Die();
    }
#endif

#if UNITY_EDITOR
    private void SyncDebugStats()
    {
        if (runtimeStats == null) return;

        debugCost = runtimeStats.cost;

        debugMaxHealth = runtimeStats.maxHealth;
        debugCurrentHealth = currentHealth;
        debugArmor = runtimeStats.armor;

        debugAttackDamage = runtimeStats.attackDamage;
        debugAttackSpeed = runtimeStats.attackSpeed;
        debugAttackRange = runtimeStats.attackRange;
        debugHitRadius = runtimeStats.hitRadius;

        debugMovementSpeed = runtimeStats.movementSpeed;

        debugCritChance = runtimeStats.critChance;
        debugCritDamage = runtimeStats.critDamage;

        debugUnitPrio = runtimeStats.unitPrio;
        debugIsTargetable = runtimeStats.isTargetable;
    }
#endif
}
