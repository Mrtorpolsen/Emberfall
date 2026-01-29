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

    private void Awake()
    {
        selfUnit = GetComponent<IUnit>();
        Debug.Assert(selfUnit != null, "TargetComponent requires an IUnit.");

        if(selfUnit.AttackRange > detectionRange)
        {
            detectionRange = selfUnit.AttackRange;
        }

        targetSelector = new TargetSelector();
    }

    void Update()
    {
        retargetTimer -= Time.deltaTime;
        if (retargetTimer <= 0f)
        {
            retargetTimer = retargetInterval;
            FindClosestTarget();
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

        IReadOnlyList<ThreatLevel> priorities = null;

        if (gameObject.TryGetComponent<ITargetingAgent>(out var agent))
        {
            priorities = agent.PreferredPriorities;
        }

        currentTarget = targetSelector.SelectTarget(possibleTargets, selfPos, priorities);
    }


    public ITargetable GetCurrentTarget() => currentTarget;
}