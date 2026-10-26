using UnityEngine;

public class TowerProjectileBaseStatsComponent : MonoBehaviour, IProjectile
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    private RangedShooter shooter;


    [Header("Attributes")]
    [SerializeField] private float speed = 7.5f;
    [SerializeField] private int damage;


    private Transform target;

    private float timeAlive;

    private void Update()
    {
        // Without a target the projectile keeps flying straight, so it needs a lifetime
        timeAlive += Time.deltaTime;
        if (timeAlive >= 5f || !PlayArea.Contains(transform.position))
        {
            Destroy(gameObject);
        }
    }

    public void Init(RangedShooter owner, int _damage)
    {
        shooter = owner;
        damage = _damage;
    }

    public void SetTarget(ITargetable _target)
    {
        target = _target.Transform;
    }

    private void FixedUpdate()
    {
        if (!target) return;

        Vector2 direction = (target.position - transform.position).normalized;

        rb.linearVelocity = direction * speed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        var targetable = collision.gameObject.GetComponent<ITargetable>();

        if (targetable != null && targetable.IsAlive)
        {
            RangedShooter.ApplyHit(shooter, targetable, damage, transform.position);
            Destroy(gameObject);
        }
    }
}
