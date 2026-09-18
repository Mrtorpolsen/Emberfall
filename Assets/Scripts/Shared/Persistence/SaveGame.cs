using System;

public static class CurrentSaveVersion
{
    public const int CURRENT_SAVE_VERSION = 4;
}

[Serializable]
public class SaveGame
{
    public int Version;

    public PlayerTalentState Talents = new();

    public PlayerResearchState Research = new();

    public CurrencyData Currency = new();

    public PlayerLoadoutCollection Loadouts = new();

    public PlayerFlagsState Flags = new();
}

[Serializable]
public class SaveGame3
{
    public int Version;

    public PlayerTalentState Talents = new();

    public PlayerResearchState Research = new();

    public CurrencyData Currency = new();

    public LegacyPlayerUnlockState Unlocks = new();
    public PlayerLoadoutCollection Loadouts = new();

    public bool HasReceivedLoginGift = false;
}
