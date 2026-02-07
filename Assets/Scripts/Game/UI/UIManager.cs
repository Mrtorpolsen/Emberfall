using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEditor.Timeline.Actions;
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
    [SerializeField] private List<SpawnButton> towerBuildMenuWestButtons;
    [SerializeField] private List<SpawnButton> towerMenuWestButtons;
    [SerializeField] private List<SpawnButton> towerBuildMenuEastButtons;
    [SerializeField] private List<SpawnButton> towerMenuEastButtons;
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
        SetupTowerBuildMenuButtons(loadOutTowers);

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
            if (i >= loadout.Length)
            {
                spawnUnitButtons[i].gameObject.SetActive(false);
                    continue;
            }

            var def = loadout[i];

            spawnUnitButtons[i].Setup(def);
            spawnUnitButtons[i].SetClickAction(() =>
            {
                SpawnManager.Instance.SpawnSouthUnit(
                    def.UnitPrefab,
                    def.UnitPrefab.name.ToLowerInvariant()
                );
            });
        }
    }

    public void SetupTowerBuildMenuButtons(SpawnDefinition[] loadout)
    {
        for (int i = 0; i < towerBuildMenuWestButtons.Count; i++)
        {
            if (i >= loadout.Length)
            {
                towerBuildMenuWestButtons[i].gameObject.SetActive(false);
                    continue;
            }

            var def = loadout[i];

            towerBuildMenuWestButtons[i].Setup(def);
            towerBuildMenuWestButtons[i].SetClickAction(() =>
            {
                SpawnSouthTowerClickAction(def.UnitPrefab, SpawnSide.West);
            });
        }

        for (int i = 0; i < towerBuildMenuEastButtons.Count; i++)
        {
            if (i >= loadout.Length)
            {
                towerBuildMenuEastButtons[i].gameObject.SetActive(false);
                    continue;
            }

            var def = loadout[i];

            towerBuildMenuEastButtons[i].Setup(def);
            towerBuildMenuEastButtons[i].SetClickAction(() =>
            {
                SpawnSouthTowerClickAction(def.UnitPrefab, SpawnSide.East);
            });
        }
    }

    public void SpawnSouthTowerClickAction(GameObject prefab, SpawnSide spawnSide)
    {
        BuildingPlot plot = UIManager.Instance.GetActivePlot();

        bool success = SpawnManager.Instance.SpawnSouthTower(
            prefab,
            spawnSide,
            out GameObject spawned
        );

        if (!success)
            return;

        if (plot != null && spawned != null)
        {
            plot.AssignTower(spawned);
            UIManager.Instance.CloseAllMenus();
        }
    }

    public void ToggleSpawnButtonsActive(float playerCurrency)
    {
        foreach (var btn in spawnUnitButtons)
        {
            btn.SetInteractable(playerCurrency, PauseManager.IsPaused);
        }
        foreach (var btn in towerBuildMenuEastButtons)
        {
            btn.SetInteractable(playerCurrency, PauseManager.IsPaused);
        }
        foreach (var btn in towerBuildMenuWestButtons)
        {
            btn.SetInteractable(playerCurrency, PauseManager.IsPaused);
        }

        incomeBtn.interactable = (!PauseManager.IsPaused && playerCurrency >= GameManager.Instance.incomeUpgradeCost);
    }

    public void UpdateIncomeCostText()
    {
        incomeCostText.text = GameManager.Instance.incomeUpgradeCost.ToString();
    }

    public void ToggleBuildMenu(BuildingPlot plot)
    {
        if (activePlot == plot)
        {
            plot.HideMenus();
            activePlot = null;
            return;
        }

        CloseAllMenus();

        activePlot = plot;
        plot.ShowBuildMenu();
    }

    public void ToggleTowerMenu(BuildingPlot plot)
    {
        if (activePlot == plot)
        {
            plot.HideMenus();
            activePlot = null;
            return;
        }

        CloseAllMenus();

        activePlot = plot;
        plot.ShowTowerMenu();
    }

    public void CloseAllMenus()
    {
        if (activePlot != null)
        {
            activePlot.HideMenus();
            activePlot = null;
        }
    }

    public BuildingPlot GetActivePlot()
    {
        return activePlot;
    }
}
