using NUnit.Framework;
using System.Linq;
using UnityEngine;

public class WaveGeneratorTests
{
    [TestCase(9)]  //Wave 10
    [TestCase(19)] //Wave 20
    [TestCase(29)] //Wave 30
    public void GenerateWave_BossExists(int waveIndex)
    {
        var wave = new WaveGenerator().GenerateWave(waveIndex);
        Assert.That(
            wave.enemiesToSpawn.Any(e => e.prefab == Prefabs.giantPrefab),
            Is.True,
            "Boss prefab must be present"
        );
    }

    [TestCase(9, 1)] //Wave 10
    [TestCase(19, 2)] //Wave 20
    [TestCase(29, 3)] //Wave 30
    public void GenerateWave_BossCount_IsCorrect(int waveIndex, int expectedCount)
    {
        var wave = new WaveGenerator().GenerateWave(waveIndex);
        var bossGroup = wave.enemiesToSpawn.First(e => e.prefab == Prefabs.giantPrefab);
        Assert.That(bossGroup.count, Is.EqualTo(expectedCount));
    }

    [TestCase(20)]   //Wave 21
    [TestCase(27)]  //Wave 28
    [TestCase(34)]  //Wave 35
    public void GenerateWave_AssasinExists(int waveIndex)
    {
        var wave = new WaveGenerator().GenerateWave(waveIndex);
        Assert.That(
            wave.enemiesToSpawn.Any(e => e.prefab == Prefabs.assasinPrefab),
            Is.True,
            "Assasin prefab must be present"
        );
    }

    [TestCase(20, 25)]   //Wave 21
    [TestCase(27, 32)]  //Wave 28
    [TestCase(34, 39)]  //Wave 35
    public void GenerateWave_AssasinCount_IsCorrect(int waveIndex, int expectedCount)
    {
        var wave = new WaveGenerator().GenerateWave(waveIndex);
        var assasinGroup = wave.enemiesToSpawn.First(e => e.prefab == Prefabs.assasinPrefab);
        Assert.That(assasinGroup.count, Is.EqualTo(expectedCount));
    }

    [TestCase(0)] //Wave 1 - Cant spawn
    [TestCase(1)] //Wave 2 - Cant spawn
    [TestCase(2)] //Wave 3 - Cant spawn
    [TestCase(3)] //Wave 4 - Cant spawn
    public void GenerateWave_EliteFighter_CantSpawnBeforeUnlockWave(int waveIndex)
    {
        var wave = new WaveGenerator(() => 0f).GenerateWave(waveIndex);
        var waveGroup = wave.enemiesToSpawn.FirstOrDefault(e => e.prefab == Prefabs.eliteFighterPrefab);
        Assert.IsNull(waveGroup);
    }

    [TestCase(4)] //Wave 5 - Can spawn
    [TestCase(12)] //Wave 13 - Can spawn
    [TestCase(26)] //Wave 27 - Can spawn
    public void GenerateWave_EliteFighter_CanSpawnAtOrAfterUnlockWave(int waveIndex)
    {
        var wave = new WaveGenerator(() => 0f).GenerateWave(waveIndex);
        var waveGroup = wave.enemiesToSpawn.FirstOrDefault(e => e.prefab == Prefabs.eliteFighterPrefab);
        Assert.IsNotNull(waveGroup);
    }

    [TestCase(0)] //Wave 1 - Cant spawn
    [TestCase(8)] //Wave 9 - Cant spawn
    [TestCase(13)] //Wave 14 - Cant spawn
    [TestCase(18)] //Wave 19 - Cant spawn
    public void GenerateWave_EliteCavalier_CantSpawnBeforeUnlockWave(int waveIndex)
    {
        var wave = new WaveGenerator(() => 0f).GenerateWave(waveIndex);
        var waveGroup = wave.enemiesToSpawn.FirstOrDefault(e => e.prefab == Prefabs.eliteCavalierPrefab);
        Assert.IsNull(waveGroup);
    }

    [TestCase(21)] //Wave 22 - Can spawn
    [TestCase(28)] //Wave 29 - Can spawn
    [TestCase(33)] //Wave 34 - Can spawn
    public void GenerateWave_EliteCavalier_CanSpawnAtOrAfterUnlockWave(int waveIndex)
    {
        var wave = new WaveGenerator(() => 0f).GenerateWave(waveIndex);
        var waveGroup = wave.enemiesToSpawn.FirstOrDefault(e => e.prefab == Prefabs.eliteCavalierPrefab);
        Assert.IsNotNull(waveGroup);
    }

    [TestCase(0)] //Wave 1 - Cant spawn
    [TestCase(5)] //Wave 6 - Cant spawn
    [TestCase(7)] //Wave 8 - Cant spawn
    [TestCase(8)] //Wave 9 - Cant spawn
    public void GenerateWave_Sapper_CantSpawnBeforeUnlockWave(int waveIndex)
    {
        var wave = new WaveGenerator(() => 0f).GenerateWave(waveIndex);
        var waveGroup = wave.enemiesToSpawn.FirstOrDefault(e => e.prefab == Prefabs.sapperPrefab);
        Assert.IsNull(waveGroup);
    }

    [TestCase(10)] //Wave 11 - Can spawn
    [TestCase(18)] //Wave 19 - Can spawn
    [TestCase(24)] //Wave 25 - Can spawn
    public void GenerateWave_Sapper_CanSpawnAtOrAfterUnlockWave(int waveIndex)
    {
        var wave = new WaveGenerator(() => 0f).GenerateWave(waveIndex);
        var waveGroup = wave.enemiesToSpawn.FirstOrDefault(e => e.prefab == Prefabs.sapperPrefab);
        Assert.IsNotNull(waveGroup);
    }

    [TestCase(9)]  // Wave 10
    [TestCase(19)] // Wave 20
    [TestCase(29)] // Wave 30
    public void GenerateWave_BossWave_ContainsOnlyBoss(int waveIndex)
    {
        //Set to 0f, to make sure if elites can spawn they will
        var wave = new WaveGenerator(() => 0f).GenerateWave(waveIndex);

        var bossPrefab = Prefabs.giantPrefab;

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
        var wave = new WaveGenerator(() => 0f).GenerateWave(waveIndex);

        var assassinPrefab = Prefabs.assasinPrefab;

        var assassinGroup = wave.enemiesToSpawn.FirstOrDefault(e => e.prefab == assassinPrefab);
        Assert.That(assassinGroup, Is.Not.Null, $"Assassin missing in wave {waveIndex + 1}");

        foreach (var group in wave.enemiesToSpawn)
        {
            Assert.That(group.prefab, Is.EqualTo(assassinPrefab),
                $"Wave {waveIndex + 1} contains unexpected unit: {group.prefab.name}");
        }
    }

}
