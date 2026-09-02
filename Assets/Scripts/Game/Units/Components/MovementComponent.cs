using UnityEngine;

public class MovementComponent : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;

    [Header("Attributes")]
    [SerializeField] public bool canMove = true;

    [Header("Ranged Settings")]
    [SerializeField] private float rangedZoneRadius = 0.1f;

    [Header("Movement")]
    [SerializeField] private float arrivalRadius = 0.3f;
    [SerializeField] private float separationRadius = 0.1f;
    [SerializeField] private float separationStrength = 0.1f;

    [Header("Stuck Detection")]
    [SerializeField] private float stuckTime = 1f;
    [SerializeField] private float retryDelay = 2f;
    [SerializeField] private float progressThreshold = 0.05f;

    private float stopBuffer = 0.1f;

    private Transform south;
    private TargetComponent targetComponent;
    private IUnit unit;
    private UnitMetadata unitMetadata;
    private RangedShooter rangedStats;
    private Transform meleeRally;
    private Transform rangedRally;

    private readonly Collider2D[] hitBuffer = new Collider2D[8];
    private ContactFilter2D separationFilter;

    private float fallbackXOffSet = 0.85f;
    private float unitFallbackXOffSet;

    private Vector2 lastProgressPosition;
    private float stuckTimer;
    private float retryTimer;

    private bool hasTemporaryPosition;
    private Vector2 temporaryPosition;

    private bool hasForcedDestination;
    private Vector2 forcedDestination;

#if UNITY_EDITOR
    [Header("Debug")]
    [SerializeField] private Vector2 debugDestination;
#endif

    void Awake()
    {
        targetComponent = GetComponent<TargetComponent>();
        unit = GetComponent<IUnit>();
        unitMetadata = GetComponent<UnitMetadata>();
        rangedStats = GetComponent<RangedShooter>();

        south = GameManager.Instance.south;
        rangedRally = GameManager.Instance.GetNextRangedRally();
        meleeRally = GameManager.Instance.meleeRally;

        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        separationFilter = new ContactFilter2D
        {
            useLayerMask = false,
            useTriggers = true
        };

        unitFallbackXOffSet = Random.Range(-fallbackXOffSet, fallbackXOffSet);

        lastProgressPosition = rb.position;
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
            ResetStuckDetection();
            return;
        }

        Vector2 target = destination.Value;

        target = ApplyTeamBoundary(target);
        target = ApplyRangedZoneBoundary(target);

        UpdateStuckDetection(target);

#if UNITY_EDITOR
        debugDestination = target;
#endif

        MoveToward(target);

        //Debug.DrawRay(transform.position, rb.linearVelocity, Color.green);
    }

    public Vector2? ResolveDestination()
    {
        ITargetable target = targetComponent?.GetCurrentTarget();
        Vector2 currentPos = rb.position;

        bool isRanged = rangedStats != null;

        if (hasForcedDestination)
        {
            if ((forcedDestination - rb.position).sqrMagnitude <= arrivalRadius * arrivalRadius)
            {
                SetMovementEnabled(false);
                return null;
            }

            return forcedDestination;
        }

        if (isRanged)
        {
            Vector2 zoneCenter = rangedRally.position;

            if (target != null && target.IsAlive)
            {
                if ((currentPos - zoneCenter).sqrMagnitude > rangedZoneRadius * rangedZoneRadius)
                    return zoneCenter;

                return null;
            }

            if ((currentPos - zoneCenter).sqrMagnitude > arrivalRadius * arrivalRadius)
                return zoneCenter;

            return null;
        }

        if (target != null && target.IsAlive)
        {
            float stopRange = unit.AttackRange + stopBuffer;

            if (Vector2.Distance(currentPos, target.Transform.position) <= stopRange)
                return null;

            return target.Transform.position;
        }

        if (hasTemporaryPosition)
        {
            retryTimer -= Time.fixedDeltaTime;

            if (retryTimer > 0)
                return temporaryPosition;

            hasTemporaryPosition = false;
        }

        Transform fallback = GetFallbackPoint();

        if (fallback == null)
            return null;

        Vector2 destination =
            (Vector2)fallback.position +
            Vector2.right * unitFallbackXOffSet;

        if ((destination - currentPos).sqrMagnitude <= arrivalRadius * arrivalRadius)
            return null;

        return destination;
    }

    private void MoveToward(Vector2 target)
    {
        Vector2 desired = target - rb.position;

        if (desired.sqrMagnitude < 0.0001f)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        desired.Normalize();

        if (targetComponent?.GetCurrentTarget() == null)
        {
            desired += GetSeparationOffset(target) * separationStrength;

            if (desired.sqrMagnitude > 0.0001f)
                desired.Normalize();
        }

        rb.linearVelocity = desired * unit.MovementSpeed;
    }

    private void UpdateStuckDetection(Vector2 target)
    {
        if (targetComponent?.GetCurrentTarget() != null)
        {
            ResetStuckDetection();
            return;
        }

        float movedDistance = Vector2.Distance(rb.position, lastProgressPosition);

        if (movedDistance >= progressThreshold)
        {
            lastProgressPosition = rb.position;
            stuckTimer = 0f;
            return;
        }

        stuckTimer += Time.fixedDeltaTime;

        if (stuckTimer >= stuckTime && !hasTemporaryPosition)
        {
            hasTemporaryPosition = true;
            temporaryPosition = rb.position;
            retryTimer = retryDelay;

            rb.linearVelocity = Vector2.zero;
        }
    }

    private void ResetStuckDetection()
    {
        stuckTimer = 0f;
        lastProgressPosition = rb.position;
    }

    private Transform GetFallbackPoint()
    {
        bool isRanged = rangedStats != null;

        if (unitMetadata.Team == Team.North)
            return south;

        if (isRanged)
            return rangedRally;

        return meleeRally;
    }

    private Vector2 ApplyTeamBoundary(Vector2 target)
    {
        if (unitMetadata.Team != Team.South)
            return target;

        float maxY = meleeRally.position.y;
        target.y = Mathf.Min(target.y, maxY);

        return target;
    }

    private Vector2 ApplyRangedZoneBoundary(Vector2 target)
    {
        if (rangedStats == null || rangedRally == null)
            return target;

        Vector2 center = rangedRally.position;
        Vector2 offset = target - center;

        if (offset.sqrMagnitude > rangedZoneRadius * rangedZoneRadius)
            return center + offset.normalized * rangedZoneRadius;

        return target;
    }

    private Vector2 GetSeparationOffset(Vector2 target)
    {
        int count = Physics2D.OverlapCircle(
            transform.position,
            separationRadius,
            separationFilter,
            hitBuffer);

        Vector2 offset = Vector2.zero;

        Vector2 moveDirection = (target - rb.position).normalized;

        for (int i = 0; i < count; i++)
        {
            Collider2D col = hitBuffer[i];

            if (col == null || col.gameObject == gameObject)
                continue;

            if (!col.TryGetComponent(out UnitMetadata other))
                continue;

            if (other.Team != unitMetadata.Team)
                continue;

            Vector2 toOther =
                ((Vector2)col.transform.position - rb.position).normalized;

            if (Vector2.Dot(moveDirection, toOther) < 0f)
                continue;

            Vector2 away = rb.position - (Vector2)col.transform.position;
            float sqrMag = away.sqrMagnitude;

            if (sqrMag > 0.0001f)
            {
                float distance = Mathf.Sqrt(sqrMag);
                float strength = 1f - (distance / separationRadius);

                offset += away.normalized * strength;
            }
        }

        return offset;
    }

    public void SetTemporaryDestination(Vector2 destination)
    {
        forcedDestination = destination;
        hasForcedDestination = true;
        canMove = true;
    }

    public void ClearTemporaryDestination()
    {
        hasForcedDestination = false;
    }

    public void SetMovementEnabled(bool enabled)
    {
        canMove = enabled;

        if (!enabled)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }
}