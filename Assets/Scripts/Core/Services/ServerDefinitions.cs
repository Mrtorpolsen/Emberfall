using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.Services.Authentication;
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

        if (!AuthenticationService.Instance.IsSignedIn)
            return;

        try
        {
            Task<string> call = new ProgressModuleBindings().GetDefinitions();

            if (await Task.WhenAny(call, Task.Delay(TimeoutMilliseconds)) != call)
            {
                Debug.LogWarning("Fetching definitions timed out, using the bundled ones");
                _ = call.ContinueWith(t => _ = t.Exception, TaskContinuationOptions.OnlyOnFaulted);
                return;
            }

            JObject combined = JObject.Parse(await call);
            TalentsJson = combined["talents"]?.ToString(Formatting.None);
            ResearchJson = combined["research"]?.ToString(Formatting.None);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Couldn't fetch definitions, using the bundled ones: {e.Message}");
        }
    }
}
