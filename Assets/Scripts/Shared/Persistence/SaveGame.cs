public static class CurrentSaveVersion
{
    public const int CURRENT_SAVE_VERSION = 4;
}

// The save is written by Newtonsoft (SaveSerializer), never by Unity, so save classes
// don't need [Serializable]. Don't mark save fields [NonSerialized] either: Newtonsoft skips them.
public class SaveGame
{
    public int Version;

    public PlayerTalentState Talents = new();

    public PlayerResearchState Research = new();

    public CurrencyData Currency = new();

    public PlayerLoadoutCollection Loadouts = new();

    public PlayerFlagsState Flags = new();
}

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
