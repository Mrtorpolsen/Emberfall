using UnityEngine;

public class CombatComponent : MonoBehaviour
{
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

            if (unit is RangedUnitStats)
            {
                (unit as RangedUnitStats).Shoot(target);
            }
            else if (unit is SapperBaseStatsComponent)
            {
                (unit as SapperBaseStatsComponent).Explode();
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

    public void ApplyProjectileDamage(ITargetable target, int damage, GameObject projObj)
    {
        if (target != null && target.IsAlive)
        {
            if (unit is BombTowerStatsBaseStatsComponent)
            {
                (unit as BombTowerStatsBaseStatsComponent).Explode(projObj);
            } 
            else
            {
                target.TakeDamage(damage);
            }
        }
    }
}

