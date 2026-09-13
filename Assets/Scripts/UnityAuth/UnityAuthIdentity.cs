using Unity.Services.Authentication;

public class UnityAuthIdentity : IPlayerIdentity
{
    public string GetPlayerId()
    {
        if (AuthenticationService.Instance.IsSignedIn)
        {
            return AuthenticationService.Instance.PlayerId;
        }
        return "offline";
    }
}
