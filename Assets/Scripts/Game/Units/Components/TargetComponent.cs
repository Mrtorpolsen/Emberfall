using System;
using System.Collections.Generic;
using UnityEngine;

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
    private float detectionRange = 2f;
    private IUnit selfUnit;

    private ITargetingAgent targetingAgent;
    private IReadOnlyList<ThreatLevel> priorities;

    private LayerMask enemyLayer;

    private readonly Dictionary<ThreatLevel, int> priorityIndex = new Dictionary<ThreatLevel, int>();

    List<ITargetable> possibleTargets = new List<ITargetable>();

    private Collider2D[] hitBuffer;
    private ContactFilter2D contactFilter;

    private void Awake()
    {
        selfUnit = GetComponent<IUnit>();
        Debug.Assert(selfUnit != null, "TargetComponent requires an IUnit.");

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

        enemyLayer = selfUnit.Team == Team.South ? northTeamLayer : southTeamLayer;

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
        if (!IsTargetStillValid())
        {
            FindClosestTarget();
            return;
        }

        if (priorities != null)
        {
            TryPriorityOverride();
        }

#if UNITY_EDITOR
        debugTarget = currentTarget?.Transform;
#endif
    }

    private void FindClosestTarget()
    {
        possibleTargets.Clear();

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

            possibleTargets.Add(target);
        }

        Vector2 selfPos = transform.position;

        currentTarget = targetSelector.SelectTarget(possibleTargets, selfPos, priorities);
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
            return false;

        if (!priorityIndex.TryGetValue(currentPrio, out int currentIndex))
            return true;

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

    public ITargetable GetCurrentTarget() => currentTarget;
}