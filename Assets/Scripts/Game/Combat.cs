using UnityEngine;

public class Combat : MonoBehaviour
{
    //[Header("debug")]
    //[SerializeField] private GameObject debugTarget = null;

    [Header("References")]
    [SerializeField] private FindTarget findTarget;

    private IUnit unit;
    private ITargetable target;
    private MovementManager movement;
    private float attackCooldown;

    private void Awake()
    {
        unit = GetComponent<IUnit>();
        findTarget = GetComponent<FindTarget>();
        movement = GetComponent<MovementManager>();
    }
    //look into only looking for targets in the player zone
    private void Update()
    {
        if (attackCooldown > 0) 
        { 
            attackCooldown -= Time.deltaTime;
        }

        if (findTarget != null)
        {
            var currentTarget = findTarget.GetCurrentTarget();
            if (currentTarget != target || target == null || !target.GetIsAlive())
            {
                target = currentTarget;
            }
        }

        if (target != null && target.GetIsAlive())
        {
            Transform t = target.GetTransform();
            if (t != null)
            {
                float dist = Vector2.Distance(transform.position, target.GetTransform().position);
                float effectiveRange = unit.GetAttackRange() + target.GetHitRadius();

                if (dist <= effectiveRange && attackCooldown <= 0)
                {
                    attackCooldown = 1f / unit.GetAttackSpeed();

                    if(unit is RangerStats)
                    {
                        (unit as RangerStats).Shoot(target);
                    }
                    else if(unit is TowerStats)
                    {
                        (unit as TowerStats).Shoot(target);
                    }
                    else
                    {
                        target.TakeDamage(unit.GetAttackDamage());
                    }
                }
                if (movement != null)
                {
                    movement.canMove = dist > unit.GetAttackRange();
                }
            }
        }

    }
    public void ApplyProjectileDamage(ITargetable target, int damage)
    {
        if (target != null && target.GetIsAlive())
        {
            target.TakeDamage(damage);
        }
    }
}

