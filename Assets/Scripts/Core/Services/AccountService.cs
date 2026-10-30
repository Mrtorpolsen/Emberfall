using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.CloudCode;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum LinkResult
{
    Linked,
    Cancelled,
    AlreadyLinkedElsewhere,
    Failed
}

public static class AccountService
{
    private const string DELETE_SCORES_SCRIPT = "DeletePlayerScores";

    // A guest (anonymous) account has no external sign-ins linked to it
    public static bool IsGuest
    {
        get
        {
            var auth = AuthenticationService.Instance;
            return auth.IsSignedIn && (auth.PlayerInfo?.Identities == null || auth.PlayerInfo.Identities.Count == 0);
        }
    }

    // Links Play Games to the signed-in (guest) account, so its progress and leaderboard entries can be recovered on
    // any device. If that Play Games account already belongs to another player, nothing changes: the player stays on
    // this device's account (no cloud save yet, so the other account's progress isn't here anyway).
    public static async Task<LinkResult> LinkPlayGamesAsync()
    {
        string authCode = await PlayGamesManager.GetServerAuthCodeAsync();
        if (authCode == null)
        {
            return LinkResult.Cancelled;
        }

        try
        {
            await AuthenticationService.Instance.LinkWithGooglePlayGamesAsync(authCode);
            Debug.Log("Linked Play Games to PlayerID: " + AuthenticationService.Instance.PlayerId);
            return LinkResult.Linked;
        }
        catch (AuthenticationException e) when (e.ErrorCode == AuthenticationErrorCodes.AccountAlreadyLinked)
        {
            Debug.LogWarning("Play Games account is already linked to another player: " + e);
            return LinkResult.AlreadyLinkedElsewhere;
        }
        catch (RequestFailedException e)
        {
            Debug.LogWarning("Linking Play Games failed: " + e);
            return LinkResult.Failed;
        }
    }

    // Deletes the player's leaderboard scores and the UGS account, then everything this install keeps about the player.
    // Call RestartGame after a success. Returns false if either deletion failed (e.g. offline); the account and
    // everything local are kept then.
    public static async Task<bool> DeleteAccountAsync()
    {
        var auth = AuthenticationService.Instance;
        if (!auth.IsSignedIn)
        {
            return false;
        }

        // Scores first, while still signed in: Cloud Code identifies the player by the session
        if (!await DeleteLeaderboardScoresAsync())
        {
            return false;
        }

        try
        {
            await auth.DeleteAccountAsync();
        }
        catch (RequestFailedException e)
        {
            Debug.LogWarning("Deleting the account failed: " + e);
            return false;
        }

        Debug.Log("Account deleted");

        await SaveService.Instance.DeleteSaveFilesAsync();
        UnityAuthIdentity.ClearCachedPlayer();
        PlayerPrefs.DeleteKey(LoginManager.LAST_PLAYER_NAME_KEY);
        PlayerPrefs.Save();

        if (auth.IsSignedIn)
        {
            auth.SignOut(clearCredentials: true);
        }
        else
        {
            auth.ClearSessionToken();
        }

        return true;
    }

    // Runs the DeletePlayerScores Cloud Code script (CloudCode/DeletePlayerScores.js); the client SDK can't delete scores
    private static async Task<bool> DeleteLeaderboardScoresAsync()
    {
        try
        {
            await CloudCodeService.Instance.CallEndpointAsync(DELETE_SCORES_SCRIPT, new Dictionary<string, object>());
            Debug.Log("Leaderboard scores deleted");
            return true;
        }
        catch (CloudCodeException e)
        {
            Debug.LogWarning("Deleting leaderboard scores failed: " + e);
            return false;
        }
        catch (RequestFailedException e)
        {
            Debug.LogWarning("Deleting leaderboard scores failed: " + e);
            return false;
        }
    }

    // Destroys our persistent objects and starts over from Bootstrap, so no service keeps the deleted player's data.
    // Objects without our scripts (Unity Services, Addressables) are left alone; they don't hold player data.
    public static void RestartGame()
    {
        var probe = new GameObject("RestartProbe");
        Object.DontDestroyOnLoad(probe);

        var ourAssembly = typeof(AccountService).Assembly;

        foreach (var root in probe.scene.GetRootGameObjects())
        {
            if (root == probe || root.GetComponents<MonoBehaviour>().Any(c => c != null && c.GetType().Assembly == ourAssembly))
            {
                Object.Destroy(root);
            }
        }

        SceneManager.LoadScene("Bootstrap");
    }
}
