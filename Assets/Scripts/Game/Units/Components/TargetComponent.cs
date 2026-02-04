using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using Unity.Profiling;
#endif

public class TargetComponent : MonoBehaviour
{
    [Header("Reference")]
    private ITargetable currentTarget;

    [SerializeField] private float retargetInterval = 0.2f;
    private float retargetTimer;

    [SerializeField] private LayerMask northTeamLayer;
    [SerializeField] private LayerMask southTeamLayer;

#if UNITY_EDITOR
    [SerializeField] private Transform debugTarget;
#endif

    private TargetSelector targetSelector;
    private float detectionRange = 1f;
    private IUnit selfUnit;

    private ITargetingAgent targetingAgent;
    private IReadOnlyList<ThreatLevel> priorities;

    private LayerMask enemyLayer;

    private readonly Dictionary<ThreatLevel, int> priorityIndex = new Dictionary<ThreatLevel, int>();

    List<ITargetable> possibleTargets = new List<ITargetable>();

    private Collider2D[] hitBuffer;
    private ContactFilter2D contactFilter;

#if UNITY_EDITOR
    static readonly ProfilerMarker UpdateMarker =
        new ProfilerMarker("TargetComponent.Update");

    static readonly ProfilerMarker FindTargetMarker =
        new ProfilerMarker("TargetComponent.FindTarget");

    static readonly ProfilerMarker OverlapMarker =
        new ProfilerMarker("TargetComponent.OverlapCircle");
#endif

    private void Awake()
    {
        selfUnit = GetComponent<IUnit>();

        if(selfUnit.AttackRange > detectionRange)
        {
            detectionRange = selfUnit.AttackRange;
        }

        targetSelector = new TargetSelector();

        targetingAgent = GetComponent<ITargetingAgent>();
        priorities = targetingAgent?.PreferredPriorities;

        if (priorities != null)
        {
            SetPriorities(priorities);
        }
    }

    private void Start()
    {
        enemyLayer = GetEnemyLayer(selfUnit.Team);

        int bufferSize;

        if (selfUnit.AttackRange <= 2f)          // melee
            bufferSize = 8;
        else if (selfUnit.AttackRange <= 5f)       // short range
            bufferSize = 16;
        else                                       // long range
            bufferSize = 32;

        hitBuffer = new Collider2D[bufferSize];

        contactFilter = new ContactFilter2D
        {
            useLayerMask = true,
            layerMask = enemyLayer,
            useTriggers = true
        };
    }
    
    void Update()
    {
#if  UNITY_EDITOR
        using (UpdateMarker.Auto())
#endif
        {
            retargetTimer -= Time.deltaTime;
            if (retargetTimer > 0f)
                return;

            retargetTimer = retargetInterval;

            if (!IsTargetStillValid())
            {
                FindClosestTarget();
                return;
            }

            if (priorities != null)
            {
                TryPriorityOverride();
            }
        }

#if UNITY_EDITOR
        debugTarget = currentTarget?.Transform;
#endif
    }

    private void FindClosestTarget()
    {
#if UNITY_EDITOR
        using (FindTargetMarker.Auto())
#endif
        {
            possibleTargets.Clear();
            int hitCount;
#if UNITY_EDITOR
            using (OverlapMarker.Auto())
#endif
            {
                hitCount = Physics2D.OverlapCircle(
                    transform.position,
                    detectionRange,
                    contactFilter,
                    hitBuffer
                );
            }
                for (int i = 0; i < hitCount; i++)
                {
                    Collider2D hit = hitBuffer[i];

                    if (!hit.TryGetComponent<ITargetable>(out var target))
                        continue;

                    if (!target.IsTargetable)
                        continue;

                    if (target.Team == selfUnit.Team)
                        continue;

                    possibleTargets.Add(target);
                }
                Vector2 selfPos = transform.position;

                currentTarget = targetSelector.SelectTarget(possibleTargets, selfPos, priorities);
        }
    }

    private void TryPriorityOverride()
    {
        if (currentTarget == null) return;

        ThreatLevel currentThreat = currentTarget.UnitPrio;

        int hitCount = Physics2D.OverlapCircle(
            transform.position,
            detectionRange,
            contactFilter,
            hitBuffer
        );

        for (int i = 0; i < hitCount; i++)
        {
            Collider2D hit = hitBuffer[i];

            if (!hit.TryGetComponent<ITargetable>(out var target))
                continue;

            if (!target.IsTargetable)
                continue;

            if (target.Team == selfUnit.Team)
                continue;

            if (IsHigherPriority(target.UnitPrio, currentThreat))
            {
                currentTarget = target;
                return;
            }
        }
    }

    private bool IsHigherPriority(ThreatLevel targetPrio, ThreatLevel currentPrio)
    {
        if (!priorityIndex.TryGetValue(targetPrio, out int targetIndex))
        {
            return false;
        }

        if (!priorityIndex.TryGetValue(currentPrio, out int currentIndex))
        {
            return true;
        }

        return targetIndex < currentIndex;
    }

    private bool IsTargetStillValid()
    {
        if (currentTarget == null) return false;
        if (!currentTarget.IsAlive) return false;

        float sqrDist = (currentTarget.Transform.position - transform.position).sqrMagnitude;

        return sqrDist <= detectionRange * detectionRange;
    }

    public void SetPriorities(IReadOnlyList<ThreatLevel> priorities)
    {
        priorityIndex.Clear();
        for (int i = 0; i < priorities.Count; i++)
        {
            priorityIndex[priorities[i]] = i;
        }
    }

    private LayerMask GetEnemyLayer(Team myTeam)
    {
        if (myTeam == Team.North)
            return LayerMask.GetMask("SouthTeam");
        else
            return LayerMask.GetMask("NorthTeam");
    }

    public ITargetable GetCurrentTarget() => currentTarget;
}