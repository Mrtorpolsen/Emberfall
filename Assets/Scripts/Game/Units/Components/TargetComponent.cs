using System.Linq;
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

    private float detectionRange = 4f;
    private IUnit selfUnit;

    private void Awake()
    {
        selfUnit = GetComponent<IUnit>();
        Debug.Assert(selfUnit != null, "TargetComponent requires an IUnit.");
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

        float closestSqrDistance = float.PositiveInfinity;
        ITargetable nearestEnemy = null;
        Vector2 selfPos = transform.position;

        foreach (var hit in hits)
        {
            if (!hit.TryGetComponent<ITargetable>(out var target))
                continue;

            if (hit.GetComponent<TowerStatsBaseStatsComponent>() != null)
                continue;

            if (target.Team == selfUnit.Team)
                continue;

            float sqrDist = (selfPos - (Vector2)hit.transform.position).sqrMagnitude;
            if (sqrDist < closestSqrDistance)
            {
                closestSqrDistance = sqrDist;
                nearestEnemy = target;
            }
        }

        currentTarget = nearestEnemy;
    }


    public ITargetable GetCurrentTarget() => currentTarget;
}