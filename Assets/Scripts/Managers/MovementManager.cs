using UnityEngine;

public class MovementManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;

    [Header("Attributes")]
    [SerializeField] public bool canMove;

    private FindTarget findTarget;
    
    void Awake()
    {
        findTarget = GetComponent<FindTarget>();
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }
    
    private void FixedUpdate()
    {
        if (!canMove)
        {
            rb.constraints = RigidbodyConstraints2D.FreezePositionY;
        } else
        {
            rb.constraints = RigidbodyConstraints2D.None;
        }

        if (findTarget == null) return;

        ITargetable target = findTarget.GetCurrentTarget();

        if (target != null)
        {
            Transform t = target.GetTransform();

            if (t != null)
            {
                Vector2 direction = (target.GetTransform().position - transform.position).normalized;
                rb.linearVelocity = direction * rb.GetComponent<IUnit>().GetMovementSpeed();
            }

            //Sets Y barrier
            if (rb.GetComponent<UnitStats>().Team == Team.South)
            {
                Vector2 pos = rb.position;
                pos.y = Mathf.Min(pos.y, GameManager.main.playerUnitBoundary.transform.position.y);
                rb.position = pos;
            }
        }

        SeperationForce();
    }
    
    private void SeperationForce()
    {
        float separationRadius = 0.1f;
        float pushStrength = 0.4f;

        Collider2D[] nearbyUnits = Physics2D.OverlapCircleAll(transform.position, separationRadius);
        Vector2 push = Vector2.zero;

        foreach (var unit in nearbyUnits)
        {
            if(unit == gameObject) continue;

            if(unit.TryGetComponent<UnitStats>(out var otherUnit) && otherUnit.Team == rb.GetComponent<UnitStats>().Team)
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
