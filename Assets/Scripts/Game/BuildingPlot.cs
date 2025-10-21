using UnityEngine;

public class BuildingPlot : MonoBehaviour
{
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private UnitStats tower;


    private Color canBuild = new Color(0.298f, 0.686f, 0.314f, 1f); // green
    private Color cantBuild = new Color(0.957f, 0.263f, 0.212f, 1f); // red

    private void Update()
    {
        sr.color = (GameManager.main.currency[Team.South] >= tower.Cost) ? canBuild : cantBuild;
    }

    public void OnPlotClicked()
    {
        if (GameManager.main.currency[Team.South] >= tower.Cost)
        {
            BuildTower();
        }
        else
        {
            Debug.Log("Not enough currency to build!");
        }
    }

    private void BuildTower()
    {
        if (tower == null) return;

        UnitManager.main.SpawnSouthTower(this.transform);
    }
}
