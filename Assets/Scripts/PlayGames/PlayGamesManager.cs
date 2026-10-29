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

        PlayGamesPlatform.Activate();
        PlayGamesPlatform.Instance.Authenticate(status =>
        {
            if (status == SignInStatus.Success)
            {
                RequestAuthCode(result);
                return;
            }

            PlayGamesPlatform.Instance.ManuallyAuthenticate(manualStatus =>
            {
                if (manualStatus != SignInStatus.Success)
                {
                    Debug.Log("Play Games sign-in unavailable: " + manualStatus);
                    result.TrySetResult(null);
                    return;
                }

                RequestAuthCode(result);
            });
        });

        return result.Task;
#else
        return Task.FromResult<string>(null);
#endif
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    private static void RequestAuthCode(TaskCompletionSource<string> result)
    {
        PlayGamesPlatform.Instance.RequestServerSideAccess(false, code => result.TrySetResult(string.IsNullOrEmpty(code) ? null : code));
    }
#endif
}
