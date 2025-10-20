using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    public static WaveManager main;

    [Header("Settings")]
    [SerializeField] private Transform northSpawn;
    [SerializeField] private int totalWaves = 100;
    [SerializeField] private float timeBetweenWaves = 10f;
    [SerializeField] private float waveGrowthRate = 1.05f;
    [SerializeField] private int baseCount = 4;

    [Header("Prefabs")]
    [SerializeField] private GameObject fighterPrefab;
    [SerializeField] private GameObject rangerPrefab;
    [SerializeField] private GameObject cavalierPrefab;
    [SerializeField] private GameObject gatePrefab;

    private int currentWaveIndex = 0;
    private bool isSpawning = false;

    private void Awake()
    {
        if (main != null && main != this)
        {
            Destroy(main);
            return;
        }
        main = this;
    }

    public void StartWaves()
    {
        StartCoroutine(RunWaves());
    }

    private IEnumerator RunWaves()
    {
        while (currentWaveIndex < totalWaves)
        {
            var wave = GenerateWave(currentWaveIndex);
            yield return StartCoroutine(SpawnWave(wave));
            currentWaveIndex++;

            yield return new WaitForSeconds(timeBetweenWaves);
        }
        Debug.Log("You won! You cheater...");
    }

    private IEnumerator SpawnWave(WaveDefinition wave)
    {
        if(GameManager.main.isGameOver != true)
        {
            isSpawning = true;
            Debug.Log($"Spawning Wave {currentWaveIndex + 1}: {wave.enemiesToSpawn.Count} groups");

            foreach(var group in wave.enemiesToSpawn)
            {
                for(int i = 0; i < group.count; i++)
                {
                    UnitManager.main.SpawnUnit(group.prefab, northSpawn, Team.North);
                    yield return new WaitForSeconds(0);
                }
            }
            isSpawning = false;
        }
    }

    private WaveDefinition GenerateWave(int waveNumber)
    {
        var wave = new WaveDefinition
        {
            enemiesToSpawn = new List<EnemyGroup>(),
        };

        (float fighter, float cavalier) unitComposition = (0f, 0f);

        if (waveNumber <= 10) unitComposition = (1f, 0f);
        else if (waveNumber <= 20) unitComposition = (0.7f, 0.2f);
        else if (waveNumber <= 40) unitComposition = (0.6f, 0.3f);
        else if (waveNumber <= 60) unitComposition = (0.5f, 0.4f);

        //scaling   
        int unitCount;

        unitCount = Mathf.RoundToInt(baseCount * Mathf.Pow(waveGrowthRate, waveNumber - 1));
        
        int fighterCount = Mathf.RoundToInt(unitCount * unitComposition.fighter);
        int cavalierCount = unitCount - fighterCount;

        float spawnDelay = 0.5f;

        //customise for special waves
        if ((waveNumber + 1) % 10 == 0)
        {
            //spawn boss
        }
        else
        {
            //build waves here
            wave.enemiesToSpawn.Add(new EnemyGroup(fighterPrefab, fighterCount, spawnDelay));
            wave.enemiesToSpawn.Add(new EnemyGroup(cavalierPrefab, cavalierCount, spawnDelay));
        }
        Debug.Log($"Wave: {waveNumber} spawned at: {TimerManager.main.GetElapsedTime()}");
        return wave;
    }

    public bool IsWaveActive() => isSpawning;
    public int GetCurrentWaveNumber() => currentWaveIndex + 1;
}