using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class WaveGeneratorTests
{
    private GameObject _spawnDatabaseObject;
    private List<SpawnDefinition> _spawnDefinitions;
    private SpawnDatabase _spawnDB;

    private WaveThreatCalculator _waveThreatCalculator;

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

        _waveThreatCalculator =
            new WaveThreatCalculator(Difficulties.Medium);
    }

    [TearDown]
    public void TearDown()
    {
        UnityEngine.Object.DestroyImmediate(_spawnDatabaseObject);
    }

    private WaveGenerator CreateGenerator(Func<float> randomFunc = null)
    {
        return new WaveGenerator(
            _spawnDB,
            _waveThreatCalculator,
            randomFunc);
    }

    private GeneralDefinition CreateGeneral(
        string name,
        SpawnDefinition generalUnit,
        List<SpawnDefinition> roster = null)
    {
        var general = ScriptableObject.CreateInstance<GeneralDefinition>();

        general.generalName = name;
        general.generalUnit = generalUnit;
        general.unitRoster = roster ?? new List<SpawnDefinition>();

        return general;
    }

    private void DestroyGeneral(GeneralDefinition general)
    {
        if (general != null)
        {
            UnityEngine.Object.DestroyImmediate(general);
        }
    }

    private float CalculateWaveThreat(
        WaveDefinition wave,
        GeneralDefinition general)
    {
        float totalThreat = 0f;

        foreach (var group in wave.enemiesToSpawn)
        {
            // General/boss is intentionally outside the normal threat budget.
            if (group.isBoss)
                continue;

            var spawnDefinition = _spawnDefinitions.First(
                spawn => spawn.UnitPrefab == group.prefab);

            var unitThreat =
                new ThreatCalculator().CalculateThreat(
                    spawnDefinition.Stats);

            totalThreat += unitThreat * group.count;
        }

        return totalThreat;
    }


    // ------------------------------------------------------------------------
    // General
    // ------------------------------------------------------------------------

    [Test]
    public void GenerateWave_FirstWave_AddsGeneral()
    {
        var generalUnit = _spawnDB.GetSpawn("spawn_giant");
        var general = CreateGeneral(
            "Test General",
            generalUnit);

        try
        {
            var generator = CreateGenerator();

            var wave = generator.GenerateWave(0, general);

            var bossGroup = wave.enemiesToSpawn
                .FirstOrDefault(group => group.isBoss);

            Assert.That(bossGroup, Is.Not.Null);
            Assert.That(bossGroup.prefab, Is.EqualTo(generalUnit.UnitPrefab));
            Assert.That(bossGroup.count, Is.EqualTo(1));
        }
        finally
        {
            DestroyGeneral(general);
        }
    }

    [Test]
    public void GenerateWave_NewGeneral_AddsNewGeneral()
    {
        var generalUnitA = _spawnDB.GetSpawn("spawn_giant");
        var generalUnitB = _spawnDB.GetSpawn("spawn_assassin");

        var generalA = CreateGeneral(
            "General A",
            generalUnitA);

        var generalB = CreateGeneral(
            "General B",
            generalUnitB);

        try
        {
            var generator = CreateGenerator();

            var firstWave = generator.GenerateWave(0, generalA);
            var secondWave = generator.GenerateWave(1, generalB);

            var firstBoss = firstWave.enemiesToSpawn
                .FirstOrDefault(group => group.isBoss);

            var secondBoss = secondWave.enemiesToSpawn
                .FirstOrDefault(group => group.isBoss);

            Assert.That(firstBoss, Is.Not.Null);
            Assert.That(firstBoss.prefab, Is.EqualTo(generalUnitA.UnitPrefab));

            Assert.That(secondBoss, Is.Not.Null);
            Assert.That(secondBoss.prefab, Is.EqualTo(generalUnitB.UnitPrefab));
        }
        finally
        {
            DestroyGeneral(generalA);
            DestroyGeneral(generalB);
        }
    }


    // ------------------------------------------------------------------------
    // Normal waves - threat system
    // ------------------------------------------------------------------------

    [Test]
    public void GenerateWave_NormalWave_DoesNotExceedThreatBudget()
    {
        var fighter = _spawnDB.GetSpawn("spawn_fighter");

        var general = CreateGeneral(
            "Test General",
            fighter,
            new List<SpawnDefinition>
            {
                fighter
            });

        try
        {
            var generator = CreateGenerator();

            const int waveNumber = 0;

            var wave = generator.GenerateWave(
                waveNumber,
                general);

            var generatedThreat = CalculateWaveThreat(
                wave,
                general);

            var threatBudget =
                _waveThreatCalculator.GetThreatValueForWave(
                    waveNumber);

            Assert.That(
                generatedThreat,
                Is.LessThanOrEqualTo(threatBudget),
                $"Generated threat ({generatedThreat}) exceeded " +
                $"wave budget ({threatBudget}).");
        }
        finally
        {
            DestroyGeneral(general);
        }
    }

    [Test]
    public void GenerateWave_NormalWave_OnlyContainsRosterUnits()
    {
        var fighter = _spawnDB.GetSpawn("spawn_fighter");
        var ranger = _spawnDB.GetSpawn("spawn_ranger");

        var roster = new List<SpawnDefinition>
        {
            fighter,
            ranger
        };

        var general = CreateGeneral(
            "Test General",
            fighter,
            roster);

        try
        {
            var generator = CreateGenerator();

            var wave = generator.GenerateWave(
                0,
                general);

            foreach (var group in wave.enemiesToSpawn)
            {
                if (group.isBoss)
                    continue;

                Assert.That(
                    roster.Any(unit =>
                        unit.UnitPrefab == group.prefab),
                    Is.True,
                    $"Unexpected unit spawned: {group.prefab.name}");
            }
        }
        finally
        {
            DestroyGeneral(general);
        }
    }

    [Test]
    public void GenerateWave_NormalWave_GeneratedUnitGroupsHaveCountOne()
    {
        var fighter = _spawnDB.GetSpawn("spawn_fighter");
        var ranger = _spawnDB.GetSpawn("spawn_ranger");

        var general = CreateGeneral(
            "Test General",
            fighter,
            new List<SpawnDefinition>
            {
                fighter,
                ranger
            });

        try
        {
            var generator = CreateGenerator();

            var wave = generator.GenerateWave(
                0,
                general);

            foreach (var group in wave.enemiesToSpawn)
            {
                if (group.isBoss)
                    continue;

                Assert.That(
                    group.count,
                    Is.EqualTo(1),
                    $"Normal unit {group.prefab.name} " +
                    $"should be added with count 1.");
            }
        }
        finally
        {
            DestroyGeneral(general);
        }
    }

    [Test]
    public void GenerateWave_NormalWave_StopsWhenNoRosterUnitFitsRemainingThreat()
    {
        var fighter = _spawnDB.GetSpawn("spawn_fighter");

        var fighterThreat =
            new ThreatCalculator().CalculateThreat(
                fighter.Stats);

        /*
         * Use a single roster unit.
         *
         * The generator can only add Fighters while the remaining
         * threat budget is large enough. Once it is below the Fighter
         * threat, generation must stop.
         */
        var general = CreateGeneral(
            "Test General",
            fighter,
            new List<SpawnDefinition>
            {
                fighter
            });

        try
        {
            var generator = CreateGenerator();

            var wave = generator.GenerateWave(
                0,
                general);

            var generatedThreat = CalculateWaveThreat(
                wave,
                general);

            var threatBudget =
                _waveThreatCalculator.GetThreatValueForWave(0);

            var remainingThreat =
                threatBudget - generatedThreat;

            Assert.That(
                remainingThreat,
                Is.LessThan(fighterThreat),
                $"Remaining threat ({remainingThreat}) was large enough " +
                $"for another Fighter ({fighterThreat}), so generation " +
                $"should not have stopped.");
        }
        finally
        {
            DestroyGeneral(general);
        }
    }


    // ------------------------------------------------------------------------
    // Assassin waves
    // ------------------------------------------------------------------------

    [TestCase(20)] // Wave 21
    [TestCase(27)] // Wave 28
    [TestCase(34)] // Wave 35
    public void GenerateWave_AssassinWave_ContainsOnlyAssassins(
        int waveNumber)
    {
        var assassin = _spawnDB.GetSpawn("spawn_assassin");

        var general = CreateGeneral(
            "Test General",
            assassin,
            new List<SpawnDefinition>
            {
                assassin
            });

        try
        {
            var generator = CreateGenerator();

            var wave = generator.GenerateWave(
                waveNumber,
                general);

            var assassinPrefab = assassin.UnitPrefab;

            Assert.That(
                wave.enemiesToSpawn.Any(
                    group => group.prefab == assassinPrefab),
                Is.True,
                $"Assassin missing in wave {waveNumber + 1}.");

            foreach (var group in wave.enemiesToSpawn)
            {
                if (group.isBoss)
                    continue;

                Assert.That(
                    group.prefab,
                    Is.EqualTo(assassinPrefab),
                    $"Wave {waveNumber + 1} contains " +
                    $"unexpected unit: {group.prefab.name}");
            }
        }
        finally
        {
            DestroyGeneral(general);
        }
    }

[TestCase(20)] // Wave 21
[TestCase(27)] // Wave 28
[TestCase(34)] // Wave 35
public void GenerateWave_AssassinWave_CountIsBasedOnThreatBudget(
    int waveNumber)
{
    var assassin = _spawnDB.GetSpawn("spawn_assassin");
    var fighter = _spawnDB.GetSpawn("spawn_fighter");

    var assassinThreat =
        new ThreatCalculator().CalculateThreat(
            assassin.Stats);

    var general = CreateGeneral(
        "Test General",
        fighter,
        new List<SpawnDefinition>
        {
            fighter
        });

    try
    {
        var generator = CreateGenerator();

        // Establish the current general.
        generator.GenerateWave(0, general);

        // Generate the assassin wave using the same general.
        var wave = generator.GenerateWave(
            waveNumber,
            general);

        var assassinGroup = wave.enemiesToSpawn
            .FirstOrDefault(group =>
                group.prefab == assassin.UnitPrefab);

        Assert.That(
            assassinGroup,
            Is.Not.Null,
            $"Assassin group missing in wave {waveNumber + 1}.");

        var threatBudget =
            _waveThreatCalculator.GetThreatValueForWave(
                waveNumber);

        var expectedCount =
            Mathf.FloorToInt(
                threatBudget / assassinThreat);

        Assert.That(
            assassinGroup.count,
            Is.EqualTo(expectedCount));
    }
    finally
    {
        DestroyGeneral(general);
    }
}

    [TestCase(19)] // Wave 20
    [TestCase(26)] // Wave 27
    [TestCase(33)] // Wave 34
    public void GenerateWave_BeforeAssassinWave_DoesNotSpawnAssassins(
        int waveNumber)
    {
        var assassin = _spawnDB.GetSpawn("spawn_assassin");
        var fighter = _spawnDB.GetSpawn("spawn_fighter");

        var general = CreateGeneral(
            "Test General",
            fighter,
            new List<SpawnDefinition>
            {
                fighter
            });

        try
        {
            var generator = CreateGenerator();

            var wave = generator.GenerateWave(
                waveNumber,
                general);

            Assert.That(
                wave.enemiesToSpawn.Any(
                    group => group.prefab == assassin.UnitPrefab),
                Is.False,
                $"Assassin should not spawn on wave {waveNumber + 1}.");
        }
        finally
        {
            DestroyGeneral(general);
        }
    }

    // ------------------------------------------------------------------------
    // Sappers
    // ------------------------------------------------------------------------

    [TestCase(0)] // Wave 1
    [TestCase(9)] // Wave 10
    public void GenerateWave_Sapper_CannotSpawnBeforeWave11(
        int waveNumber)
    {
        var fighter = _spawnDB.GetSpawn("spawn_fighter");

        var general = CreateGeneral(
            "Test General",
            fighter,
            new List<SpawnDefinition>
            {
                fighter
            });

        try
        {
            var generator = CreateGenerator(
                () => 0f);

            var wave = generator.GenerateWave(
                waveNumber,
                general);

            var sapperPrefab =
                _spawnDB.GetSpawn("spawn_sapper").UnitPrefab;

            Assert.That(
                wave.enemiesToSpawn.Any(
                    group => group.prefab == sapperPrefab),
                Is.False);
        }
        finally
        {
            DestroyGeneral(general);
        }
    }

    [Test]
    public void GenerateWave_Sapper_CanSpawnAfterWave10()
    {
        var fighter = _spawnDB.GetSpawn("spawn_fighter");
        var sapper = _spawnDB.GetSpawn("spawn_sapper");

        var general = CreateGeneral(
            "Test General",
            fighter,
            new List<SpawnDefinition>
            {
                fighter
            });

        try
        {
            var generator = CreateGenerator(
                () => 0f);

            // Wave 11
            var wave = generator.GenerateWave(
                10,
                general);

            Assert.That(
                wave.enemiesToSpawn.Any(
                    group => group.prefab == sapper.UnitPrefab),
                Is.True);
        }
        finally
        {
            DestroyGeneral(general);
        }
    }

    [Test]
    public void GenerateWave_Sapper_DoesNotSpawnWhenRandomCheckFails()
    {
        var fighter = _spawnDB.GetSpawn("spawn_fighter");
        var sapper = _spawnDB.GetSpawn("spawn_sapper");

        var general = CreateGeneral(
            "Test General",
            fighter,
            new List<SpawnDefinition>
            {
                fighter
            });

        try
        {
            var generator = CreateGenerator(
                () => 1f);

            // Wave 11
            var wave = generator.GenerateWave(
                10,
                general);

            Assert.That(
                wave.enemiesToSpawn.Any(
                    group => group.prefab == sapper.UnitPrefab),
                Is.False);
        }
        finally
        {
            DestroyGeneral(general);
        }
    }

    [Test]
    public void GenerateWave_Sapper_DoesNotSpawnOnAssassinWave()
    {
        var fighter = _spawnDB.GetSpawn("spawn_fighter");
        var assassin = _spawnDB.GetSpawn("spawn_assassin");
        var sapper = _spawnDB.GetSpawn("spawn_sapper");

        var general = CreateGeneral(
            "Test General",
            fighter,
            new List<SpawnDefinition>
            {
                fighter
            });

        try
        {
            var generator = CreateGenerator(
                () => 0f);

            // Wave 21
            var wave = generator.GenerateWave(
                20,
                general);

            Assert.That(
                wave.enemiesToSpawn.Any(
                    group => group.prefab == sapper.UnitPrefab),
                Is.False);

            Assert.That(
                wave.enemiesToSpawn.Any(
                    group => group.prefab == assassin.UnitPrefab),
                Is.True);
        }
        finally
        {
            DestroyGeneral(general);
        }
    }

    [Test]
    public void GenerateWave_Sapper_RespectsCooldown()
    {
        var fighter = _spawnDB.GetSpawn("spawn_fighter");
        var sapper = _spawnDB.GetSpawn("spawn_sapper");

        var general = CreateGeneral(
            "Test General",
            fighter,
            new List<SpawnDefinition>
            {
                fighter
            });

        try
        {
            var generator = CreateGenerator(
                () => 0f);

            // Wave 11 - Sapper spawns.
            var wave11 = generator.GenerateWave(
                10,
                general);

            Assert.That(
                wave11.enemiesToSpawn.Any(
                    group => group.prefab == sapper.UnitPrefab),
                Is.True);

            // Waves 12-14 should be on cooldown.
            for (int waveNumber = 11; waveNumber <= 13; waveNumber++)
            {
                var wave = generator.GenerateWave(
                    waveNumber,
                    general);

                Assert.That(
                    wave.enemiesToSpawn.Any(
                        group => group.prefab == sapper.UnitPrefab),
                    Is.False,
                    $"Sapper should still be on cooldown in wave {waveNumber + 1}.");
            }

            // Wave 15 should be eligible again.
            var wave15 = generator.GenerateWave(
                14,
                general);

            Assert.That(
                wave15.enemiesToSpawn.Any(
                    group => group.prefab == sapper.UnitPrefab),
                Is.True);
        }
        finally
        {
            DestroyGeneral(general);
        }
    }

    // ------------------------------------------------------------------------
    // Milestones - Currently only used for Assassin waves, but could be used for other things in the future.
    // ------------------------------------------------------------------------

    [TestCase(9, false)]   // Wave 10
    [TestCase(19, false)]  // Wave 20
    [TestCase(20, true)] // Wave 21
    [TestCase(21, false)] // Wave 22
    [TestCase(26, false)] // Wave 27
    [TestCase(27, true)]  // Wave 28
    [TestCase(33, false)] // Wave 34
    [TestCase(34, true)]  // Wave 35
    public void IsMilestone_ReturnsExpectedResult(
        int waveNumber,
        bool expected)
    {
        var result = WaveGenerator.IsMilestone(
            waveNumber,
            7,
            0,
            21);

        Assert.That(result, Is.EqualTo(expected));
    }
}
