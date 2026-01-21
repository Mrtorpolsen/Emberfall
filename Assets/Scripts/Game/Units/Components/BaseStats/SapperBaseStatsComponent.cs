using System.Linq;
using UnityEngine;

public class SapperBaseStatsComponent : BaseUnitStats
{
    [SerializeField] private float explosionRadius = 1.5f;


    public void Explode()
    {
        float radiusSquare = explosionRadius * explosionRadius;
        Vector2 selfPos = transform.position;

        var targets = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<ITargetable>();

        foreach(var target in targets)
        {
            if(target.Team == metadata.Team) continue;

            Transform t = target.Transform;

            if(t == null) continue;

            float squareDist = (selfPos - (Vector2)t.position).sqrMagnitude;
            if(squareDist < radiusSquare)
            {
                target.TakeDamage(attackDamage);
            }
        }
        Debug.Log("Explosion boom");
        Destroy(gameObject);
    }
    protected override void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}