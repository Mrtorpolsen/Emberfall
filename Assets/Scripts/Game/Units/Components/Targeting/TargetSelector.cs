using System.Collections.Generic;
using UnityEngine;

public class TargetSelector
{
    public ITargetable SelectTarget(List<ITargetable> possibleTargets, Vector2 selfPos,
        IReadOnlyList<ThreatLevel> prioList = null)
    {
        if (possibleTargets == null || possibleTargets.Count == 0)
        {
            return null;
        }

        //no prio list retrun
        if(prioList == null || prioList.Count == 0)
        {
            return GetClosest(possibleTargets, selfPos);
        }

        //starts a prio 0 and works up, returns when closest target in prio has been found.
        //If no target, move to next prio.
        foreach(var prio in prioList)
        {
            ITargetable best = null;
            float bestSqrDistance = float.PositiveInfinity;

            foreach(var target in possibleTargets)
            {
                if (target.UnitPrio != prio)
                    continue;

                float sqrDist = (selfPos - (Vector2)target.Transform.position).sqrMagnitude;
                if(sqrDist < bestSqrDistance)
                {
                    bestSqrDistance = sqrDist;
                    best = target;
                }
            }
            if (best != null)
            {
                return best;
            }
        }
        return GetClosest(possibleTargets, selfPos);
    }

    public ITargetable GetClosest(List<ITargetable> possibleTargets, Vector2 selfPos)
    {
        float closestSqrDistance = float.PositiveInfinity;
        ITargetable nearestEnemy = null;

        foreach (var target in possibleTargets)
        {
            float sqrDist = (selfPos - (Vector2)target.Transform.position).sqrMagnitude;
            if (sqrDist < closestSqrDistance)
            {
                closestSqrDistance = sqrDist;
                nearestEnemy = target;
            }
        }
        return nearestEnemy;
    }
}