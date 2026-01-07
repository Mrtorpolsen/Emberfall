using UnityEngine;

public class IdentityService : MonoBehaviour
{
    public static IdentityService main;
    public IPlayerIdentity Current {  get; private set; }

    private void Awake()
    {
        if(main != null)
        {
            Destroy(gameObject);
            return;
        }

        main = this;
        DontDestroyOnLoad(gameObject);

        Current = new UnityAuthIdentity();
    }
}
