using UnityEngine;

public class TowerSelector : MonoBehaviour
{
    public static TowerSelector Instance;

    [Header("References")]
    [SerializeField] private UnitStats[] towerPrefabs;

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
    
    public UnitStats GetSelectedTower()
    {
        return towerPrefabs[selectedTower];
    }
}
