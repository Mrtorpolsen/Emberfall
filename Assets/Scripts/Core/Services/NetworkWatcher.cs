using System;
using System.Threading.Tasks;
using UnityEngine;

// Notices when the device's connection comes back. A session that started online stays signed in while the
// network is gone, so IdentityService.Reconnected never fires for it; this does (NetworkRestored), so work that
// failed offline (run rewards, research completion, uploads) is retried right away instead of on a timer.
public class NetworkWatcher : GlobalSystem<NetworkWatcher>
{
    private const float CheckIntervalSeconds = 1f;

    // Android reports a connection before it's usable (DNS fails for a moment), so it must stay up this long
    private const float SettleSeconds = 3f;

    // Raised on the main thread when the device goes from no connection to a connection
    public static event Action NetworkRestored;

    private static TaskCompletionSource<bool> restored = NewRestoredSource();

    // Counts restorations, so a retry loop can tell it was woken by one
    public static int RestoredCount { get; private set; }

    private bool online = true;
    private float reachableSince = -1f;
    private float nextCheckTime;

    // Waits for the delay, or less if the connection comes back first
    public static Task WaitAsync(TimeSpan delay) => Task.WhenAny(Task.Delay(delay), restored.Task);

    private void Start()
    {
        online = IsReachable();
    }

    private void Update()
    {
        if (Time.unscaledTime < nextCheckTime)
            return;

        nextCheckTime = Time.unscaledTime + CheckIntervalSeconds;

        if (!IsReachable())
        {
            online = false;
            reachableSince = -1f;
            return;
        }

        if (online)
            return;

        if (reachableSince < 0f)
        {
            reachableSince = Time.unscaledTime;
            return;
        }

        if (Time.unscaledTime - reachableSince < SettleSeconds)
            return;

        online = true;
        RestoredCount++;
        Debug.Log("Network restored");

        var wakeUp = restored;
        restored = NewRestoredSource();
        wakeUp.TrySetResult(true);

        NetworkRestored?.Invoke();
    }

    private static bool IsReachable() => Application.internetReachability != NetworkReachability.NotReachable;

    private static TaskCompletionSource<bool> NewRestoredSource() =>
        new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
}
