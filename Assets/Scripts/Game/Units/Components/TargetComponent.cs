using System.Collections.Generic;
using UnityEngine;

public class TargetComponent : MonoBehaviour
{
    [Header("Reference")]
    private ITargetable currentTarget;

    [SerializeField] private float retargetInterval = 0.25f;
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
    }

    void Update()
    {
        //retargetTimer -= Time.deltaTime;
        //if (retargetTimer <= 0f)
        //{
        //    retargetTimer = retargetInterval;
        //    FindClosestTarget();
        //}

        if (!IsTargetStillValid())
        {
            FindClosestTarget();
            return;
        }

#if UNITY_EDITOR
        debugTarget = currentTarget?.Transform;
#endif
    }

    private void FindClosestTarget()
    {
        LayerMask enemyLayer = selfUnit.Team == Team.South ? northTeamLayer : southTeamLayer;

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            detectionRange,
            enemyLayer
        );

        Vector2 selfPos = transform.position;

        List<ITargetable> possibleTargets = new List<ITargetable>();
        
        foreach (var hit in hits)
        {
            if (!hit.TryGetComponent<ITargetable>(out var target))
                continue;

            if (hit.GetComponent<TowerStatsBaseStatsComponent>() != null)
                continue;

            if (hit.GetComponent<BalistaTowerStatsBaseStatsComponent>() != null)
                continue;

            if (target.Team == selfUnit.Team)
                continue;

            possibleTargets.Add(target);
        }

        currentTarget = targetSelector.SelectTarget(possibleTargets, selfPos, priorities);
    }

    private bool IsTargetStillValid()
    {
        if (currentTarget == null) return false;
        if (!currentTarget.IsAlive) return false;

        float sqrDist = (currentTarget.Transform.position - transform.position).sqrMagnitude;

        return sqrDist <= detectionRange * detectionRange;
    }

    public ITargetable GetCurrentTarget() => currentTarget;
}