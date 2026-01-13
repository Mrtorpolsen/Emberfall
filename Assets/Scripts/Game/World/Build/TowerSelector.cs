using UnityEngine;

public class TowerSelector : MonoBehaviour
{
    public static TowerSelector Instance;

    [Header("References")]
    [SerializeField] private UnitMetadata[] towerPrefabs;

    private int selectedTower = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public UnitMetadata GetSelectedTower()
    {
        return towerPrefabs[selectedTower];
    }
}
