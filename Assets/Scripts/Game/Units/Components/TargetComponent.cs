using System.Linq;
using UnityEngine;

public class TargetComponent : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private float detectionRange = 4f;
    [SerializeField] private ITargetable currentTarget;

    [Header("Debug")]
    [SerializeField] private Transform debugTarget;

    private IUnit selfUnit;

    private void Awake()
    {
        selfUnit = GetComponent<IUnit>();
    }

    void Update()
    {
        FindClosestTarget();
        debugTarget = currentTarget?.Transform;
    }

    private void FindClosestTarget()
    {
        var allTargets = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<ITargetable>();

        float closestDistance = Mathf.Infinity;
        ITargetable nearestEnemy = null;

        foreach (var target in allTargets)
        {
            if (target.Team == selfUnit.Team)
            {
                continue;
            }

            Transform t = target.Transform;
            if (t != null)
            {

                float dist = Vector2.Distance(transform.position, target.Transform.position);
                if (dist < closestDistance && dist <= detectionRange)
                {
                    closestDistance = dist;
                    nearestEnemy = target;
                }
            }
        }
        currentTarget = nearestEnemy;
    }

    public ITargetable GetCurrentTarget() => currentTarget;
}