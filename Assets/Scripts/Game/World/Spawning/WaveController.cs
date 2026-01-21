using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveController : MonoBehaviour
{
    public struct EnemyScalingContext
    {
        public int waveIndex;
        // For later, incase we want some soft enrage,
        // or maybe use this to track how many spawns we've made,
        // compared to how many kills and figure out increase in
        // dmg to units if too many units alive.
        public int spawnIndex; 
    }

    public static WaveController Instance;

    [Header("Settings")]
    [SerializeField] private Transform northSpawn;
    [SerializeField] private int totalWaves = 100;
    [SerializeField] private float timeBetweenWaves = 10f;
    [SerializeField] private float waveGrowthRate = 1.05f;
    [SerializeField] private int baseCount = 4;
    [SerializeField] private int bossCount = 1;
    [SerializeField] private int sapperCount = 1;

    [Header("Test")]
    [SerializeField] private Boolean isTest;

    private int currentWaveIndex = 0;
    private bool isSpawning = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(Instance);
            return;
        }
        Instance = this;
    }

    public void StartWaves()
    {
        if (isTest) return;

        StartCoroutine(RunWaves());
    }

    private IEnumerator RunWaves()
    {
        while (currentWaveIndex < totalWaves && !GameManager.Instance.isGameOver)
        {
            var wave = GenerateWave(currentWaveIndex);
            yield return StartCoroutine(SpawnWave(wave));
            currentWaveIndex++;

            yield return new WaitForSeconds(timeBetweenWaves);
        }
        Debug.Log("You won! You cheater... or lost haha");
    }

    private IEnumerator SpawnWave(WaveDefinition wave)
    {
        isSpawning = true;

        int spawnIndex = 0;
        Debug.Log($"Spawning Wave {currentWaveIndex + 1}: {wave.enemiesToSpawn.Count} groups");

        foreach(var group in wave.enemiesToSpawn)
        {
            for(int i = 0; i < group.count; i++)
            {
                var scaling = new EnemyScalingContext
                {
                    waveIndex = currentWaveIndex,
                    spawnIndex = spawnIndex++
                };

                string unitName = group.prefab.name.ToLowerInvariant();

                FinalStats stats = UnitStatsManager.Instance.GetEnemyStats(unitName, scaling);

                SpawnManager.Instance.SpawnUnit(group.prefab, northSpawn, Team.North, stats);
                yield return new WaitForSeconds(0);
            }
        }
        isSpawning = false;
    }

    private WaveDefinition GenerateWave(int waveNumber)
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

        //customise for special waves
        if (IsMilestone(waveNumber, 10, 0, 10))
        {
            wave.enemiesToSpawn.Add(new EnemyGroup(Prefabs.giantPrefab, bossCount, spawnDelay));
            bossCount++;
        }
        else if (IsMilestone(waveNumber, 10, 1, 10))
        {
            wave.enemiesToSpawn.Add(new EnemyGroup(Prefabs.sapperPrefab, sapperCount, spawnDelay));
            sapperCount++;
        }
        else
        {
            if(waveNumberDisplay >= 5 && UnityEngine.Random.value < 0.2f)
            {
                wave.enemiesToSpawn.Add(new EnemyGroup(Prefabs.eliteFighterPrefab, 1, spawnDelay));
                fighterCount--;
            }   
            if(waveNumberDisplay >= 20 && UnityEngine.Random.value < 0.2f)
            {
                wave.enemiesToSpawn.Add(new EnemyGroup(Prefabs.eliteCavalierPrefab, 1, spawnDelay));
                cavalierCount--;
            }   
            //build waves here
            wave.enemiesToSpawn.Add(new EnemyGroup(Prefabs.fighterPrefab, fighterCount, spawnDelay));
            wave.enemiesToSpawn.Add(new EnemyGroup(Prefabs.cavalierPrefab, cavalierCount, spawnDelay));
        }

        Debug.Log($"Wave: {waveNumberDisplay} spawned at: {TimerManager.Instance.GetElapsedTime()}");
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

    public bool IsWaveActive() => isSpawning;
    public int GetCurrentWaveNumber() => currentWaveIndex + 1;
}