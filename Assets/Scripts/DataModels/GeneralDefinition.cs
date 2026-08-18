using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/General Definition")]
public class GeneralDefinition : ScriptableObject
{
    public string id;
    public string generalName;
    public List<string> tauntPhrases;
    public List<SpawnDefinition> unitRoster;
    public SpawnDefinition generalUnit;
    public int spawnLimit;

    public float GetMinCost(ThreatCalculator threatCalculator)
    {
        float minCost = threatCalculator.CalculateThreat(unitRoster[0].Stats);

        foreach (var unit in unitRoster)
        {
            float cost = threatCalculator.CalculateThreat(unit.Stats);

            if (cost < minCost)
            {
                minCost = cost;
            }
        }

        return minCost;
    }
}