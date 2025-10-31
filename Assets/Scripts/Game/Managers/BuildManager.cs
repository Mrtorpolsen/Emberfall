using UnityEngine;

public class BuildManager : MonoBehaviour
{
    public static BuildManager main;

    [Header("References")]
    [SerializeField] private UnitStats[] towerPrefabs;

    private int selectedTower = 0;

    private void Awake()
    {
        main = this;
    }
    
    public UnitStats GetSelectedTower()
    {
        return towerPrefabs[selectedTower];
    }
}
