using UnityEngine;

public class IdentityService : GameService<IdentityService>
{
    public IPlayerIdentity Current { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        Current = new UnityAuthIdentity();
    }
}
