using System;
using UnityEngine;

public class TowerProjectile : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] public IUnit tower;


    [Header("Attributes")]
    [SerializeField] private float speed = 7.5f;
    [SerializeField] private int damage;


    private Transform target;
    public event Action<ITargetable, int> OnHit;

    public void Init(IUnit owner, int _damage)
    {
        tower = owner;
        damage = _damage;
    }

    public void SetTarget(ITargetable _target)
    {
        target = _target.GetTransform();
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

        if (targetable != null && targetable.GetIsAlive())
        {
            OnHit?.Invoke(targetable, damage);
            Destroy(gameObject);
        }
    }
    private void OnBecameInvisible()
    {
        Destroy(gameObject);
    }
}
