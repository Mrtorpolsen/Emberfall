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

    public float upgradeCost;
    public float sellValue;
    public bool canUpgrade;
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
        if (!currentTower.TryGetComponent<TowerUnitStats>(out var towerStats))
        {
            Debug.LogError("Can't get tower BaseUnitStats.");
            return;
        }
        sellValue = towerStats.GetSellValue();
        upgradeCost = towerStats.GetUpgradeCost();
        canUpgrade = towerStats.CanUpgrade();
    }

    public void SellTower()
    {
        if (state != PlotState.Occupied || currentTower == null)
            return;

        if (sellValue <= 0)
        {
            Debug.LogError("Cant sell tower, value is 0 or less");
            return;
        }

        GameManager.Instance.AddCurrency(Team.South, sellValue);
        Destroy(currentTower);

        currentTower = null;
        sr.enabled = true;
        state = PlotState.Empty;
        UIManager.Instance.CloseAllMenus();
    }

    public void UpgradeTower(SpawnSide spawnSide)
    {
        if (state != PlotState.Occupied || currentTower == null || GameManager.Instance.currency[Team.South] < upgradeCost)
            return;

        if (!currentTower.TryGetComponent<TowerUnitStats>(out var towerStats))
        {
            Debug.LogError("Tower missing TowerUnitStats.");
            return;
        }

        if (!towerStats.CanUpgrade())
            return;

        GameManager.Instance.SubtractCurrency(Team.South, upgradeCost);

        towerStats.UpgradeTier();

        sellValue = towerStats.GetSellValue();
        upgradeCost = towerStats.GetUpgradeCost();
        canUpgrade = towerStats.CanUpgrade();
    }

    public GameObject GetTower()
    {
        return currentTower;
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
