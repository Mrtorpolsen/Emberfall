using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Unity.Services.CloudSave;
using UnityEngine;

// Syncs ClientState (loadouts, tutorial flag) with the player's "client" Cloud Save key (Default access; the game
// writes it itself, docs/cloud-save.md). Last upload wins. The JSON last uploaded is remembered per player
// (PlayerPrefs), so a local copy edited offline is uploaded instead of being replaced, while a fresh install
// (nothing uploaded yet) takes the server's copy.
public static class ClientStateSync
{
    private const string Key = "client";
    private const string LastUploadedPrefix = "ClientStateUploaded_";
    private const int TimeoutMilliseconds = 6000;
    private const int UploadDelayMilliseconds = 5000;

    private static int scheduledUploads;

    // Decides between the local and the server's client state at load. Returns the state to use, or null to keep
    // the local one (which is then uploaded if the server should have it). localIsNew: the save file was just
    // created (missing, corrupt), so the local copy holds no edits even if this device uploaded before.
    public static async Task<ClientState> LoadAsync(string playerId, ClientState local, bool localIsNew)
    {
        if (!ServerActions.IsOnline)
            return null;

        string serverJson;

        try
        {
            var items = await CloudCall.WithTimeout(CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { Key }),
                TimeoutMilliseconds);

            serverJson = items.TryGetValue(Key, out var item) ? item.Value.GetAsString() : null;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Couldn't load the client state, using the local copy: {e.Message}");
            return null;
        }

        string localJson = JsonConvert.SerializeObject(local);

        string lastUploaded = localIsNew ? null : GetLastUploaded(playerId);

        if (ShouldUseServer(serverJson, localJson, lastUploaded))
        {
            SetLastUploaded(playerId, serverJson);
            return Parse(serverJson);
        }

        _ = UploadIfChangedAsync();
        return null;
    }

    // The server's copy, unless there is none or the local one was changed since this device last uploaded
    public static bool ShouldUseServer(string serverJson, string localJson, string lastUploadedJson)
    {
        if (string.IsNullOrEmpty(serverJson))
            return false;

        bool localChanged = lastUploadedJson != null && localJson != lastUploadedJson;
        return !localChanged;
    }

    public static ClientState Parse(string json)
    {
        var state = JsonConvert.DeserializeObject<ClientState>(json) ?? new ClientState();
        state.Loadouts ??= new PlayerLoadoutCollection();
        return state;
    }

    // Uploads a few seconds after the last change, so editing a loadout doesn't upload every slot
    public static void ScheduleUpload()
    {
        int scheduled = ++scheduledUploads;

        _ = Task.Delay(UploadDelayMilliseconds).ContinueWith(_ =>
        {
            if (scheduled == scheduledUploads)
            {
                _ = UploadIfChangedAsync();
            }
        }, TaskScheduler.FromCurrentSynchronizationContext());
    }

    public static async Task UploadIfChangedAsync()
    {
        var save = SaveService.Instance != null ? SaveService.Instance.Current : null;
        string playerId = IdentityService.Instance?.Current?.GetPlayerId();

        if (save == null || string.IsNullOrEmpty(playerId) || !ServerActions.IsOnline)
            return;

        string json = JsonConvert.SerializeObject(save.Client);

        if (json == GetLastUploaded(playerId))
            return;

        try
        {
            await CloudCall.WithTimeout(CloudSaveService.Instance.Data.Player.SaveAsync(new Dictionary<string, object> { { Key, json } }),
                TimeoutMilliseconds);

            SetLastUploaded(playerId, json);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Couldn't upload the client state: {e.Message}");
        }
    }

    // Account deletion
    public static void Forget(string playerId)
    {
        PlayerPrefs.DeleteKey(LastUploadedPrefix + playerId);
    }

    private static string GetLastUploaded(string playerId)
    {
        string key = LastUploadedPrefix + playerId;
        return PlayerPrefs.HasKey(key) ? PlayerPrefs.GetString(key) : null;
    }

    private static void SetLastUploaded(string playerId, string json)
    {
        PlayerPrefs.SetString(LastUploadedPrefix + playerId, json);
        PlayerPrefs.Save();
    }
}
