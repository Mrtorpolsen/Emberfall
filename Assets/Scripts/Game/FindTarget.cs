using System.Linq;
using UnityEngine;

public class FindTarget : MonoBehaviour
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

        currentTarget = GetEnemyCastle(selfUnit.GetTeam());
    }

    void Update()
    {

        FindClosestTarget();
        debugTarget = currentTarget?.GetTransform()?.gameObject;

    }

    private void FindClosestTarget()
    {
        var allTargets = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<ITargetable>();

        float closestDistance = Mathf.Infinity;
        ITargetable nearestEnemy = null;

        foreach (var target in allTargets)
        {
            if (target.GetTeam() == selfUnit.GetTeam())
            {
                continue;
            }

            Transform t = target.GetTransform();
            if (t != null)
            {

                float dist = Vector2.Distance(transform.position, target.GetTransform().position);
                if (dist < closestDistance && dist <= detectionRange)
                {
                    closestDistance = dist;
                    nearestEnemy = target;
                }
            }
        }
        currentTarget = nearestEnemy;
    }
    private ITargetable GetEnemyCastle(Team selfTeam)
    {
        if (selfUnit == null) return GameManager.main.north.GetComponent<ITargetable>();

        return currentTarget = (selfUnit.GetTeam() == Team.North)
           ? GameManager.main.south.GetComponent<ITargetable>()
           : GameManager.main.north.GetComponent<ITargetable>();
    }
    public ITargetable GetCurrentTarget() => currentTarget;
}