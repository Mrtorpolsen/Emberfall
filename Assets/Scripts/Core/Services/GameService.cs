using UnityEngine;

public abstract class GameService<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T Instance { get; private set; }

    public static T Create()
    {
        if (Instance != null)
            return Instance;

        var go = new GameObject(typeof(T).Name);
        Instance = go.AddComponent<T>();
        DontDestroyOnLoad(go);
        return Instance;
    }

    protected virtual void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this as T;
        DontDestroyOnLoad(gameObject);
    }
}
