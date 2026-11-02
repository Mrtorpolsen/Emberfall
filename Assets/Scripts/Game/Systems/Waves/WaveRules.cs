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

    private GeneralDefinition lastGeneral;

    public GeneralDefinition GetGeneral()
    {
        if (availableGenerals.Count == 0)
        {
            availableGenerals = new List<GeneralDefinition>(allGenerals);
            Debug.Log("All generals have been used. Resetting available generals.");
        }

        int index = Random.Range(0, availableGenerals.Count);

        // Don't open a new cycle with the general that just ended the last one,
        // so the same general never runs twice in a row
        if (availableGenerals.Count > 1 && availableGenerals[index] == lastGeneral)
            index = (index + Random.Range(1, availableGenerals.Count)) % availableGenerals.Count;

        var general = availableGenerals[index];
        availableGenerals.Remove(general);
        lastGeneral = general;
        return general;
    }
}
