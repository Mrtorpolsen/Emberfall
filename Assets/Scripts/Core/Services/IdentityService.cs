using UnityEngine;

public class IdentityService : GlobalSystem<IdentityService>
{
    public IPlayerIdentity Current { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        Current = new UnityAuthIdentity();
    }
}
