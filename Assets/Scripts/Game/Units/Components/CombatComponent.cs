using UnityEngine;

public class CombatComponent : MonoBehaviour
{
    //[Header("debug")]
    //[SerializeField] private GameObject debugTarget = null;

    [Header("References")]
    [SerializeField] private TargetComponent targetComponent;

    private BaseUnitStats unit;
    private ITargetable target;
    private MovementComponent movement;
    private float attackCooldown;

    private void Awake()
    {
        unit = GetComponent<BaseUnitStats>();
        targetComponent = GetComponent<TargetComponent>();
        movement = GetComponent<MovementComponent>();
    }

    private void Update()
    {
        if (attackCooldown > 0) 
        { 
            attackCooldown -= Time.deltaTime;
        }

        if (targetComponent != null)
        {
            var currentTarget = targetComponent.GetCurrentTarget();

            if (currentTarget != target || target == null || !target.IsAlive)
            {
                target = currentTarget;
            }
        }

        if (target != null && target.IsAlive)
        {
            HandleCombat();
        }
        else
        {
            if (movement != null)
            {
                movement.canMove = true;
            }
        }
    }

    private void HandleCombat()
    {
        Transform t = target.Transform;
        if (t == null) return;

        float dist = Vector2.Distance(transform.position, target.Transform.position);
        float effectiveRange = unit.AttackRange + target.HitRadius;

        if (dist <= effectiveRange && attackCooldown <= 0)
        {
            attackCooldown = 1f / unit.AttackSpeed;

            if (unit is RangerStats)
            {
                (unit as RangerStats).Shoot(target);
            }
            else if (unit is TowerStats)
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
            movement.canMove = dist > unit.AttackRange;
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

