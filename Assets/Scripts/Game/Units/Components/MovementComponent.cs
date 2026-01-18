using UnityEngine;

public class MovementComponent : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;

    [Header("Attributes")]
    [SerializeField] public bool canMove;

    [Header("Ranged Settings")]
    [SerializeField] private float rangedZoneRadius = 0.1f; // how far they can move from rally

    private Transform south;
    private TargetComponent targetComponent;
    private IUnit unit;
    private UnitMetadata unitMetadata;
    private RangedUnitStats rangedStats;
    private Transform rangedRally;
    
    void Awake()
    {
        targetComponent = GetComponent<TargetComponent>();
        unit = GetComponent<IUnit>();
        unitMetadata = GetComponent<UnitMetadata>();
        rangedStats = GetComponent<RangedUnitStats>();
        south = GameManager.Instance.south;
        rangedRally = GameManager.Instance.GetNextRangedRally();

        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    private void FixedUpdate()
    {
        Vector2? destination = ResolveDestination();

        if (destination.HasValue && canMove)
        {
            Vector2 direction = (destination.Value - rb.position).normalized;
            rb.linearVelocity = direction * unit.MovementSpeed;
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }

        ApplyTeamBoundary();
        SeparationForce();
        ApplyRangedZoneBoundary();
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
                float distanceToZone = Vector2.Distance(currentPos, zoneCenter);
                if (distanceToZone > rangedZoneRadius)
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
            return target.Transform.position;
        }

        Transform fallback = GetFallbackPoint();

        return fallback?.position;
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


    private void ApplyTeamBoundary()
    {
        if (unitMetadata.Team != Team.South)
            return;

        Vector2 pos = rb.position;
        pos.y = Mathf.Min(
            pos.y,
            GameManager.Instance.playerUnitBoundary.transform.position.y
        );
        rb.position = pos;
    }

    private void ApplyRangedZoneBoundary()
    {
        if (rangedStats == null || rangedRally == null) return;

        Vector2 pos = rb.position;
        float distanceToRally = Vector2.Distance(pos, rangedRally.position);

        if (distanceToRally > rangedZoneRadius)
        {
            Vector2 direction = (rangedRally.position - (Vector3)pos).normalized;
            pos += direction * Mathf.Min(unit.MovementSpeed * Time.fixedDeltaTime, distanceToRally);
            rb.position = pos;
        }
    }

    private void SeparationForce()
    {
        float separationRadius = 0.1f;
        float pushStrength = 0.4f;

        Collider2D[] nearbyUnits = Physics2D.OverlapCircleAll(transform.position, separationRadius);
        Vector2 push = Vector2.zero;

        foreach (var unit in nearbyUnits)
        {
            if(unit == gameObject) continue;

            if(unit.TryGetComponent<UnitMetadata>(out var otherUnit) && otherUnit.Team == rb.GetComponent<UnitMetadata>().Team)
            {
                Vector2 away = (Vector2)(transform.position - unit.transform.position);

                if(away.magnitude > 0)
                {
                    push += away.normalized / away.magnitude;
                }
            }
        }

        if (push != Vector2.zero)
        {
            rb.AddForce(push * pushStrength);
        }
    }
}
