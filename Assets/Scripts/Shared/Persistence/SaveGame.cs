using System.Collections.Generic;

public static class CurrentSaveVersion
{
    public const int CURRENT_SAVE_VERSION = 5;
}

// The local save file. The save is written by Newtonsoft (SaveSerializer), never by Unity, so save classes
// don't need [Serializable]. Don't mark save fields [NonSerialized] either: Newtonsoft skips them.
//
// Since v5 it is split by who owns each part (docs/cloud-save.md): Progress will be written only by the
// server (the file keeps the last server copy), Client by the game, and PendingRuns are run rewards
// the server hasn't granted yet.
public class SaveGame
{
    public int Version;

    public ServerProgress Progress = new();

    public ClientState Client = new();

    public List<PendingRun> PendingRuns = new();
}

public class SaveGame4
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
