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

    public static WaveController Instance { get; private set; }

    public static WaveGenerator waveGenerator;
    private WaveRules waveRules;

    [Header("Settings")]
    [SerializeField] private Transform northSpawn;
    [SerializeField] private Transform southSpawn;
    [SerializeField] private int totalWaves = 100;
    [SerializeField] private float timeBetweenWaves = 10f;

    [Header("Generals")]
    [SerializeField] private List<GeneralDefinition> allGenerals = new();

#if UNITY_EDITOR
    [Header("Test")]
    [SerializeField] private Boolean isTest;
    [SerializeField] private int spawnOfEach;
#endif

    private int currentWaveIndex = 0;
    private bool isSpawning = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        //Get settings from difficulty
        var waveThreatCalculator = new WaveThreatCalculator(1.1f, 200, 6);

        waveRules = new WaveRules(allGenerals);
        waveGenerator = new WaveGenerator(SpawnDatabase.Instance, waveThreatCalculator);
    }

    public void StartWaves()
    {
#if UNITY_EDITOR
        if (isTest)
        {
            Benchmark(spawnOfEach);
            return;
        }
#endif

        StartCoroutine(RunWaves());
    }

    private IEnumerator RunWaves()
    {
        //Get the starting general
        var generalDefinition = waveRules.GetGeneral();
        int generalSpawnIndex = generalDefinition.spawnLimit;

        while (currentWaveIndex < totalWaves && !GameManager.Instance.isGameOver)
        {
            if (currentWaveIndex >= generalSpawnIndex)
            {
                generalDefinition = waveRules.GetGeneral();
                generalSpawnIndex += generalDefinition.spawnLimit;
            }

            var wave = waveGenerator.GenerateWave(currentWaveIndex, generalDefinition);
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
        //Debug.Log($"Spawning Wave {currentWaveIndex + 1}: {wave.enemiesToSpawn.Count} groups");

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

                //remove to enable stats scaling again
                scaling.waveIndex = 0;

                FinalStats stats = UnitStatsManager.Instance.GetEnemyStats(unitName, scaling);

                SpawnManager.Instance.SpawnUnit(group.prefab, northSpawn, Team.North, stats);
                yield return new WaitForSeconds(0);
            }
        }
        isSpawning = false;
    }
#if UNITY_EDITOR
    private void Benchmark(int spawnOfEach)
    {
        SpawnDatabase spawnDB = SpawnDatabase.Instance;
        GameManager.Instance.AddCurrency(Team.South, 1000000);
        for (int i = 0; i < spawnOfEach; i++)
        {
            SpawnManager.Instance.SpawnUnit(spawnDB.GetSpawn("spawn_fighter").UnitPrefab, southSpawn, Team.South);
        }
        for (int i = 0; i < spawnOfEach; i++)
        {
            SpawnManager.Instance.SpawnUnit(spawnDB.GetSpawn("spawn_ranger").UnitPrefab, southSpawn, Team.South);
        }
        for (int i = 0; i < spawnOfEach; i++)
        {
            SpawnManager.Instance.SpawnUnit(spawnDB.GetSpawn("spawn_cavalier").UnitPrefab, southSpawn, Team.South);
        }

        CoroutineHelpers.DoAfterDelay(5, () =>
        {
        });

        var wave = new WaveDefinition
        {
            enemiesToSpawn = new List<EnemyGroup>(),
        };
        
        wave.enemiesToSpawn.Add(new EnemyGroup(spawnDB.GetSpawn("spawn_cavalier").UnitPrefab, spawnOfEach, 0.5f));
        wave.enemiesToSpawn.Add(new EnemyGroup(spawnDB.GetSpawn("spawn_fighter").UnitPrefab, spawnOfEach, 0.5f));
        wave.enemiesToSpawn.Add(new EnemyGroup(spawnDB.GetSpawn("spawn_ranger").UnitPrefab, spawnOfEach, 0.5f));

        StartCoroutine(SpawnWave(wave));

    }
#endif
    public bool IsWaveActive() => isSpawning;
    public int GetCurrentWaveNumber() => currentWaveIndex + 1;
}