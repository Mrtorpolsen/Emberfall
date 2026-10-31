using System.Collections.Generic;
using UnityEngine;

// Where PendingScoreQueue keeps its entries. PlayerPrefsScoreStore in the game; tests use an in-memory store.
public interface IPendingScoreStore
{
    bool TryGet(string key, out int score);
    void Set(string key, int score);
    void Delete(string key);
}

// Leaderboard scores that haven't reached the server yet (offline, or the submit failed), so they aren't lost.
// Keeps only the best score per player and difficulty; Tutorial has no leaderboard and is never queued.
// Plain C# so it can be tested without the UGS SDK.
public class PendingScoreQueue
{
    public static readonly DifficultyLevel[] RankedDifficulties =
    {
        DifficultyLevel.Easy,
        DifficultyLevel.Medium,
        DifficultyLevel.Hard,
        DifficultyLevel.Nightmare
    };

    private readonly IPendingScoreStore store;

    public PendingScoreQueue(IPendingScoreStore store)
    {
        this.store = store;
    }

    public static bool IsRanked(DifficultyLevel difficulty) => difficulty != DifficultyLevel.Tutorial;

    // Returns true if the score is now the queued one (no entry yet, or higher than the queued score)
    public bool Add(string playerId, DifficultyLevel difficulty, int score)
    {
        if (string.IsNullOrEmpty(playerId) || !IsRanked(difficulty))
        {
            return false;
        }

        string key = Key(playerId, difficulty);
        if (store.TryGet(key, out int queued) && queued >= score)
        {
            return false;
        }

        store.Set(key, score);
        return true;
    }

    public bool TryGet(string playerId, DifficultyLevel difficulty, out int score)
    {
        score = 0;
        return !string.IsNullOrEmpty(playerId) && IsRanked(difficulty) && store.TryGet(Key(playerId, difficulty), out score);
    }

    public List<KeyValuePair<DifficultyLevel, int>> GetPending(string playerId)
    {
        var pending = new List<KeyValuePair<DifficultyLevel, int>>();
        foreach (var difficulty in RankedDifficulties)
        {
            if (TryGet(playerId, difficulty, out int score))
            {
                pending.Add(new KeyValuePair<DifficultyLevel, int>(difficulty, score));
            }
        }

        return pending;
    }

    // Call after submittedScore reached the server (or the server already has a higher one). Keeps the entry if a
    // higher score was queued meanwhile, so that one still gets submitted.
    public void MarkSubmitted(string playerId, DifficultyLevel difficulty, int submittedScore)
    {
        if (TryGet(playerId, difficulty, out int queued) && queued <= submittedScore)
        {
            store.Delete(Key(playerId, difficulty));
        }
    }

    public void Clear(string playerId)
    {
        if (string.IsNullOrEmpty(playerId))
        {
            return;
        }

        foreach (var difficulty in RankedDifficulties)
        {
            store.Delete(Key(playerId, difficulty));
        }
    }

    public static string Key(string playerId, DifficultyLevel difficulty) => $"PendingScore_{playerId}_{difficulty}";
}

public class PlayerPrefsScoreStore : IPendingScoreStore
{
    public bool TryGet(string key, out int score)
    {
        if (!PlayerPrefs.HasKey(key))
        {
            score = 0;
            return false;
        }

        score = PlayerPrefs.GetInt(key);
        return true;
    }

    // Saved right away: the queue exists for cases where the app may be closed before the next chance to submit
    public void Set(string key, int score)
    {
        PlayerPrefs.SetInt(key, score);
        PlayerPrefs.Save();
    }

    public void Delete(string key)
    {
        if (!PlayerPrefs.HasKey(key))
        {
            return;
        }

        PlayerPrefs.DeleteKey(key);
        PlayerPrefs.Save();
    }
}
