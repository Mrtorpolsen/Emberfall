using System.Collections.Generic;
using NUnit.Framework;

public class PendingScoreQueueTests
{
    private const string PLAYER = "player-1";

    private class MemoryStore : IPendingScoreStore
    {
        public readonly Dictionary<string, int> Values = new();

        public bool TryGet(string key, out int score) => Values.TryGetValue(key, out score);
        public void Set(string key, int score) => Values[key] = score;
        public void Delete(string key) => Values.Remove(key);
    }

    private MemoryStore store;
    private PendingScoreQueue queue;

    [SetUp]
    public void SetUp()
    {
        store = new MemoryStore();
        queue = new PendingScoreQueue(store);
    }

    [Test]
    public void Add_FirstScore_IsQueued()
    {
        Assert.IsTrue(queue.Add(PLAYER, DifficultyLevel.Hard, 5000));

        Assert.IsTrue(queue.TryGet(PLAYER, DifficultyLevel.Hard, out int score));
        Assert.AreEqual(5000, score);
    }

    [Test]
    public void Add_KeepsOnlyTheHighestPerDifficulty()
    {
        queue.Add(PLAYER, DifficultyLevel.Easy, 5000);

        Assert.IsFalse(queue.Add(PLAYER, DifficultyLevel.Easy, 4000));
        Assert.IsFalse(queue.Add(PLAYER, DifficultyLevel.Easy, 5000));
        Assert.IsTrue(queue.Add(PLAYER, DifficultyLevel.Easy, 6000));

        queue.TryGet(PLAYER, DifficultyLevel.Easy, out int score);
        Assert.AreEqual(6000, score);
    }

    [Test]
    public void Add_DifficultiesAreSeparate()
    {
        queue.Add(PLAYER, DifficultyLevel.Easy, 9000);
        queue.Add(PLAYER, DifficultyLevel.Nightmare, 1000);

        var pending = queue.GetPending(PLAYER);

        CollectionAssert.AreEqual(new[]
        {
            new KeyValuePair<DifficultyLevel, int>(DifficultyLevel.Easy, 9000),
            new KeyValuePair<DifficultyLevel, int>(DifficultyLevel.Nightmare, 1000)
        }, pending);
    }

    [Test]
    public void Add_Tutorial_IsSkipped()
    {
        Assert.IsFalse(queue.Add(PLAYER, DifficultyLevel.Tutorial, 5000));
        Assert.IsEmpty(store.Values);
    }

    [Test]
    public void Add_WithoutPlayerId_IsSkipped()
    {
        Assert.IsFalse(queue.Add(null, DifficultyLevel.Easy, 5000));
        Assert.IsFalse(queue.Add("", DifficultyLevel.Easy, 5000));
        Assert.IsEmpty(store.Values);
    }

    [Test]
    public void Players_AreKeptApart()
    {
        queue.Add(PLAYER, DifficultyLevel.Medium, 5000);

        Assert.IsFalse(queue.TryGet("player-2", DifficultyLevel.Medium, out _));
        Assert.IsEmpty(queue.GetPending("player-2"));
    }

    [Test]
    public void MarkSubmitted_RemovesTheEntry()
    {
        queue.Add(PLAYER, DifficultyLevel.Medium, 5000);

        queue.MarkSubmitted(PLAYER, DifficultyLevel.Medium, 5000);

        Assert.IsFalse(queue.TryGet(PLAYER, DifficultyLevel.Medium, out _));
    }

    [Test]
    public void MarkSubmitted_KeepsAHigherScoreQueuedDuringTheSubmit()
    {
        queue.Add(PLAYER, DifficultyLevel.Medium, 5000);
        queue.Add(PLAYER, DifficultyLevel.Medium, 7000);

        queue.MarkSubmitted(PLAYER, DifficultyLevel.Medium, 5000);

        Assert.IsTrue(queue.TryGet(PLAYER, DifficultyLevel.Medium, out int score));
        Assert.AreEqual(7000, score);
    }

    [Test]
    public void MarkSubmitted_LeavesOtherDifficulties()
    {
        queue.Add(PLAYER, DifficultyLevel.Easy, 5000);
        queue.Add(PLAYER, DifficultyLevel.Hard, 5000);

        queue.MarkSubmitted(PLAYER, DifficultyLevel.Easy, 5000);

        Assert.IsTrue(queue.TryGet(PLAYER, DifficultyLevel.Hard, out _));
    }

    [Test]
    public void Clear_RemovesOnlyThatPlayersEntries()
    {
        queue.Add(PLAYER, DifficultyLevel.Easy, 5000);
        queue.Add(PLAYER, DifficultyLevel.Nightmare, 5000);
        queue.Add("player-2", DifficultyLevel.Easy, 3000);

        queue.Clear(PLAYER);

        Assert.IsEmpty(queue.GetPending(PLAYER));
        Assert.IsTrue(queue.TryGet("player-2", DifficultyLevel.Easy, out _));
    }
}
