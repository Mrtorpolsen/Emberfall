using UnityEngine;

public enum PlotState
{
    Empty,
    Occupied
}

public class BuildingPlot : MonoBehaviour
{
    [SerializeField] private GameObject buildMenu;
    [SerializeField] private GameObject towerMenu;
    [SerializeField] private SpriteRenderer sr;

    private PlotState state = PlotState.Empty;
    private GameObject currentTower;

    public bool HasTower => state == PlotState.Occupied;

    public void OnPlotClicked()
    {
        if (state == PlotState.Empty)
        {
            UIManager.Instance.ToggleBuildMenu(this);
        }
        else
        {
            UIManager.Instance.ToggleTowerMenu(this);
        }
    }

    public void AssignTower(GameObject tower)
    {
        currentTower = tower;
        state = PlotState.Occupied;
        sr.enabled = false;
        UIManager.Instance.CloseAllMenus();
    }

    public GameObject GetTower()
    {
        return currentTower;
    }

    public void SellTower()
    {
        if (state != PlotState.Occupied || currentTower == null)
            return;

        if (!currentTower.TryGetComponent<BaseUnitStats>(out var towerStats))
        {
            Debug.LogError("Attempted to sell tower without BaseUnitStats.");
            return;
        }

        GameManager.Instance.AddCurrency(Team.South, (towerStats.Cost / 2));
        Destroy(currentTower);

        currentTower = null;
        state = PlotState.Empty;
    }

    public void UpgradeTower()
    {
        Debug.Log("Wuhu im upgrading yaaaaa");
    }

    public void ShowBuildMenu()
    {
        buildMenu.transform.localScale = Vector3.one;
        towerMenu.transform.localScale = Vector3.zero;
    }

    public void ShowTowerMenu()
    {
        buildMenu.transform.localScale = Vector3.zero;
        towerMenu.transform.localScale = Vector3.one;
    }

    public void HideMenus()
    {
        buildMenu.transform.localScale = Vector3.zero;
        towerMenu.transform.localScale = Vector3.zero;
    }
}
