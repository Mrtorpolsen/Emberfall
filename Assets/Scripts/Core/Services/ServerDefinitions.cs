using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.Services.CloudCode.GeneratedBindings;
using UnityEngine;

// The talent and research definitions deployed with the Cloud Code module (the ones the server checks
// purchases against), fetched once per launch before the save loads. Null when offline or the fetch fails;
// TalentService and ResearchService then use their bundled copy (DefinitionsSelector picks).
public static class ServerDefinitions
{
    // A slow server shouldn't hold up startup; the bundled copy is fine to start with
    private const int TimeoutMilliseconds = 4000;

    public static string TalentsJson { get; private set; }
    public static string ResearchJson { get; private set; }

    public static async Task FetchAsync()
    {
        TalentsJson = null;
        ResearchJson = null;

        if (!ServerActions.IsOnline)
            return;

        try
        {
            string json = await CloudCall.WithTimeout(new ProgressModuleBindings().GetDefinitions(), TimeoutMilliseconds);

            JObject combined = JObject.Parse(json);
            TalentsJson = combined["talents"]?.ToString(Formatting.None);
            ResearchJson = combined["research"]?.ToString(Formatting.None);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Couldn't fetch definitions, using the bundled ones: {e.Message}");
        }
    }
}
