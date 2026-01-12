using System.Linq;
using UnityEngine;

public class TargetComponent : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private float detectionRange = 100f;
    [SerializeField] private ITargetable currentTarget;

    [Header("Debug")]
    [SerializeField] private GameObject debugTarget;

    private IUnit selfUnit;

    private void Awake()
    {
        selfUnit = GetComponent<IUnit>();

        currentTarget = GetDefaultTarget(selfUnit.Team);
    }

    void Update()
    {

        FindClosestTarget();
        debugTarget = currentTarget?.Transform?.gameObject;

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

    private ITargetable GetDefaultTarget(Team selfTeam)
    {
        if (selfUnit == null) return GameManager.Instance.north.GetComponent<ITargetable>();

        return currentTarget = (selfUnit.Team == Team.North)
           ? GameManager.Instance.south.GetComponent<ITargetable>()
           : GameManager.Instance.north.GetComponent<ITargetable>();
    }
    public ITargetable GetCurrentTarget() => currentTarget;
}