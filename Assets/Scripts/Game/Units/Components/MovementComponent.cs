using UnityEngine;

public class MovementComponent : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;

    [Header("Attributes")]
    [SerializeField] public bool canMove = true;

    [Header("Ranged Settings")]
    [SerializeField] private float rangedZoneRadius = 0.1f; // how far they can move from rally

    private float stopBuffer = 0.1f; // extend the range to better detect when to stop moving

    private Transform south;
    private TargetComponent targetComponent;
    private IUnit unit;
    private UnitMetadata unitMetadata;
    private RangedShooter rangedStats;
    private Transform rangedRally;

    private Collider2D[] hitBuffer = new Collider2D[8];
    private ContactFilter2D separationFilter;

    void Awake()
    {
        targetComponent = GetComponent<TargetComponent>();
        unit = GetComponent<IUnit>();
        unitMetadata = GetComponent<UnitMetadata>();
        rangedStats = GetComponent<RangedShooter>();
        south = GameManager.Instance.south;
        rangedRally = GameManager.Instance.GetNextRangedRally();

        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        separationFilter = new ContactFilter2D
        {
            useLayerMask = false,
            useTriggers = true
        };
    }

    private void FixedUpdate()
    {
        if (!canMove)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2? destination = ResolveDestination();
        if (!destination.HasValue)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 target = destination.Value;

        target = ApplyTeamBoundary(target);
        target = ApplyRangedZoneBoundary(target);

        bool hasCombatTarget = targetComponent?.GetCurrentTarget() != null;

        if (!hasCombatTarget)
        {
            target = ApplySeparation(target);
        }

        MoveToward(target);
    }

    public Vector2? ResolveDestination()
    {
        ITargetable target = targetComponent?.GetCurrentTarget();
        Vector2 currentPos = rb.position;

        bool isRanged = rangedStats != null;

        if (isRanged)
        {
            Vector2 zoneCenter = rangedRally.position;

            if (target != null && target.IsAlive)
            {
                // Stay inside the zone, move only slightly to avoid clustering
                float distanceSqr = (currentPos - zoneCenter).sqrMagnitude;
                float radiusSqr = rangedZoneRadius * rangedZoneRadius;

                if (distanceSqr > radiusSqr)
                {
                    // Pull back toward zone if outside
                    return zoneCenter;
                }
                else
                {
                    // Stay in place and attack
                    return currentPos;
                }
            }

            // No target move toward rally if not already inside
            if (Vector2.Distance(currentPos, zoneCenter) > 0.1f)
            {
                return zoneCenter;
            }

            return null;
        }

        // Melee units: move toward target or fallback
        if (target != null && target.IsAlive)
        {
            float stopRange = unit.AttackRange + stopBuffer;

            float distance = Vector2.Distance(
                rb.position,
                target.Transform.position
            );

            if (distance <= stopRange)
            {
                rb.linearVelocity = Vector2.zero;
                return null;
            }

            // Otherwise keep chasing
            return target.Transform.position;
        }

        Transform fallback = GetFallbackPoint();

        return fallback?.position;
    }

    private void MoveToward(Vector2 target)
    {
        Vector2 toTarget = target - rb.position;
        float sqrDist = toTarget.sqrMagnitude;

        if (sqrDist < 0.0001f)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        rb.linearVelocity = toTarget.normalized * unit.MovementSpeed;
    }

    private Transform GetFallbackPoint()
    {
        bool isRanged = rangedStats != null;

        if (unitMetadata.Team == Team.North)
        {
            //North team move to south castle
            return south;
        }
        else
        {
            if (isRanged)
            {
                return rangedRally;
            }

            return GameManager.Instance.playerUnitBoundary;
        }
    }


    private Vector2 ApplyTeamBoundary(Vector2 target)
    {
        if (unitMetadata.Team != Team.South)
            return target;

        float maxY = GameManager.Instance.playerUnitBoundary.transform.position.y;
        target.y = Mathf.Min(target.y, maxY);

        return target;
    }

    private Vector2 ApplyRangedZoneBoundary(Vector2 target)
    {
        if (rangedStats == null || rangedRally == null)
            return target;

        Vector2 center = rangedRally.position;

        Vector2 offset = target - center;
        float maxRadius = rangedZoneRadius;

        if (offset.sqrMagnitude > maxRadius * maxRadius)
        {
            return center + offset.normalized * maxRadius;
        }

        return target;
    }

    private Vector2 ApplySeparation(Vector2 target)
    {
        Vector2 offset = GetSeparationOffset();

        if (offset == Vector2.zero)
            return target;

        float strength = 0.4f;

        return target + offset * strength;
    }

    private Vector2 GetSeparationOffset()
    {
        float separationRadius = 0.1f;

        int count = Physics2D.OverlapCircle(
            transform.position,
            separationRadius,
            separationFilter,
            hitBuffer
        );

        Vector2 offset = Vector2.zero;

        for (int i = 0; i < count; i++)
        {
            Collider2D col = hitBuffer[i];

            if (col == null || col.gameObject == gameObject)
                continue;

            if (!col.TryGetComponent<UnitMetadata>(out var other))
                continue;

            if (other.Team != unitMetadata.Team)
                continue;

            Vector2 away = (Vector2)(transform.position - col.transform.position);
            float sqrMag = away.sqrMagnitude;

            if (sqrMag > 0.0001f)
            {
                offset += away / sqrMag;
            }
        }

        return offset;
    }
}
