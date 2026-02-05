using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("References")]
    [SerializeField] private TMP_Text survivalText;
    [SerializeField] private TMP_Text incomeCostText;
    [SerializeField] public Canvas gameUI;
    [SerializeField] public Canvas pauseMenu;
    [SerializeField] private SpawnDefinition[] loadOutUnits;
    [SerializeField] private SpawnDefinition[] loadOutTowers;

    [Header("References Spawn Buttons")]
    [SerializeField] private List<SpawnButton> spawnUnitButtons;
    [SerializeField] private List<SpawnButton> spawnTowersEastButtons;
    [SerializeField] private List<SpawnButton> spawnTowersWestButtons;
    [SerializeField] public Button incomeBtn;

    private BuildingPlot activePlot;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(Instance);
        }
        else
        {
            Instance = this;
        }

        SetupUnitButtons(loadOutUnits);
        SetupTowerButtons(loadOutTowers);

        ToggleSpawnButtonsActive(GameManager.Instance.currency[Team.South]);
    }
    private void OnEnable()
    {
        PauseManager.OnPauseChanged += TogglePauseMenu;
    }

    private void OnDisable()
    {
        PauseManager.OnPauseChanged -= TogglePauseMenu;
    }

    public void OnPauseBtnClick() { PauseManager.TogglePause(); }

    private void TogglePauseMenu(bool paused)
    {
        pauseMenu.gameObject.SetActive(paused);
        ToggleSpawnButtonsActive(GameManager.Instance.currency[Team.South]);
    }

    public void Initialize()
    {
        gameUI.gameObject.SetActive(true);
        SetGameOverMessage();
    }

    public void SetGameOverMessage()
    {
        int cinders = CinderRewardCalculator.GetCinders(TimerManager.Instance.GetElapsedTimeInMinutes());
        if (cinders > 0)
        {
            survivalText.SetText($"Survival: {TimerManager.Instance.GetFormattedTime()} and earned {cinders} cinders <voffset=0.35em><sprite=0></voffset>");
        }
        else
        {
            survivalText.SetText($"You didnt survive for long...");
        }
    }

    public void GoToMainMenu()
    {
        GameManager.Instance.EndOfGame();
        //Use scenemanager to get root
        SceneManager.LoadScene("UI_Root");
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void SetupUnitButtons(SpawnDefinition[] loadout)
    {
        for (int i = 0; i < spawnUnitButtons.Count; i++)
        {
            if (i < loadout.Length)
            {
                spawnUnitButtons[i].Setup(loadout[i]);
            }
            else
            {
                spawnUnitButtons[i].gameObject.SetActive(false);
            }
        }
    }

    public void SetupTowerButtons(SpawnDefinition[] loadout)
    {
        for (int i = 0; i < spawnTowersEastButtons.Count; i++)
        {
            if (i < loadout.Length)
            {
                spawnTowersEastButtons[i].Setup(loadout[i]);
            }
            else
            {
                spawnTowersEastButtons[i].gameObject.SetActive(false);
            }
        }
        for (int i = 0; i < spawnTowersWestButtons.Count; i++)
        {
            if (i < loadout.Length)
            {
                spawnTowersWestButtons[i].Setup(loadout[i]);
            }
            else
            {
                spawnTowersWestButtons[i].gameObject.SetActive(false);
            }
        }
    }

    public void ToggleSpawnButtonsActive(float playerCurrency)
    {
        foreach (var btn in spawnUnitButtons)
        {
            btn.SetInteractable(playerCurrency, PauseManager.IsPaused);
        }
        foreach (var btn in spawnTowersEastButtons)
        {
            btn.SetInteractable(playerCurrency, PauseManager.IsPaused);
        }
        foreach (var btn in spawnTowersWestButtons)
        {
            btn.SetInteractable(playerCurrency, PauseManager.IsPaused);
        }

        incomeBtn.interactable = (!PauseManager.IsPaused && playerCurrency >= GameManager.Instance.incomeUpgradeCost);
    }

    public void UpdateIncomeCostText()
    {
        incomeCostText.text = GameManager.Instance.incomeUpgradeCost.ToString();
    }

    public void ToggleSpawnMenu(BuildingPlot plot)
    {
        if (activePlot == plot)
        {
            plot.HideMenu();
            activePlot = null;
            return;
        }

        if (activePlot != null)
        {
            activePlot.HideMenu();
        }

        activePlot = plot;
        plot.ShowMenu();
    }

    public void ClearActivePlot()
    {
        activePlot = null;
    }

    public BuildingPlot GetActivePlot()
    {
        return activePlot;
    }
}
