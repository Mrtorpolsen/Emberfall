using System;

[Serializable]
public class SaveGame
{
    public const int CURRENT_SAVE_VERSION = 3;
    public int Version;

    public PlayerTalentState Talents = new();

    public PlayerResearchState Research = new();

    public CurrencyData Currency = new();

    public PlayerUnlockState Unlocks = new();
    public PlayerLoadoutCollection Loadouts = new();

    public bool HasReceivedLoginGift = false;
}
