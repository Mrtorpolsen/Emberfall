using System.Collections.Generic;
using UnityEngine;

public class WaveGenerator
{
    [SerializeField] private float waveGrowthRate = 1.05f;
    [SerializeField] private int baseCount = 4;
    [SerializeField] private int sapperCount = 1;

    private readonly System.Func<float> randomFunc;

    public WaveGenerator(System.Func<float> randomFunc = null)
    {
        this.randomFunc = randomFunc ?? (() => UnityEngine.Random.value);
    }

    public WaveDefinition GenerateWave(int waveNumber)
    {
        int waveNumberDisplay = waveNumber + 1;

        var wave = new WaveDefinition
        {
            enemiesToSpawn = new List<EnemyGroup>(),
        };

        (float fighter, float cavalier) unitComposition = (0f, 0f);

        if (waveNumberDisplay <= 10) unitComposition = (1f, 0f);
        else if (waveNumberDisplay <= 20) unitComposition = (0.7f, 0.2f);
        else if (waveNumberDisplay <= 40) unitComposition = (0.6f, 0.3f);
        else if (waveNumberDisplay <= 60) unitComposition = (0.5f, 0.4f);

        //scaling
        int unitCount;

        unitCount = Mathf.RoundToInt(baseCount * Mathf.Pow(waveGrowthRate, waveNumber - 1));

        int fighterCount = Mathf.RoundToInt(unitCount * unitComposition.fighter);
        int cavalierCount = unitCount - fighterCount;

        float spawnDelay = 0.5f;

        //build waves here
        //customise for special waves
        if (IsMilestone(waveNumber, 10, 0, 10)) //start wave 10, and runs every 10 level
        {
            int bossCountForWave = (waveNumberDisplay / 10);

            wave.enemiesToSpawn.Add(new EnemyGroup(Prefabs.giantPrefab, bossCountForWave, spawnDelay));
        }
        else if (IsMilestone(waveNumber, 7, 0, 21)) //start wave 21, and runs every 7 level
        {
            int assassinMilestoneIndex = (waveNumberDisplay - 21) / 7;

            int assassinCountForWave = 25 + assassinMilestoneIndex * 7;

            wave.enemiesToSpawn.Add(new EnemyGroup(Prefabs.assasinPrefab, assassinCountForWave, spawnDelay));
        }
        else
        {
            if (waveNumberDisplay >= 5 && randomFunc() < 0.2f)
            {
                wave.enemiesToSpawn.Add(new EnemyGroup(Prefabs.eliteFighterPrefab, 1, spawnDelay));
                fighterCount--;
            }
            if (waveNumberDisplay >= 20 && randomFunc() < 0.2f)
            {
                wave.enemiesToSpawn.Add(new EnemyGroup(Prefabs.eliteCavalierPrefab, 1, spawnDelay));
                cavalierCount--;
            }
            if (waveNumberDisplay > 10 && randomFunc() < 0.2f)
            {
                wave.enemiesToSpawn.Add(new EnemyGroup(Prefabs.sapperPrefab, sapperCount, spawnDelay));
            }
            wave.enemiesToSpawn.Add(new EnemyGroup(Prefabs.fighterPrefab, fighterCount, spawnDelay));
            wave.enemiesToSpawn.Add(new EnemyGroup(Prefabs.cavalierPrefab, cavalierCount, spawnDelay));
        }

        //Debug.Log($"Wave: {waveNumberDisplay} spawned at: {TimerManager.Instance.GetElapsedTime()}");
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