using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Unity.Services.CloudCode.GeneratedBindings;
using UnityEngine;

// Gets the player's progress from the server (docs/cloud-save.md), where only the Cloud Code module writes it.
// On the first cloud login the local progress is imported once; the module limits it to what's possible.
public static class ServerProgressSync
{
    private const int TimeoutMilliseconds = 6000;

    // The server's progress, or null when signed out or the server can't be reached (then the local copy is used)
    public static async Task<ServerProgress> LoadOrImportAsync(ServerProgress localProgress)
    {
        if (!ServerActions.IsOnline)
            return null;

        try
        {
            var module = new ProgressModuleBindings();
            ProgressResponse response = ParseResponse(await CloudCall.WithTimeout(module.GetProgress(), TimeoutMilliseconds));

            if (response.Progress == null)
            {
                Debug.Log("No server progress yet, importing the local save");
                string localJson = JsonConvert.SerializeObject(localProgress);
                response = ParseResponse(await CloudCall.WithTimeout(module.ImportLocalSave(localJson), TimeoutMilliseconds));
            }

            ServerClock.Sync(response.ServerTime);

            return response.Progress?.EnsureDefaults() ?? throw new JsonException("The server returned no progress");
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Couldn't load server progress, using the local copy: {e.Message}");
            return null;
        }
    }

    public static ProgressResponse ParseResponse(string json)
    {
        var response = JsonConvert.DeserializeObject<ProgressResponse>(json)
            ?? throw new JsonException("Empty answer from the server");

        response.Progress?.EnsureDefaults();
        return response;
    }
}
