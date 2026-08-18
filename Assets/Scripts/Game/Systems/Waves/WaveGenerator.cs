using System.Collections.Generic;
using System;
using UnityEngine;

public class WaveGenerator
{
    private int sapperCount = 1;

    private int sapperWaveCooldown = 0;

    private readonly Func<float> randomFunc;
    private readonly SpawnDatabase spawnDB;
    private readonly ThreatCalculator threatCalculator = new ThreatCalculator();
    private readonly WaveThreatCalculator waveThreatCalculator;

    public event Action<int> OnWaveNumberChanged;

    public WaveGenerator(SpawnDatabase spawnDB, WaveThreatCalculator waveThreatCalculator, Func<float> randomFunc = null)
    {
        // For testing to gaurentee spawn
        this.randomFunc = randomFunc ?? (() => UnityEngine.Random.value);
        this.spawnDB = spawnDB;
        this.waveThreatCalculator = waveThreatCalculator;
    }

    public WaveDefinition GenerateWave(int waveNumber, GeneralDefinition generalDefinition)
    {
        int waveNumberDisplay = waveNumber + 1;

        var generalRoster = new List<SpawnDefinition>(generalDefinition.unitRoster);

        float threatValue = waveThreatCalculator.GetThreatValueForWave(waveNumber);
        float spawnDelay = 0.5f;

        OnWaveNumberChanged?.Invoke(waveNumberDisplay);

        WaveDefinition wave = new WaveDefinition
        {
            enemiesToSpawn = new List<EnemyGroup>(),
        };

        //if (currentGeneral != generalDefinition)
        //{
        //    wave.enemiesToSpawn.Add(new EnemyGroup(generalDefinition.generalUnit.UnitPrefab, 1, spawnDelay));
        //    currentGeneral = generalDefinition;
        //}

        if (IsMilestone(waveNumber, 7, 0, 21)) //start wave 21, and runs every 7 level
        {
            float threatCost = threatCalculator.CalculateThreat(spawnDB.GetSpawn("spawn_assassin").Stats);

            int assassinCountForWave = Mathf.FloorToInt(threatValue / threatCost);

            wave.enemiesToSpawn.Add(new EnemyGroup(spawnDB.GetSpawn("spawn_assassin").UnitPrefab, assassinCountForWave, spawnDelay));
        }
        else
        {
            int safety = 0;
            while (generalRoster.Count > 0 && threatValue > 0)
            {
                if (++safety > 1000)
                {
                    Debug.LogError("Wave generation exceeded 1,000 iterations.");
                    break;
                }

                var unitToAdd = generalRoster[UnityEngine.Random.Range(0, generalRoster.Count)];
                var threatCost = threatCalculator.CalculateThreat(unitToAdd.Stats);

                if (threatCost <= 0)
                {
                    throw(new Exception("Threat cost must be greater than zero."));
                }

                if (threatCost <= threatValue)
                {
                    wave.enemiesToSpawn.Add(new EnemyGroup(unitToAdd.UnitPrefab, 1, spawnDelay));
                    threatValue -= threatCost;
                }
                else
                {
                    generalRoster.Remove(unitToAdd);
                }
            }
        }

        if (waveNumberDisplay > 10 && sapperWaveCooldown == 0 && randomFunc() < 0.2f && !IsMilestone(waveNumber, 7, 0, 21))
        {
            wave.enemiesToSpawn.Add(new EnemyGroup(spawnDB.GetSpawn("spawn_sapper").UnitPrefab, sapperCount, spawnDelay));
            sapperWaveCooldown = 4; // Set cooldown for 5 waves
        }
        if (sapperWaveCooldown > 0)
        {
            sapperWaveCooldown--;
        }

        return wave;
    }

    public static bool IsMilestone(
        int waveNumber,
        int interval,
        int offset,
        int minDisplayWave)
    {
        int displayWave = waveNumber + 1;

        if (displayWave < minDisplayWave)
            return false;

        return displayWave % interval == offset;
    }
}