using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// Picks between the bundled definitions JSON (Talents.json/Research.json) and the server's copy: the server's
// when it has the same schemaVersion (this build can read it) and a higher definitionsVersion (it's newer).
public static class DefinitionsSelector
{
    public static bool PreferServer(string bundledJson, string serverJson)
    {
        if (string.IsNullOrEmpty(serverJson))
            return false;

        try
        {
            JObject bundled = JObject.Parse(bundledJson);
            JObject server = JObject.Parse(serverJson);

            return server.Value<int>("schemaVersion") == bundled.Value<int>("schemaVersion") &&
                   server.Value<int>("definitionsVersion") > bundled.Value<int>("definitionsVersion");
        }
        catch (JsonException)
        {
            return false;
        }
    }

    public static string Choose(string bundledJson, string serverJson) =>
        PreferServer(bundledJson, serverJson) ? serverJson : bundledJson;
}
