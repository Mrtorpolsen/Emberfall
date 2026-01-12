using UnityEngine;

public class CombatManager : MonoBehaviour
{
    //[Header("debug")]
    //[SerializeField] private GameObject debugTarget = null;

    [Header("References")]
    [SerializeField] private TargetManager findTarget;

    private IUnit unit;
    private ITargetable target;
    private MovementManager movement;
    private float attackCooldown;

    private void Awake()
    {
        unit = GetComponent<IUnit>();
        findTarget = GetComponent<TargetManager>();
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
            if (currentTarget != target || target == null || !target.IsAlive)
            {
                target = currentTarget;
            }
        }

        if (target != null && target.IsAlive)
        {
            Transform t = target.Transform;
            if (t != null)
            {
                float dist = Vector2.Distance(transform.position, target.Transform.position);
                float effectiveRange = unit.AttackRange + target.HitRadius;

                if (dist <= effectiveRange && attackCooldown <= 0)
                {
                    attackCooldown = 1f / unit.AttackSpeed;

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
                        target.TakeDamage(unit.AttackDamage);
                    }
                }
                if (movement != null)
                {
                    movement.canMove = dist > unit.AttackRange;
                }
            }
        }
    }

    public void ApplyProjectileDamage(ITargetable target, int damage)
    {
        if (target != null && target.IsAlive)
        {
            target.TakeDamage(damage);
        }
    }
}

