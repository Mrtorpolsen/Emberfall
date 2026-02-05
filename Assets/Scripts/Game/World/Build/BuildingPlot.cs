using UnityEngine;

public class BuildingPlot : MonoBehaviour
{
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private GameObject spawnMenu;
    private bool isOccupied;

    public bool IsOccupied => isOccupied;

    public void OnPlotClicked()
    {
        UIManager.Instance.ToggleSpawnMenu(this);
    }

    public void ShowMenu()
    {
        spawnMenu.transform.localScale = Vector3.one;
    }

    public void HideMenu()
    {
        spawnMenu.transform.localScale = Vector3.zero;
    }

    public void MarkOccupied()
    {
        isOccupied = true;

        GetComponent<BoxCollider2D>().enabled = false;
        sr.enabled = false;
        HideMenu();
    }

}
