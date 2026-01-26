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

    public static WaveGenerator waveGenerator;

    [Header("Settings")]
    [SerializeField] private Transform northSpawn;
    [SerializeField] private int totalWaves = 100;
    [SerializeField] private float timeBetweenWaves = 10f;


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
        waveGenerator = new WaveGenerator();
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
            var wave = waveGenerator.GenerateWave(currentWaveIndex);
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

                FinalStats stats = UnitStatsManager.Instance.GetEnemyStats(unitName, scaling);

                SpawnManager.Instance.SpawnUnit(group.prefab, northSpawn, Team.North, stats);
                yield return new WaitForSeconds(0);
            }
        }
        isSpawning = false;
    }



    public bool IsWaveActive() => isSpawning;
    public int GetCurrentWaveNumber() => currentWaveIndex + 1;
}