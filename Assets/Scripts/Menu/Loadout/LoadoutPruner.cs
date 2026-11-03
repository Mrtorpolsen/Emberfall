using System;

// Plain C# so it can be tested without the loadout/unlock services
public static class LoadoutPruner
{
    // Empties every slot whose ID isAvailable rejects; returns whether any slot changed
    public static bool RemoveUnavailable(PlayerLoadoutState state, Func<string, bool> isAvailable)
    {
        if (state == null)
            return false;

        bool removed = RemoveUnavailable(state.UnitLoadout, isAvailable);
        removed |= RemoveUnavailable(state.TowerLoadout, isAvailable);
        removed |= RemoveUnavailable(state.AbilityLoadout, isAvailable);
        return removed;
    }

    private static bool RemoveUnavailable(string[] slots, Func<string, bool> isAvailable)
    {
        if (slots == null)
            return false;

        bool removed = false;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null && !isAvailable(slots[i]))
            {
                slots[i] = null;
                removed = true;
            }
        }

        return removed;
    }
}
