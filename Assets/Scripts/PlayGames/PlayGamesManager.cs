using System.Threading.Tasks;
using UnityEngine;
#if UNITY_ANDROID && !UNITY_EDITOR
using GooglePlayGames;
using GooglePlayGames.BasicApi;
#endif

// Fetches a one-time server auth code from Google Play Games v2 for Unity Authentication.
// Only works in an Android device build.
public static class PlayGamesManager
{
#if UNITY_ANDROID && !UNITY_EDITOR
    public static bool IsSupported => true;
#else
    public static bool IsSupported => false;
#endif

    // Uses the automatic launch sign-in if it succeeded, otherwise prompts the player.
    // Returns null if Play Games isn't available or the player cancels.
    public static Task<string> GetServerAuthCodeAsync()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        var result = new TaskCompletionSource<string>();

        // The plugin throws on misconfiguration (e.g. a missing web client ID); without the guards the task would never complete
        Guarded(result, () =>
        {
            PlayGamesPlatform.Activate();
            PlayGamesPlatform.Instance.Authenticate(status => Guarded(result, () =>
            {
                if (status == SignInStatus.Success)
                {
                    RequestAuthCode(result);
                    return;
                }

                PlayGamesPlatform.Instance.ManuallyAuthenticate(manualStatus => Guarded(result, () =>
                {
                    if (manualStatus != SignInStatus.Success)
                    {
                        Debug.Log("Play Games sign-in unavailable: " + manualStatus);
                        result.TrySetResult(null);
                        return;
                    }

                    RequestAuthCode(result);
                }));
            }));
        });

        return result.Task;
#else
        return Task.FromResult<string>(null);
#endif
    }

    // The signed-in player's Play Games name, or null
    public static string GetDisplayName()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        return PlayGamesPlatform.Instance.IsAuthenticated() ? PlayGamesPlatform.Instance.GetUserDisplayName() : null;
#else
        return null;
#endif
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    private static void RequestAuthCode(TaskCompletionSource<string> result)
    {
        PlayGamesPlatform.Instance.RequestServerSideAccess(false, code => result.TrySetResult(string.IsNullOrEmpty(code) ? null : code));
    }

    private static void Guarded(TaskCompletionSource<string> result, System.Action action)
    {
        try
        {
            action();
        }
        catch (System.Exception e)
        {
            Debug.LogError("Play Games sign-in failed: " + e);
            result.TrySetResult(null);
        }
    }
#endif
}
