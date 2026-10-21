using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UnitMetadata))]
public abstract class BaseUnitStats : MonoBehaviour, IUnit, ITargetable
{
    [Header("Reference")]
    [SerializeField] protected GameObject unit;
    [SerializeField] public UnitStatsDefinition statsDef;
    [SerializeField] private Collider2D unitCollider;
    [SerializeField] private Rigidbody2D unitRigidbody;

    [SerializeField] private float showRangeGizmo1 = 0f;
    [SerializeField] private float showRangeGizmo2 = 0f;

    protected UnitStatsDefinition StatsDef => statsDef;

    private static readonly StatType[] AllStats = (StatType[])Enum.GetValues(typeof(StatType));

    public int currentHealth;
    protected UnitMetadata metadata;
    private RuntimeStats runtimeStats;

    // progression (talents/research, or the definition) -> base (+ permanent changes like tower tier) -> current (+ active modifiers)
    private StatBlock progressionStats;
    private StatBlock baseStats;
    private readonly StatBlock currentStats = new();

    public List<ActiveEffect> ActiveEffects = new();

    private readonly Dictionary<StatType, List<StatModifier>> modifiersByStat = new();
    private readonly HashSet<StatType> dirtyStats = new();

    private FloatingHealthBar healthBar;
    public Vector3 healthBarScale;

    private bool isDying;
    private bool isInvulnerable = false;

    // IUnit
    public float AttackRange => currentStats.Get(StatType.AttackRange);
    public int AttackDamage => currentStats.GetInt(StatType.AttackDamage);
    public float AttackSpeed => currentStats.Get(StatType.AttackSpeed);
    public float MovementSpeed => currentStats.Get(StatType.MovementSpeed);
    public float CritChance => currentStats.Get(StatType.CritChance);
    public float CritDamage => currentStats.Get(StatType.CritDamage);
    public int MaxHealth => currentStats.GetInt(StatType.Health);
    public int Armor => currentStats.GetInt(StatType.Armor);
    public int Mass => currentStats.GetInt(StatType.Mass);
    public float SplashRadius => currentStats.Get(StatType.SplashRadius);

    // ITargetable
    public GameObject GameObject => gameObject;
    public Transform Transform => (this != null) ? transform : null;
    public float HitRadius => runtimeStats.hitRadius;
    public bool IsAlive => !isDying && currentHealth > 0;
    public virtual ThreatLevel UnitPrio => runtimeStats.unitPrio;
    public bool IsTargetable => runtimeStats.isTargetable;
    public bool IsInvulnerable => isInvulnerable;

    // UnitMetadata
    public Team Team => metadata.Team;
    public float Cost => statsDef.cost;


    //Debug
#if UNITY_EDITOR
    [Header("Debug Stats")]
    [SerializeField] private float debugCost;

    [SerializeField] private int debugMaxHealth;
    [SerializeField] private int debugCurrentHealth;
    [SerializeField] private int debugArmor;

    [SerializeField] private int debugAttackDamage;
    [SerializeField] private float debugAttackSpeed;
    [SerializeField] private float debugAttackRange;
    [SerializeField] private float debugHitRadius;
    [SerializeField] private float debugSplashRadius;

    [SerializeField] private float debugMovementSpeed;
    [SerializeField] private int debugMass;

    [SerializeField] private float debugCritChance;
    [SerializeField] private float debugCritDamage;

    [SerializeField] private ThreatLevel debugUnitPrio;
    [SerializeField] private bool debugIsTargetable;
#endif

    protected virtual void Awake()
    {
        runtimeStats = new RuntimeStats
        {
            cost = statsDef.cost,
            hitRadius = statsDef.hitRadius,
            unitPrio = statsDef.unitPrio,
            isTargetable = statsDef.isTargetable
        };

        metadata = GetComponent<UnitMetadata>();

        SetProgressionStats(statsDef.ToStatBlock());

        healthBarScale = statsDef.healthbarScale;
    }

    private void LateUpdate()
    {
        if (dirtyStats.Count == 0) return;

        foreach (var stat in dirtyStats)
        {
            ApplyToRuntime(stat, CalculateStat(stat, baseStats.Get(stat)));
        }

        dirtyStats.Clear();

#if UNITY_EDITOR
        SyncDebugStats();
#endif
    }

    public virtual void TakeDamage(int amount)
    {
        if (IsInvulnerable) return;

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
            if (!wasFull && healthBar != null)
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
        float reductionFactor = 100f / (Armor + 100f);
        return Mathf.Max(1, Mathf.RoundToInt(dmg * reductionFactor));
    }

    public virtual void Die()
    {
        if (isDying) return;
        isDying = true;

        TargetRegistry.Instance.UnregisterUnit(this);

        OnDeath();

        Destroy(unit != null ? unit : gameObject);
    }

    protected virtual void OnDeath() { }

    public virtual void ApplyFinalStats(FinalStats finalStats)
    {
        runtimeStats.cost = finalStats.cost;
        runtimeStats.hitRadius = finalStats.hitRadius;

        SetProgressionStats(finalStats.Stats.Clone());
    }

    private void SetProgressionStats(StatBlock stats)
    {
        progressionStats = stats;
        RebuildBaseStats();
    }

    // Call when something that BuildBaseStats depends on changes (e.g. tower tier)
    protected void RebuildBaseStats()
    {
        baseStats = BuildBaseStats(progressionStats);

        foreach (StatType stat in AllStats)
        {
            ApplyToRuntime(stat, CalculateStat(stat, baseStats.Get(stat)));
        }

#if UNITY_EDITOR
        SyncDebugStats();
#endif
    }

    // Permanent changes on top of progression stats; override for things like tower tiers
    protected virtual StatBlock BuildBaseStats(StatBlock progression)
    {
        return progression.Clone();
    }

    public int GetAttackDamage()
    {
        int dmg = AttackDamage;

        if (CritChance > 0)
        {
            if (RollCrit())
            {
                dmg = Mathf.RoundToInt(dmg * CritDamage);
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

        return StatPipeline.Calculate(baseValue, list);
    }

    private void ApplyToRuntime(StatType stat, float value)
    {
        switch (stat)
        {
            case StatType.Health:
                {
                    // Keep the same health percentage when max health changes (full on first set)
                    float percent = MaxHealth > 0 ? (float)currentHealth / MaxHealth : 1f;

                    currentStats.Set(stat, value);
                    currentHealth = Mathf.RoundToInt(MaxHealth * percent);

                    if (HealthbarManager.Instance != null)
                    {
                        if (healthBar != null)
                        {
                            healthBar.UpdateValue(currentHealth, MaxHealth);
                        }
                    }

                    break;
                }

            case StatType.Mass:
                currentStats.Set(stat, value);

                if (Mass > 0 && unitRigidbody != null)
                {
                    unitRigidbody.mass = Mass;
                }

                break;

            default:
                currentStats.Set(stat, value);
                break;
        }
    }

    protected bool RollCrit()
    {
        return UnityEngine.Random.value <= CritChance;
    }

    protected virtual void ShowCritFeedback(int dmg)
    {
        // Incase i wanna add something to the UI later
        Debug.Log($"{name} landed a CRIT for {dmg} damage!");
    }

    public Vector3 GetHeadPosition()
    {
        return unitCollider.bounds.center + Vector3.up * unitCollider.bounds.extents.y;
    }

    public void ClearHealthBarReference()
    {
        healthBar = null;
    }

    public void SetTargetable(bool isTargetable)
    {
        runtimeStats.isTargetable = isTargetable;
    }

    public void SetInvulnerable(bool isInvulnerable)
    {
        this.isInvulnerable = isInvulnerable;
    }

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, showRangeGizmo1);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, showRangeGizmo2);
    }

    protected virtual void OnDestroy()
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

        debugMaxHealth = MaxHealth;
        debugCurrentHealth = currentHealth;
        debugArmor = Armor;

        debugAttackDamage = AttackDamage;
        debugAttackSpeed = AttackSpeed;
        debugAttackRange = AttackRange;
        debugHitRadius = runtimeStats.hitRadius;
        debugSplashRadius = SplashRadius;

        debugMovementSpeed = MovementSpeed;
        debugMass = Mass;

        debugCritChance = CritChance;
        debugCritDamage = CritDamage;

        debugUnitPrio = runtimeStats.unitPrio;
        debugIsTargetable = runtimeStats.isTargetable;
    }
#endif
}
