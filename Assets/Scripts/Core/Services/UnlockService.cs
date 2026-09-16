using System.Collections.Generic;

public class UnlockService : GlobalSystem<UnlockService>
{
    private HashSet<string> unlockedItems = new HashSet<string>();

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

}
