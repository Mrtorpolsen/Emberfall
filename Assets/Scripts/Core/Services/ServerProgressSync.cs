using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Unity.Services.Authentication;
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
        if (!AuthenticationService.Instance.IsSignedIn)
            return null;

        try
        {
            var module = new ProgressModuleBindings();
            string json = await CloudCall.WithTimeout(module.GetProgress(), TimeoutMilliseconds);

            if (string.IsNullOrEmpty(json))
            {
                Debug.Log("No server progress yet, importing the local save");
                json = await CloudCall.WithTimeout(module.ImportLocalSave(JsonConvert.SerializeObject(localProgress)), TimeoutMilliseconds);
            }

            return Parse(json);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Couldn't load server progress, using the local copy: {e.Message}");
            return null;
        }
    }

    public static ServerProgress Parse(string json)
    {
        var progress = JsonConvert.DeserializeObject<ServerProgress>(json)
            ?? throw new JsonException("Server progress is empty");

        return SaveSerializer.EnsureDefaults(progress);
    }
}
