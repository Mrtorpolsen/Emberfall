#nullable disable // Unity code is nullable-oblivious; the Cloud Code module will compile this file with nullable on

// Player progress the server owns (docs/cloud-save.md): Cloud Save Protected data, written only by the
// Cloud Code module. Plain C# so the module can share it: no UnityEngine.
public class ServerProgress
{
    // Separate from the local save version: the module migrates the server copy on its own
    public const int CurrentVersion = 1;

    public int Version = CurrentVersion;

    public CurrencyData Currency = new();

    public PlayerTalentState Talents = new();

    public PlayerResearchState Research = new();

    public bool HasReceivedLoginGift;
}
