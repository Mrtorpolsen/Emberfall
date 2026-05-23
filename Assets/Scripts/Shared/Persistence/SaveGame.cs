using System;
using System.Collections.Generic;

[Serializable]
public class SaveGame
{
    public const int CURRENT_SAVE_VERSION = 3;
    public int Version;

    public PlayerTalentState Talents = new();

    public PlayerResearchState Research = new();

    public CurrencyData Currency = new();

    public PlayerUnlockState Unlocks = new();
    public List<SavedLoadout> Loadouts = new();

    public bool HasReceivedLoginGift = false;
}
