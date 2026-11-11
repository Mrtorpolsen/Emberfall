#nullable disable // Unity code is nullable-oblivious; the Cloud Code module compiles this file with nullable on
using System.Collections.Generic;

// Player progress the server owns (docs/cloud-save.md): Cloud Save Protected data, written only by the
// Cloud Code module. Plain C# so the module can share it: no UnityEngine.
public class ServerProgress
{
    // Separate from the local save version: the module migrates the server copy on its own.
    // 2: RewardClock and ClaimedRunIds (run rewards). Fields are only added, with defaults, so EnsureDefaults
    // upgrades older copies.
    public const int CurrentVersion = 2;

    public int Version = CurrentVersion;

    public CurrencyData Currency = new();

    public PlayerTalentState Talents = new();

    public PlayerResearchState Research = new();

    public bool HasReceivedLoginGift;

    // Run rewards (RunRewards in the module): real time, on the server's clock (unix seconds), up to which minutes
    // of runs have been paid. 0 = never claimed.
    public long RewardClock;

    // The last claimed PendingRun.RunIds, so a retried claim pays once
    public List<string> ClaimedRunIds = new();

    // Fills sections missing from older or hand-edited progress, so callers never null-check them
    public ServerProgress EnsureDefaults()
    {
        Talents ??= new PlayerTalentState();
        Talents.Purchases ??= new Dictionary<string, UnitSaveData>();
        Talents.CurrencySpent ??= new Dictionary<CurrencyTypes, int>();

        Research ??= new PlayerResearchState();
        Research.CompletedResearch ??= new Dictionary<string, int>();
        Research.ActiveResearch ??= new List<ActiveResearch>();

        Currency ??= new CurrencyData();

        ClaimedRunIds ??= new List<string>();

        if (Version < CurrentVersion)
        {
            Version = CurrentVersion;
        }

        return this;
    }
}
