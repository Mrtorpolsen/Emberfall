using System.Linq;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class AccountService
{
    // Deletes the UGS account, then everything this install keeps about the player. Call RestartGame after a success.
    // Returns false if the account couldn't be deleted (e.g. offline); nothing local is touched then.
    public static async Task<bool> DeleteAccountAsync()
    {
        var auth = AuthenticationService.Instance;
        if (!auth.IsSignedIn)
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
