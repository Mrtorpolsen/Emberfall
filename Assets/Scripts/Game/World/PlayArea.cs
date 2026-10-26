using UnityEngine;

// The area inside the walls; units and projectiles found outside it are removed
public class PlayArea : MonoBehaviour
{
    public static PlayArea Instance { get; private set; }

    // Trigger outlining the inside of the walls, on a layer that collides with nothing
    [SerializeField] private Collider2D area;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    // Without a play area everything counts as inside, so nothing is removed by mistake
    public static bool Contains(Vector2 position)
    {
        return Instance == null || Instance.area.OverlapPoint(position);
    }
}
