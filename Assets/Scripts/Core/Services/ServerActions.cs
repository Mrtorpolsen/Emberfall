using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Unity.Services.Authentication;
using Unity.Services.CloudCode.GeneratedBindings;
using UnityEngine;

// Sends progress actions (BuyTalent, ...) to the Cloud Code module, which checks and applies them
// (docs/cloud-save.md), and puts the progress it returns into the save.
public static class ServerActions
{
    private const int TimeoutMilliseconds = 8000;

    // Actions need the server; offline the game shows the cached progress but can't change it. Signed in isn't
    // enough: a session that started online stays signed in when the network drops, and a call then only fails
    // after the Cloud Code package has logged an error.
    public static bool IsOnline =>
        AuthenticationService.Instance.IsSignedIn && Application.internetReachability != NetworkReachability.NotReachable;

    // The server's answer, with its progress already applied; null when the server couldn't be reached
    public static async Task<ProgressResponse> SendAsync(Func<ProgressModuleBindings, Task<string>> action)
    {
        try
        {
            string json = await CloudCall.WithTimeout(action(new ProgressModuleBindings()), TimeoutMilliseconds);
            var response = JsonConvert.DeserializeObject<ProgressResponse>(json);

            if (response != null)
            {
                ServerClock.Sync(response.ServerTime);
            }

            if (response?.Progress != null)
            {
                await ApplyAsync(response.Progress.EnsureDefaults());
            }

            if (!string.IsNullOrEmpty(response?.Error))
            {
                Debug.LogWarning($"Server refused the action: {response.Error}");
            }

            return response;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Server action failed: {e.Message}");
            return null;
        }
    }

    private static async Task ApplyAsync(ServerProgress progress)
    {
        SaveService.Instance.Current.Progress = progress;
        CurrencyManager.Instance.NotifyChanged();
        await SaveService.Instance.SaveAsync();
    }

    // A short message for the player about a refused action (ProgressResponse.Error)
    public static string Describe(string error) => error switch
    {
        nameof(TalentPurchaseCheck.NotEnoughCurrency) => "Not enough cinders.",
        nameof(TalentPurchaseCheck.MaxedOut) => "Already at the maximum level.",
        nameof(TalentPurchaseCheck.PrerequisitesNotMet) => "Requirements not met yet.",
        nameof(ResearchStartCheck.CategoryBusy) => "Something is already being researched in this category.",
        ProgressResponse.DefinitionsOutdated => "The game data was updated. Restart the game to continue.",
        ProgressResponse.OutOfSync => "Your progress was refreshed from the server.",
        ProgressResponse.Conflict => "Something went wrong. Try again.",
        _ => "That didn't work. Try again."
    };
}
