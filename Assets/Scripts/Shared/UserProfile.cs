using UnityEngine;

[DisallowMultipleComponent]
public class UserProfile : MonoBehaviour
{
    public static UserProfile main;

    public string userName;
    public int currency;

    private void Awake()
    {
        if (main != null && main != this)
        {
            Destroy(gameObject);
            return;
        }

        main = this;
        DontDestroyOnLoad(gameObject);
    }
}
