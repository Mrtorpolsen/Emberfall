#nullable disable // Unity code is nullable-oblivious; the Cloud Code module compiles this file with nullable on
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

// A finished run whose reward the server hasn't granted yet (offline, or the claim failed). Kept in the
// local save until the server accepts it; RunId makes a retried claim count once.
public class PendingRun
{
    public string RunId;

    [JsonConverter(typeof(StringEnumConverter))]
    public DifficultyLevel Difficulty;

    public int Minutes;

    // Unix seconds, device clock: only for display and ordering, the server times rewards itself
    public long EndedAtUtc;
}
