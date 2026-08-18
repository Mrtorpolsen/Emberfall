using System.Collections;
using UnityEngine;

public class WaveThreatCalculator
{
    private float difficultyScaling;
    private float startingThreat;
    private int scalingDelay;

    //Need to set difficulty scaling and starting threat based on difficulty level
    public WaveThreatCalculator(float difficultyScaling, float startingThreat, int scalingDelay)
    {
        this.difficultyScaling = difficultyScaling;
        this.startingThreat = startingThreat;
        this.scalingDelay = scalingDelay;
    }

    public int GetThreatValueForWave(int waveNumber)
    {
        if(waveNumber < scalingDelay)
        {
            return Mathf.RoundToInt(startingThreat);
        } 
        else
        {
            return Mathf.RoundToInt(startingThreat * Mathf.Pow(difficultyScaling, waveNumber));
        }
    }
}
