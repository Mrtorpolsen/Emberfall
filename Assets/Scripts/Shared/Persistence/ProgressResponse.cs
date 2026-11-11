#nullable disable // Unity code is nullable-oblivious; the Cloud Code module compiles this file with nullable on

// What the Cloud Code module's progress actions (BuyTalent, ...) return: the player's progress after the
// action, and an error code when it was refused. Progress can be null with an error (e.g. a write conflict);
// the game then keeps what it has. Shared with the module, so plain C#.
public class ProgressResponse
{
    public const string NoProgress = "NoProgress";
    public const string DefinitionsOutdated = "DefinitionsOutdated";
    public const string OutOfSync = "OutOfSync";
    public const string Conflict = "Conflict";

    // Null when the action was applied (or had already been applied by an earlier try)
    public string Error;

    public ServerProgress Progress;
}
