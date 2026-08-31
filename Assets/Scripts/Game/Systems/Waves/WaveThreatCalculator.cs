using System.Collections;
using UnityEngine;

public class WaveThreatCalculator
{
    private float difficultyScaling;
    private float startingThreat;
    private int scalingDelay;

    //Need to set difficulty scaling and starting threat based on difficulty level
    public WaveThreatCalculator(Difficulty difficulty)
    {
        this.difficultyScaling = difficulty.DifficultyScaling;
        this.startingThreat = difficulty.StartingThreat;
        this.scalingDelay = difficulty.ScalingDelay;

        Debug.Log($"WaveThreatCalculator initialized with difficulty scaling: {difficultyScaling}, starting threat: {startingThreat}, scaling delay: {scalingDelay}");
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
