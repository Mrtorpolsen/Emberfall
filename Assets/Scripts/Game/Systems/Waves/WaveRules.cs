using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class WaveRules
{
    private readonly List<GeneralDefinition> allGenerals = new();
    private List<GeneralDefinition> availableGenerals;

    public WaveRules(List<GeneralDefinition> allGenerals)
    {
        this.allGenerals = allGenerals;
        availableGenerals = new List<GeneralDefinition>(allGenerals);
    }

    public GeneralDefinition GetGeneral()
    {
        if (availableGenerals.Count == 0)
        {
            availableGenerals = new List<GeneralDefinition>(allGenerals);
            Debug.Log("All generals have been used. Resetting available generals.");
        }

        var general = availableGenerals[Random.Range(0, availableGenerals.Count)];
        availableGenerals.Remove(general);
        return general;
    }
}
