using Unity.Services.Authentication;
using UnityEngine;

public class UnityAuthIdentity : IPlayerIdentity
{
    private const string LAST_PLAYER_ID_KEY = "LastPlayerId";

    // While signed in the ID is cached, so an offline launch can keep playing on the same local save.
    // Returns null if nobody has signed in on this install yet.
    public string GetPlayerId()
    {
        if (AuthenticationService.Instance.IsSignedIn)
        {
            string playerId = AuthenticationService.Instance.PlayerId;
            PlayerPrefs.SetString(LAST_PLAYER_ID_KEY, playerId);
            PlayerPrefs.Save();
            return playerId;
        }

        return PlayerPrefs.GetString(LAST_PLAYER_ID_KEY, null);
    }

    public static void ClearCachedPlayer()
    {
        PlayerPrefs.DeleteKey(LAST_PLAYER_ID_KEY);
    }
}
