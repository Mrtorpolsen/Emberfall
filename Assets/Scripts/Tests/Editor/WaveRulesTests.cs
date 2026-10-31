using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class WaveRulesTests
{
    private readonly List<GeneralDefinition> generals = new();

    [SetUp]
    public void SetUp()
    {
        for (int i = 0; i < 4; i++)
        {
            var general = ScriptableObject.CreateInstance<GeneralDefinition>();
            general.generalName = $"General {i}";
            generals.Add(general);
        }
    }

    [TearDown]
    public void TearDown()
    {
        foreach (var general in generals)
            Object.DestroyImmediate(general);

        generals.Clear();
    }

    [Test]
    public void GetGeneral_EachCycleUsesEveryGeneralOnce()
    {
        var rules = new WaveRules(generals);

        for (int cycle = 0; cycle < 5; cycle++)
        {
            var seen = new HashSet<GeneralDefinition>();

            for (int i = 0; i < generals.Count; i++)
                Assert.That(seen.Add(rules.GetGeneral()), Is.True, $"General repeated within cycle {cycle}.");

            Assert.That(seen.Count, Is.EqualTo(generals.Count));
        }
    }

    [Test]
    public void GetGeneral_NewCycleNeverStartsWithLastGeneral()
    {
        var rules = new WaveRules(generals);
        GeneralDefinition previous = null;

        // Many cycles so a random repeat at the loop point would show up
        for (int pick = 0; pick < generals.Count * 200; pick++)
        {
            var general = rules.GetGeneral();
            Assert.That(general, Is.Not.SameAs(previous), $"Same general picked twice in a row at pick {pick}.");
            previous = general;
        }
    }

    [Test]
    public void GetGeneral_SingleGeneral_KeepsLooping()
    {
        var single = new List<GeneralDefinition> { generals[0] };
        var rules = new WaveRules(single);

        for (int i = 0; i < 3; i++)
            Assert.That(rules.GetGeneral(), Is.SameAs(generals[0]));
    }
}
