using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class GamePlayTutorialPresenter : MonoBehaviour
{
    public static GamePlayTutorialPresenter Instance { get; private set; }

    [Header("Canvas")]
    [SerializeField] private Canvas tutorialCanvas;

    [Header("Panels")]
    [SerializeField] private GameObject gameInfoPanel;
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject rallyPanel;
    [SerializeField] private GameObject towerPanel;
    [SerializeField] private GameObject unitsPanel;

    [Header("Units sub panels")]
    [SerializeField] private GameObject unitsExplanation;
    [SerializeField] private GameObject sendFighter;

    [Header("Popup Buttons")]
    [SerializeField] private Button gameInfoButton;
    [SerializeField] private Button menuButton;
    [SerializeField] private Button rallyButton;
    [SerializeField] private Button towerButton;
    [SerializeField] private Button unitsButton;

    [Header("Spawn Fighter")]
    [SerializeField] private Button spawnFighterButton;
    [SerializeField] private GameObject fighterPrefab;

    private TaskCompletionSource<bool> completionSource;

    public int fighterSpawnCount = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        tutorialCanvas.gameObject.SetActive(false);
    }

    public Task RunTutorial()
    {
        completionSource = new TaskCompletionSource<bool>();
        ShowOverlayTutorial();
        return completionSource.Task;
    }

    public void ShowOverlayTutorial()
    {
        tutorialCanvas.gameObject.SetActive(true);
        ShowGameInfoPanel();
    }

    public void ShowGameInfoPanel()
    {
        gameInfoPanel.gameObject.SetActive(true);
    }

    public void ShowMenuPanel()
    {
        gameInfoPanel.gameObject.SetActive(false);
        menuPanel.gameObject.SetActive(true);
    }

    public void ShowRallyPanel()
    {
        menuPanel.gameObject.SetActive(false);
        rallyPanel.gameObject.SetActive(true);
    }

    public void ShowTowerPanel()
    {
        rallyPanel.gameObject.SetActive(false);
        towerPanel.gameObject.SetActive(true);
    }

    public void ShowUnitsExplanationPanel()
    {
        towerPanel.gameObject.SetActive(false);
        unitsPanel.gameObject.SetActive(true);
        unitsExplanation.gameObject.SetActive(true);
    }

    public void ShowSendFighterPanel()
    {
        unitsExplanation.gameObject.SetActive(false);
        sendFighter.gameObject.SetActive(true);
    }

    public void SpawnFighter()
    {
        fighterSpawnCount++;

        SpawnManager.Instance.SpawnSouthUnit(fighterPrefab, "fighter");

        if (fighterSpawnCount >= 5)
        {
            TutorialComplete();
        }
    }

    private void TutorialComplete()
    {
        tutorialCanvas.gameObject.SetActive(false);
        completionSource?.TrySetResult(true);
    }
}
