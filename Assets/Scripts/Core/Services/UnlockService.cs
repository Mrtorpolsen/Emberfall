using System.Collections.Generic;

public class UnlockService : GlobalSystem<UnlockService>
{
    private HashSet<string> unlockedItems = new HashSet<string>();
    // Default unlocks; Lock leaves these alone, e.g. a talent refund can't lock a unit that's unlocked by default
    private HashSet<string> permanentItems = new HashSet<string>();

    public bool IsUnlocked(string itemId)
    {
        return unlockedItems.Contains(itemId);
    }

    public void Unlock(string itemId)
    {
        if (!unlockedItems.Contains(itemId))
        {
            unlockedItems.Add(itemId);
        }
    }

    public void UnlockPermanently(string itemId)
    {
        permanentItems.Add(itemId);
        unlockedItems.Add(itemId);
    }

    public void Lock(string itemId)
    {
        if (permanentItems.Contains(itemId))
            return;

        unlockedItems.Remove(itemId);
    }

    public int GetUnlockedCount()
    {
        return unlockedItems.Count;
    }
}
