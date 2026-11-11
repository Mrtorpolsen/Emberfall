using System;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EmberfallModule;

// The talent and research definitions embedded in the module (EmberfallModule.csproj). Read and parsed once
// per worker; the same JSON is handed to the game, so what it shows is what the server charges.
public static class DefinitionsStore
{
    static readonly Lazy<string> s_TalentsJson = new(() => ReadResource("Talents.json"));
    static readonly Lazy<string> s_ResearchJson = new(() => ReadResource("Research.json"));

    static readonly Lazy<TalentTree> s_Talents = new(() =>
    {
        var tree = JsonConvert.DeserializeObject<TalentTree>(s_TalentsJson.Value)
            ?? throw new InvalidOperationException("Talents.json is empty");
        tree.BuildTalentsByUnit();
        return tree;
    });

    static readonly Lazy<ResearchTree> s_Research = new(() =>
    {
        var tree = JsonConvert.DeserializeObject<ResearchTree>(s_ResearchJson.Value)
            ?? throw new InvalidOperationException("Research.json is empty");
        tree.NormalizeCategoryKeys();
        tree.ApplyScalingPresets();
        return tree;
    });

    // { "talents": <Talents.json>, "research": <Research.json> }
    static readonly Lazy<string> s_CombinedJson = new(() => new JObject
    {
        ["talents"] = JToken.Parse(s_TalentsJson.Value),
        ["research"] = JToken.Parse(s_ResearchJson.Value)
    }.ToString(Formatting.None));

    public static TalentTree Talents => s_Talents.Value;
    public static ResearchTree Research => s_Research.Value;
    public static string CombinedJson => s_CombinedJson.Value;

    static string ReadResource(string name)
    {
        using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name)
            ?? throw new InvalidOperationException($"Embedded resource {name} not found");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
