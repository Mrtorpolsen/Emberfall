using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WaveController : MonoBehaviour
{
    public struct EnemyScalingContext
    {
        public int waveIndex;
        public bool isRanged;
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
    [SerializeField] private Transform generalSpawn;
    [SerializeField] private Transform southSpawn;
    [SerializeField] private Transform bossPosition;
    [SerializeField] private int totalWaves = 1000;
    [SerializeField] private float timeBetweenWaves = 10f;

    [Header("Generals")]
    [SerializeField] private List<GeneralDefinition> allGenerals = new();

    private GeneralDefinition currentGeneral;

    private int currentWaveIndex = 0;
    private bool isSpawning = false;

    private Difficulty currentDifficulty;

#if UNITY_EDITOR
    [Header("Test")]
    [SerializeField] private Boolean isTest;
    [SerializeField] private int spawnOfEach;
#endif


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void InitializeDifficulty(Difficulty difficulty)
    {
        //Can move generals to sets and here the set gets selected based on difficulty, but for now keep it simple
        currentDifficulty = difficulty;

        List<GeneralDefinition> activeGenerals = new List<GeneralDefinition>(allGenerals);
        //Temporary solution, until the general difficulty sets are implemented
        GeneralDefinition tutorialGeneral = allGenerals.FirstOrDefault((general) => general.id == "sirtutorthethird");
        activeGenerals.Remove(tutorialGeneral);

        if (currentDifficulty.Level == DifficultyLevel.Tutorial)
        {
            activeGenerals = new List<GeneralDefinition> { tutorialGeneral };
            totalWaves = activeGenerals[0].spawnLimit;
        }

        Debug.Log($"Difficulty Settings: {currentDifficulty.Level}");
        var waveThreatCalculator = new WaveThreatCalculator(currentDifficulty);

        waveRules = new WaveRules(activeGenerals);
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
        currentGeneral = waveRules.GetGeneral();
        int generalSpawnIndex = currentGeneral.spawnLimit;

        while (currentWaveIndex < totalWaves && !GameManager.Instance.isGameOver)
        {
            if (currentWaveIndex >= generalSpawnIndex)
            {
                currentGeneral = waveRules.GetGeneral();
                generalSpawnIndex += currentGeneral.spawnLimit;
            }

            var wave = waveGenerator.GenerateWave(currentWaveIndex, currentGeneral);
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

        foreach (var group in wave.enemiesToSpawn)
        {
            for (int i = 0; i < group.count; i++)
            {
                var scaling = CreateScalingContext(group.prefab);
                scaling.spawnIndex = spawnIndex++;

                string unitName = group.prefab.name.ToLowerInvariant();

                StatBlock stats = UnitStatsManager.Instance.GetEnemyStats(unitName, scaling);

                if (group.isBoss)
                {
                    SpawnManager.Instance.SpawnUnitAtPoint(group.prefab, generalSpawn, Team.North, out var bossPrefab, stats);
                    var generalComponent = bossPrefab.AddComponent<GeneralComponent>();
                    generalComponent.Initialize(currentGeneral, bossPosition, currentWaveIndex);
                }
                else
                {
                    SpawnManager.Instance.SpawnUnit(group.prefab, northSpawn, Team.North, stats);
                }

                // One spawn per frame
                yield return null;
            }
        }
        isSpawning = false;
    }
    // Scaling for an enemy spawned now (wave spawns and enemy summons)
    public EnemyScalingContext CreateScalingContext(GameObject prefab)
    {
        return new EnemyScalingContext
        {
            // Difficulties without stat scaling always use wave 0
            waveIndex = currentDifficulty.StatScaling ? currentWaveIndex : 0,
            isRanged = prefab.TryGetComponent<RangedShooter>(out _)
        };
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