using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using UnityEditor;
using UnityEngine;

public class WaveGeneratorTests
{
    private GameObject _spawnDatabaseObject;
    private List<SpawnDefinition> _spawnDefinitions;
    private SpawnDatabase _spawnDB;

    private WaveRules _waveRules;

    [SetUp]
    public void SetUp()
    {
        _spawnDefinitions = AssetDatabase
            .FindAssets("t:SpawnDefinition")
            .Select(guid =>
                AssetDatabase.LoadAssetAtPath<SpawnDefinition>(
                    AssetDatabase.GUIDToAssetPath(guid)))
            .ToList();

        _spawnDatabaseObject = new GameObject("SpawnDatabase");
        _spawnDB = _spawnDatabaseObject.AddComponent<SpawnDatabase>();

        _spawnDB.Initialize(_spawnDefinitions);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(_spawnDatabaseObject);
    }


    [TestCase(9)]  //Wave 10
    [TestCase(19)] //Wave 20
    [TestCase(29)] //Wave 30
    public void GenerateWave_BossExists(int waveIndex)
    {
        var wave = new WaveGenerator(_spawnDB, _waveRules).GenerateWave(waveIndex);

        var bossPrefab = _spawnDB.GetSpawn("spawn_giant").UnitPrefab;

        Assert.That(
            wave.enemiesToSpawn.Any(e => e.prefab == bossPrefab),
            Is.True,
            "Boss prefab must be present"
        );
    }

    [TestCase(9, 1)] //Wave 10
    [TestCase(19, 2)] //Wave 20
    [TestCase(29, 3)] //Wave 30
    public void GenerateWave_BossCount_IsCorrect(int waveIndex, int expectedCount)
    {
        var wave = new WaveGenerator(_spawnDB, _waveRules).GenerateWave(waveIndex);

        var bossPrefab = _spawnDB.GetSpawn("spawn_giant").UnitPrefab;

        var bossGroup = wave.enemiesToSpawn.First(e => e.prefab == bossPrefab);
        Assert.That(bossGroup.count, Is.EqualTo(expectedCount));
    }

    [TestCase(20)]   //Wave 21
    [TestCase(27)]  //Wave 28
    [TestCase(34)]  //Wave 35
    public void GenerateWave_AssasinExists(int waveIndex)
    {
        var wave = new WaveGenerator(_spawnDB, _waveRules).GenerateWave(waveIndex);

        var assasinPrefab = _spawnDB.GetSpawn("spawn_assassin").UnitPrefab;

        Assert.That(
            wave.enemiesToSpawn.Any(e => e.prefab == assasinPrefab),
            Is.True,
            "Assasin prefab must be present"
        );
    }

    [TestCase(20, 25)]   //Wave 21
    [TestCase(27, 32)]  //Wave 28
    [TestCase(34, 39)]  //Wave 35
    public void GenerateWave_AssasinCount_IsCorrect(int waveIndex, int expectedCount)
    {
        var wave = new WaveGenerator(_spawnDB, _waveRules).GenerateWave(waveIndex);

        var assasinPrefab = _spawnDB.GetSpawn("spawn_assassin").UnitPrefab;

        var assasinGroup = wave.enemiesToSpawn.First(e => e.prefab == assasinPrefab);
        Assert.That(assasinGroup.count, Is.EqualTo(expectedCount));
    }

    [TestCase(0)] //Wave 1 - Cant spawn
    [TestCase(1)] //Wave 2 - Cant spawn
    [TestCase(2)] //Wave 3 - Cant spawn
    [TestCase(3)] //Wave 4 - Cant spawn
    public void GenerateWave_EliteFighter_CantSpawnBeforeUnlockWave(int waveIndex)
    {
        var wave = new WaveGenerator(_spawnDB, _waveRules, () => 0f).GenerateWave(waveIndex);

        var eliteFighterPrefab = _spawnDB.GetSpawn("spawn_elitefighter").UnitPrefab;

        var waveGroup = wave.enemiesToSpawn.FirstOrDefault(e => e.prefab == eliteFighterPrefab);
        Assert.IsNull(waveGroup);
    }

    [TestCase(4)] //Wave 5 - Can spawn
    [TestCase(12)] //Wave 13 - Can spawn
    [TestCase(26)] //Wave 27 - Can spawn
    public void GenerateWave_EliteFighter_CanSpawnAtOrAfterUnlockWave(int waveIndex)
    {
        var wave = new WaveGenerator(_spawnDB, _waveRules, () => 0f).GenerateWave(waveIndex);

        var eliteFighterPrefab = _spawnDB.GetSpawn("spawn_elitefighter").UnitPrefab;

        var waveGroup = wave.enemiesToSpawn.FirstOrDefault(e => e.prefab == eliteFighterPrefab);
        Assert.IsNotNull(waveGroup);
    }

    [TestCase(0)] //Wave 1 - Cant spawn
    [TestCase(8)] //Wave 9 - Cant spawn
    [TestCase(13)] //Wave 14 - Cant spawn
    [TestCase(18)] //Wave 19 - Cant spawn
    public void GenerateWave_EliteCavalier_CantSpawnBeforeUnlockWave(int waveIndex)
    {
        var wave = new WaveGenerator(_spawnDB, _waveRules, () => 0f).GenerateWave(waveIndex);

        var eliteCavalierPrefab = _spawnDB.GetSpawn("spawn_elitecavalier").UnitPrefab;

        var waveGroup = wave.enemiesToSpawn.FirstOrDefault(e => e.prefab == eliteCavalierPrefab);
        Assert.IsNull(waveGroup);
    }

    [TestCase(21)] //Wave 22 - Can spawn
    [TestCase(28)] //Wave 29 - Can spawn
    [TestCase(33)] //Wave 34 - Can spawn
    public void GenerateWave_EliteCavalier_CanSpawnAtOrAfterUnlockWave(int waveIndex)
    {
        var wave = new WaveGenerator(_spawnDB, _waveRules, () => 0f).GenerateWave(waveIndex);

        var eliteCavalierPrefab = _spawnDB.GetSpawn("spawn_elitecavalier").UnitPrefab;

        var waveGroup = wave.enemiesToSpawn.FirstOrDefault(e => e.prefab == eliteCavalierPrefab);
        Assert.IsNotNull(waveGroup);
    }

    [TestCase(0)] //Wave 1 - Cant spawn
    [TestCase(5)] //Wave 6 - Cant spawn
    [TestCase(7)] //Wave 8 - Cant spawn
    [TestCase(8)] //Wave 9 - Cant spawn
    public void GenerateWave_Sapper_CantSpawnBeforeUnlockWave(int waveIndex)
    {
        var wave = new WaveGenerator(_spawnDB, _waveRules, () => 0f).GenerateWave(waveIndex);

        var sapperPrefab = _spawnDB.GetSpawn("spawn_sapper").UnitPrefab;

        var waveGroup = wave.enemiesToSpawn.FirstOrDefault(e => e.prefab == sapperPrefab);
        Assert.IsNull(waveGroup);
    }

    [TestCase(10)] //Wave 11 - Can spawn
    [TestCase(18)] //Wave 19 - Can spawn
    [TestCase(24)] //Wave 25 - Can spawn
    public void GenerateWave_Sapper_CanSpawnAtOrAfterUnlockWave(int waveIndex)
    {
        var wave = new WaveGenerator(_spawnDB, _waveRules, () => 0f).GenerateWave(waveIndex);

        var sapperPrefab = _spawnDB.GetSpawn("spawn_sapper").UnitPrefab;

        var waveGroup = wave.enemiesToSpawn.FirstOrDefault(e => e.prefab == sapperPrefab);
        Assert.IsNotNull(waveGroup);
    }

    [TestCase(9)]  // Wave 10
    [TestCase(19)] // Wave 20
    [TestCase(29)] // Wave 30
    public void GenerateWave_BossWave_ContainsOnlyBoss(int waveIndex)
    {
        //Set to 0f, to make sure if elites can spawn they will
        var wave = new WaveGenerator(_spawnDB, _waveRules, () => 0f).GenerateWave(waveIndex);

        var bossPrefab = _spawnDB.GetSpawn("spawn_giant").UnitPrefab;

        var bossGroup = wave.enemiesToSpawn.FirstOrDefault(e => e.prefab == bossPrefab);
        Assert.That(bossGroup, Is.Not.Null, $"Boss missing in wave {waveIndex + 1}");

        foreach (var group in wave.enemiesToSpawn)
        {
            Assert.That(group.prefab, Is.EqualTo(bossPrefab),
                $"Wave {waveIndex + 1} contains unexpected unit: {group.prefab.name}");
        }
    }

    [TestCase(20)] // Wave 21
    [TestCase(27)] // Wave 28
    [TestCase(34)] // Wave 35
    public void GenerateWave_AssassinWave_ContainsOnlyAssassins(int waveIndex)
    {
        //Set to 0f, to make sure if elites can spawn they will
        var wave = new WaveGenerator(_spawnDB, _waveRules, () => 0f).GenerateWave(waveIndex);

        var assassinPrefab = _spawnDB.GetSpawn("spawn_assassin").UnitPrefab;

        var assassinGroup = wave.enemiesToSpawn.FirstOrDefault(e => e.prefab == assassinPrefab);
        Assert.That(assassinGroup, Is.Not.Null, $"Assassin missing in wave {waveIndex + 1}");

        foreach (var group in wave.enemiesToSpawn)
        {
            Assert.That(group.prefab, Is.EqualTo(assassinPrefab),
                $"Wave {waveIndex + 1} contains unexpected unit: {group.prefab.name}");
        }
    }

}
