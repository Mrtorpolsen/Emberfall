using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public class IdentityService : GlobalSystem<IdentityService>
{
    // How often a session that started offline retries signing in on its own
    private const float RECONNECT_INTERVAL_SECONDS = 120f;

    public IPlayerIdentity Current { get; private set; }

    // Raised after TryReconnectAsync signs an offline session back in (not for the normal sign-in at launch)
    public event Action Reconnected;

    private Task<bool> reconnectTask;
    private bool startedOffline;
    private float nextAutoReconnectTime;

    protected override void Awake()
    {
        base.Awake();

        Current = new UnityAuthIdentity();
    }

    // Called by LoginManager when restoring the session failed and the game plays offline as the cached player.
    // Turns on the periodic retry until a reconnect succeeds.
    public void MarkStartedOffline()
    {
        startedOffline = true;
        nextAutoReconnectTime = Time.unscaledTime + RECONNECT_INTERVAL_SECONDS;
    }

    private void Update()
    {
        if (!startedOffline || Time.unscaledTime < nextAutoReconnectTime)
        {
            return;
        }

        nextAutoReconnectTime = Time.unscaledTime + RECONNECT_INTERVAL_SECONDS;
        _ = TryReconnectAsync();
    }

    // Restores the cached session (guest or Google) if the player is signed out, e.g. after an offline launch.
    // Returns true if signed in afterwards. Only uses the saved session token: with no token it does nothing, because
    // a fresh anonymous account would orphan the player's existing one. Calls made while an attempt is running get
    // that attempt's result instead of starting another.
    public Task<bool> TryReconnectAsync()
    {
        if (reconnectTask != null && !reconnectTask.IsCompleted)
        {
            return reconnectTask;
        }

        reconnectTask = ReconnectAsync();
        return reconnectTask;
    }

    private async Task<bool> ReconnectAsync()
    {
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            return false;
        }

        var auth = AuthenticationService.Instance;
        if (auth.IsSignedIn)
        {
            return true;
        }

        if (!auth.SessionTokenExists)
        {
            startedOffline = false;
            return false;
        }

        // Read before signing in: GetPlayerId overwrites the cache once signed in
        string cachedPlayerId = UnityAuthIdentity.CachedPlayerId;

        try
        {
            await auth.SignInAnonymouslyAsync();
        }
        catch (RequestFailedException e)
        {
            // Still offline or the service is down; AuthenticationException derives from RequestFailedException
            Debug.Log("Reconnect failed: " + e.Message);

            if (!auth.SessionTokenExists)
            {
                // The SDK dropped an invalid token; retrying can't succeed without creating a new account
                startedOffline = false;
            }

            return false;
        }

        // The local save in use belongs to the cached player; don't continue on it as somebody else
        if (!string.IsNullOrEmpty(cachedPlayerId) && auth.PlayerId != cachedPlayerId)
        {
            Debug.LogError($"Reconnected as {auth.PlayerId}, but this session plays as {cachedPlayerId}; signing out");
            auth.SignOut();
            startedOffline = false;
            return false;
        }

        startedOffline = false;
        Current.GetPlayerId(); // refreshes the cached ID

        if (!string.IsNullOrEmpty(auth.PlayerName))
        {
            LoginManager.ApplyUserName(auth.PlayerName);
        }

        Debug.Log("Reconnected! PlayerID: " + auth.PlayerId);
        Reconnected?.Invoke();
        return true;
    }
}
