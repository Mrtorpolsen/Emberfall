using System.Collections.Generic;
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
    [SerializeField] private List<ActionButton> spawnUnitButtons;
    [SerializeField] private List<ActionButton> towerBuildMenuWestButtons;
    [SerializeField] private List<ActionButton> towerMenuWestButtons;
    [SerializeField] private List<ActionButton> towerBuildMenuEastButtons;
    [SerializeField] private List<ActionButton> towerMenuEastButtons;
    [SerializeField] public Button incomeBtn;

    private List<ActionButton> boundButtons;

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

        boundButtons = new List<ActionButton>();

        SetupUnitButtons(loadOutUnits);
        SetupTowerBuildMenuButtons(loadOutTowers);
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
        RefreshAllButtons();
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

            spawnUnitButtons[i].Setup(def.DisplayName, def.Cost, def.Icon, (() => !PauseManager.IsPaused && GameManager.Instance.currency[Team.South] >= def.Cost));
            spawnUnitButtons[i].SetClickAction(() =>
            {
                SpawnManager.Instance.SpawnSouthUnit(
                    def.UnitPrefab,
                    def.UnitPrefab.name.ToLowerInvariant()
                );
            });

            boundButtons.Add(spawnUnitButtons[i]);
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

            towerBuildMenuWestButtons[i].Setup(def.DisplayName, def.Cost, def.Icon, (() => !PauseManager.IsPaused && GameManager.Instance.currency[Team.South] >= def.Cost));
            towerBuildMenuWestButtons[i].SetClickAction(() =>
            {
                SpawnSouthTowerClickAction(def.UnitPrefab, SpawnSide.West);
            });

            boundButtons.Add(towerBuildMenuWestButtons[i]);
        }

        for (int i = 0; i < towerBuildMenuEastButtons.Count; i++)
        {
            if (i >= loadout.Length)
            {
                towerBuildMenuEastButtons[i].gameObject.SetActive(false);
                    continue;
            }

            var def = loadout[i];

            towerBuildMenuEastButtons[i].Setup(def.DisplayName, def.Cost, def.Icon, (() => !PauseManager.IsPaused && GameManager.Instance.currency[Team.South] >= def.Cost));
            towerBuildMenuEastButtons[i].SetClickAction(() =>
            {
                SpawnSouthTowerClickAction(def.UnitPrefab, SpawnSide.East);
            });

            boundButtons.Add(towerBuildMenuEastButtons[i]);
        }
    }

    public void SetupTowerMenuButtons(BuildingPlot plot, SpawnSide spawnSide)
    {
        if (spawnSide == SpawnSide.West)
        {
            for (int i = 0; i < towerMenuWestButtons.Count; i++)
            {
                if (i == 0)
                {
                    towerMenuWestButtons[i].Setup("Sell", plot.sellValue, null, 
                        () => !PauseManager.IsPaused
                    );
                    towerMenuWestButtons[i].SetClickAction(() =>
                    {
                        plot.SellTower();
                        boundButtons.Remove(towerMenuWestButtons[1]);
                    });
                }
                if (i == 1)
                {
                    towerMenuWestButtons[i].Setup("Upgrade", plot.sellValue, null,
                        () => !PauseManager.IsPaused
                    );
                    towerMenuWestButtons[i].SetClickAction(() =>
                    {
                        plot.UpgradeTower();
                    });

                    boundButtons.Add(towerMenuWestButtons[i]);
                }
            }
        }

        if (spawnSide == SpawnSide.East)
        {
            for (int i = 0; i < towerMenuEastButtons.Count; i++)
            {
                if (i == 0)
                {
                    towerMenuEastButtons[i].Setup("Sell", plot.sellValue, null,
                        () => !PauseManager.IsPaused
                    );
                    towerMenuEastButtons[i].SetClickAction(() =>
                    {
                        plot.SellTower();
                        boundButtons.Remove(towerMenuEastButtons[1]);
                    });
                }
                if (i == 1)
                {
                    towerMenuEastButtons[i].Setup("Upgrade", plot.sellValue, null,
                        () => !PauseManager.IsPaused
                    );
                    towerMenuEastButtons[i].SetClickAction(() =>
                    {
                        plot.UpgradeTower();
                    });

                    boundButtons.Add(towerMenuEastButtons[i]);
                }
            }
        }

    }

    public void RefreshAllButtons()
    {
        //foreach (var button in spawnUnitButtons)
        //{
        //    button.Refresh();
        //}
        //foreach (var button in towerBuildMenuWestButtons)
        //{
        //    button.Refresh();
        //}
        //foreach (var button in towerMenuWestButtons)
        //{
        //    button.Refresh();
        //}
        //foreach (var button in towerBuildMenuEastButtons)
        //{
        //    button.Refresh();
        //}
        //foreach (var button in towerMenuEastButtons)
        //{
        //    button.Refresh();
        //}
        foreach (var button in boundButtons)
        {
            button.Refresh();
        }
        incomeBtn.interactable = (!PauseManager.IsPaused && GameManager.Instance.currency[Team.South] >= GameManager.Instance.incomeUpgradeCost);
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
            SetupTowerMenuButtons(plot, spawnSide);
        }
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
