using UnityEngine;

public class ArrowBaseStatsComponent : MonoBehaviour, IProjectile
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    private RangedShooter shooter;


    [Header("Attributes")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private int damage;

    private Transform target;

    private float timeAlive;

    private void Update()
    {
        timeAlive += Time.deltaTime;
        if (timeAlive >= 5f || !PlayArea.Contains(transform.position))
        {
            Destroy(gameObject);
        }
    }

    private void FixedUpdate()
    {
        if (!target) return;

        Vector2 direction = (target.position - transform.position).normalized;

        rb.linearVelocity = direction * speed;
    }

    private void OnEnable()
    {
        timeAlive = 0f;
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

    void OnTriggerEnter2D(Collider2D other)
    {
        var targetable = other.gameObject.GetComponent<ITargetable>();

        if (targetable != null && targetable.IsAlive)
        {
            RangedShooter.ApplyHit(shooter, targetable, damage, transform.position);
            Destroy(gameObject);
        }
    }
}
