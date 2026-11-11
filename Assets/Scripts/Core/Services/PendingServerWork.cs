// Server work that waits for a connection: queued run rewards, the tutorial gift and client state changes.
// Retried when an offline session signs back in (IdentityService.Reconnected) or the network comes back
// mid-session (NetworkWatcher.NetworkRestored). ResearchService retries research completion the same way.
public static class PendingServerWork
{
    private static bool listeningToNetwork;
    // AccountService.RestartGame recreates IdentityService, so the subscription is per instance
    private static IdentityService listeningToIdentity;

    // Called at every start (Bootstrapper); subscribes once per IdentityService
    public static void Listen()
    {
        if (!listeningToNetwork)
        {
            NetworkWatcher.NetworkRestored += RetryAll;
            listeningToNetwork = true;
        }

        var identity = IdentityService.Instance;

        if (identity != null && identity != listeningToIdentity)
        {
            identity.Reconnected += RetryAll;
            listeningToIdentity = identity;
        }
    }

    public static void RetryAll()
    {
        _ = RunRewardQueue.ClaimAsync();
        _ = ClientStateSync.UploadIfChangedAsync();

        // The gameplay tutorial (and its gift claim) comes before the menu tutorial ends
        if (SaveService.Instance != null && SaveService.Instance.Current?.Client.HasCompletedTutorial == true)
        {
            _ = TutorialGift.ClaimIfDueAsync();
        }
    }
}
