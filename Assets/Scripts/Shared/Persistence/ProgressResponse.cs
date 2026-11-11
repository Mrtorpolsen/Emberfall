#nullable disable // Unity code is nullable-oblivious; the Cloud Code module compiles this file with nullable on

// What the Cloud Code module's progress functions (GetProgress, BuyTalent, ...) return: the player's progress
// (after the action, or null when they have none yet), and an error code when an action was refused. Progress can be null with an error (e.g. a write conflict);
// the game then keeps what it has. Shared with the module, so plain C#.
public class ProgressResponse
{
    public const string NoProgress = "NoProgress";
    public const string DefinitionsOutdated = "DefinitionsOutdated";
    public const string OutOfSync = "OutOfSync";
    public const string Conflict = "Conflict";
    // CompleteResearch before the research's time is up on the server's clock
    public const string NotDone = "NotDone";

    // Null when the action was applied (or had already been applied by an earlier try)
    public string Error;

    public ServerProgress Progress;

    // The server's clock when it answered (unix seconds); the game times research against it (ServerClock)
    public long ServerTime;
}
