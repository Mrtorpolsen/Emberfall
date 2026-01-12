using Unity.Services.Authentication;
using UnityEngine;

public class UnityAuthIdentity : IPlayerIdentity
{
    public string GetPlayerId()
    {
        if(AuthenticationService.Instance.IsSignedIn)
        {
            return AuthenticationService.Instance.PlayerId;
        }
        return "offline";
    }
}
