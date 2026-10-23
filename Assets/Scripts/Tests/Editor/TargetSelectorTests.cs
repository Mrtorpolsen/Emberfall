using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class TargetSelectorTests
{
    private readonly List<GameObject> created = new();

    // Minimal target: only position and priority matter to TargetSelector
    private class FakeTarget : ITargetable
    {
        public FakeTarget(GameObject go, ThreatLevel prio)
        {
            GameObject = go;
            UnitPrio = prio;
        }

        public GameObject GameObject { get; }
        public Transform Transform => GameObject.transform;
        public ThreatLevel UnitPrio { get; }
        public Team Team => Team.North;
        public float HitRadius => 0f;
        public bool IsAlive => true;
        public bool IsTargetable => true;
        public bool IsInvulnerable => false;
        public void TakeDamage(int amount) { }
        public void Die() { }
    }

    private FakeTarget Target(float x, ThreatLevel prio = ThreatLevel.Normal)
    {
        var go = new GameObject("Target");
        go.transform.position = new Vector3(x, 0f, 0f);
        created.Add(go);
        return new FakeTarget(go, prio);
    }

    [TearDown]
    public void TearDown()
    {
        foreach (var go in created)
        {
            Object.DestroyImmediate(go);
        }
        created.Clear();
    }

    [Test]
    public void NoTargets_ReturnsNull()
    {
        Assert.IsNull(new TargetSelector().SelectTarget(new List<ITargetable>(), Vector2.zero));
    }

    [Test]
    public void NoPriorities_PicksClosest()
    {
        var near = Target(1f);
        var far = Target(5f);

        var result = new TargetSelector().SelectTarget(new List<ITargetable> { far, near }, Vector2.zero);

        Assert.AreSame(near, result);
    }

    [Test]
    public void Priorities_PickHigherPriorityEvenIfFarther()
    {
        var nearNormal = Target(1f, ThreatLevel.Normal);
        var farImmediate = Target(5f, ThreatLevel.Immediate);
        var priorities = new List<ThreatLevel> { ThreatLevel.Immediate, ThreatLevel.Normal };

        var result = new TargetSelector().SelectTarget(new List<ITargetable> { nearNormal, farImmediate }, Vector2.zero, priorities);

        Assert.AreSame(farImmediate, result);
    }

    [Test]
    public void Priorities_SamePriority_PicksClosest()
    {
        var farImmediate = Target(5f, ThreatLevel.Immediate);
        var nearImmediate = Target(2f, ThreatLevel.Immediate);
        var priorities = new List<ThreatLevel> { ThreatLevel.Immediate };

        var result = new TargetSelector().SelectTarget(new List<ITargetable> { farImmediate, nearImmediate }, Vector2.zero, priorities);

        Assert.AreSame(nearImmediate, result);
    }

    [Test]
    public void Priorities_NoneInRange_FallsBackToClosest()
    {
        var near = Target(1f, ThreatLevel.Normal);
        var far = Target(5f, ThreatLevel.High);
        var priorities = new List<ThreatLevel> { ThreatLevel.Immediate };

        var result = new TargetSelector().SelectTarget(new List<ITargetable> { far, near }, Vector2.zero, priorities);

        Assert.AreSame(near, result);
    }
}
