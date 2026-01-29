using UnityEngine;

public class BuildingPlot : MonoBehaviour
{
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private BaseUnitStats tower;

    private Color canBuild = new Color(0.298f, 0.686f, 0.314f, 1f); // green
    private Color cantBuild = new Color(0.957f, 0.263f, 0.212f, 1f); // red

    private void Update()
    {
        sr.color = (GameManager.Instance.currency[Team.South] >= tower.Cost) ? canBuild : cantBuild;
    }

    public void OnPlotClicked()
    {
        Debug.Log("Plot clicked!!!");
        if (GameManager.Instance.currency[Team.South] >= tower.Cost)
        {
            BuildTower();
            gameObject.SetActive(false);
        }
        else
        {
            Debug.Log("Not enough currency to build!");
        }
    }

    private void BuildTower()
    {
        if (tower == null) return;

        SpawnManager.Instance.SpawnSouthTower(this.transform, tower.GameObject);
    }
}
