using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] public Canvas gameUI;
    [SerializeField] public Canvas pauseMenu;

    [Header("References Game Info")]
    [SerializeField] public TMP_Text currencyText;
    [SerializeField] public TMP_Text incomeMultiplierText;
    [SerializeField] private TMP_Text survivalText;
    [SerializeField] private Button topMenuButton;
    [SerializeField] private Button bottomMenuButton;
    [SerializeField] private TMP_Text incomeCostText;
    [SerializeField] private TMP_Text waveCountText;

    [Header("References Buttons")]
    [SerializeField] public Button incomeButton;
    [SerializeField] private Button unitsButton;
    [SerializeField] private Button abilitiesButton;
    [SerializeField] private Button orderButton;
    [SerializeField] private List<ActionButton> unitButtons;
    [SerializeField] private List<ActionButton> abilityButtons;
    [SerializeField] private GameObject unitButtonsPanel;
    [SerializeField] private GameObject abilityButtonsPanel;
    [SerializeField] private GameObject orderButtonsPanel;
    [SerializeField] private List<ActionButton> towerBuildMenuWestButtons;
    [SerializeField] private List<ActionButton> towerMenuWestButtons;
    [SerializeField] private List<ActionButton> towerBuildMenuEastButtons;
    [SerializeField] private List<ActionButton> towerMenuEastButtons;

    [Header("References Pause Menu")]
    [SerializeField] private Button pauseMainMenuButton;
    [SerializeField] private TMP_Text pauseTitleText;

    private const string TutorialPauseMessage = "Paused...\nThe main menu is locked until you defeat Sir Tutor the Third!";

    [Header("Tower Assets")]
    [SerializeField] private AssetReference sellTowerIcon;
    [SerializeField] private AssetReference upgradeTowerIcon;
    [SerializeField] private AssetReference towerIcon;
    [SerializeField] private AssetReference bombTowerIcon;
    [SerializeField] private AssetReference ballistaTowerIcon;

    private SpawnDefinition[] loadOutUnits;
    private SpawnDefinition[] loadOutTowers;
    private AbilityDefinition[] loadOutAbilities;

    private Sprite towerSprite;
    private Sprite bombSprite;
    private Sprite ballistaSprite;
    private Sprite sellSprite;
    private Sprite upgradeSprite;

    private List<ActionButton> boundButtons;

    private BuildingPlot activePlot;
    private GameObject activeMenuPanel;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        EnsureCorrectInitialPanelState();
    }

    private async void Start()
    {
        await InitializeAsync();

        SetupMenuButtons();
        SetupUnitButtons(loadOutUnits);
        SetupTowerBuildMenuButtons(loadOutTowers);
        SetupAbilityButtons(loadOutAbilities);
    }

    private async Task InitializeAsync()
    {
        boundButtons = new List<ActionButton>();

        // Loaded in parallel; released in OnDestroy
        var sellHandle = sellTowerIcon.LoadAssetAsync<Sprite>();
        var upgradeHandle = upgradeTowerIcon.LoadAssetAsync<Sprite>();
        var towerHandle = towerIcon.LoadAssetAsync<Sprite>();
        var bombHandle = bombTowerIcon.LoadAssetAsync<Sprite>();
        var ballistaHandle = ballistaTowerIcon.LoadAssetAsync<Sprite>();

        await Task.WhenAll(sellHandle.Task, upgradeHandle.Task, towerHandle.Task, bombHandle.Task, ballistaHandle.Task);

        sellSprite = sellHandle.Result;
        upgradeSprite = upgradeHandle.Result;
        towerSprite = towerHandle.Result;
        bombSprite = bombHandle.Result;
        ballistaSprite = ballistaHandle.Result;

        if (LoadoutService.Instance == null)
        {
            Debug.LogError("LoadoutService not found, returning to menu");
            //SceneManager.LoadScene("UI_Root");
            return;
        }

        if (GameSettingsService.Instance.Difficulty == DifficultyLevel.Tutorial)
        {
            var tutorialLoadout = LoadoutService.Instance.GetTutorialLoadout();

            loadOutUnits = tutorialLoadout.UnitLoadout;
            loadOutTowers = tutorialLoadout.TowerLoadout;
            loadOutAbilities = tutorialLoadout.AbilityLoadout;
        }
        else
        {
            loadOutUnits = LoadoutService.Instance.CurrentLoadout.UnitLoadout;
            loadOutTowers = LoadoutService.Instance.CurrentLoadout.TowerLoadout;
            loadOutAbilities = LoadoutService.Instance.CurrentLoadout.AbilityLoadout;
        }
    }

    private void OnEnable()
    {
        PauseManager.OnPauseChanged += TogglePauseMenu;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.Economy.CurrencyChanged += HandleCurrencyChanged;
            GameManager.Instance.Economy.IncomeChanged += HandleIncomeChanged;
        }
        BindWaveChangeUpdateEvent();
    }

    private void OnDisable()
    {
        PauseManager.OnPauseChanged -= TogglePauseMenu;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.Economy.CurrencyChanged -= HandleCurrencyChanged;
            GameManager.Instance.Economy.IncomeChanged -= HandleIncomeChanged;
        }
        if (WaveController.waveGenerator != null)
        {
            WaveController.waveGenerator.OnWaveNumberChanged -= UpdateWaveCountText;
        }
    }

    private void OnDestroy()
    {
        ReleaseIfLoaded(sellTowerIcon);
        ReleaseIfLoaded(upgradeTowerIcon);
        ReleaseIfLoaded(towerIcon);
        ReleaseIfLoaded(bombTowerIcon);
        ReleaseIfLoaded(ballistaTowerIcon);
    }

    private static void ReleaseIfLoaded(AssetReference reference)
    {
        if (reference != null && reference.OperationHandle.IsValid())
        {
            reference.ReleaseAsset();
        }
    }

    public void OnPauseBtnClick() { PauseManager.TogglePause(); }

    public void BindWaveChangeUpdateEvent()
    {
        if (WaveController.waveGenerator != null)
        {
            WaveController.waveGenerator.OnWaveNumberChanged += UpdateWaveCountText;
        }
    }

    private void TogglePauseMenu(bool paused)
    {
        pauseMenu.gameObject.SetActive(paused);
        RefreshAllButtons();
    }

    public void SetGameOver()
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

    // Leaving mid-tutorial isn't allowed; after Sir Tutor dies the tutorial-over panel offers the main menu instead
    public void LockPauseMenuForTutorial()
    {
        if (pauseMainMenuButton == null || pauseTitleText == null)
        {
            Debug.LogError("Pause menu references not assigned on UIManager");
            return;
        }

        pauseMainMenuButton.interactable = false;
        pauseTitleText.SetText(TutorialPauseMessage);
    }

    public void SetTutorialOver()
    {
        bottomMenuButton.gameObject.SetActive(false);
        gameUI.gameObject.SetActive(true);
        survivalText.SetText($"Congratulations! You beat Sir Tutor the Third and earned 2000 cinders <voffset=0.35em><sprite=0></voffset>");
    }

    public void SetTutorialGameOver()
    {
        topMenuButton.gameObject.SetActive(false);
        gameUI.gameObject.SetActive(true);
        survivalText.SetText($"Learning can be tough, and Sir Tutor the Third can be a hard teacher! Try again, but this time build more fighters!");
    }

    public void GoToMainMenu()
    {
        GameManager.Instance.AwardReward();
        //Use scenemanager to get root
        SceneManager.LoadScene("UI_Root");
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void SetupMenuButtons()
    {
        unitsButton.onClick.AddListener(() => ToggleButtonPanel(unitButtonsPanel));
        abilitiesButton.onClick.AddListener(() => ToggleButtonPanel(abilityButtonsPanel));
        orderButton.onClick.AddListener(() => ToggleButtonPanel(orderButtonsPanel));
        topMenuButton.onClick.AddListener(() => GoToMainMenu());
        bottomMenuButton.onClick.AddListener(() => RestartGame());
    }

    public void SetupUnitButtons(SpawnDefinition[] loadout)
    {
        for (int i = 0; i < unitButtons.Count; i++)
        {
            if (i >= loadout.Length)
            {
                unitButtons[i].gameObject.SetActive(false);
                continue;
            }

            var def = loadout[i];

            if (def == null)
            {
                Debug.Log("Definition at index " + i + " is null, skipping button setup.");
                unitButtons[i].gameObject.SetActive(false);
                continue;
            }

            unitButtons[i].Setup(def.DisplayName, def.Cost, def.Icon, def.Cooldown, (() => !PauseManager.IsPaused && GameManager.Instance.Economy.CanAfford(Team.South, def.Cost)));
            unitButtons[i].SetClickAction(() =>
            {
                SpawnManager.Instance.SpawnSouthUnit(def.UnitPrefab);
            });

            boundButtons.Add(unitButtons[i]);
        }
    }

    public void SetupAbilityButtons(AbilityDefinition[] loadout)
    {
        for (int i = 0; i < abilityButtons.Count; i++)
        {
            if (i >= loadout.Length)
            {
                abilityButtons[i].gameObject.SetActive(false);
                continue;
            }

            AbilityDefinition def = loadout[i];

            if (def == null)
            {
                Debug.Log("Definition at index " + i + " is null, skipping button setup.");
                abilityButtons[i].gameObject.SetActive(false);
                continue;
            }

            abilityButtons[i].Setup(def, (() => !PauseManager.IsPaused && GameManager.Instance.Economy.CanAfford(Team.South, def.Cost) && AbilityCooldownManager.Instance.CanUse(def)));
            abilityButtons[i].SetClickAction(() =>
            {
                if (!AbilityCooldownManager.Instance.CanUse(def) || !GameManager.Instance.Economy.TrySpend(Team.South, def.Cost))
                    return;

                foreach (var action in def.actions)
                {
                    action.Execute(new AbilityContext(TargetRegistry.Instance));
                }

                AbilityCooldownManager.Instance.TriggerCooldown(def);
            });

            boundButtons.Add(abilityButtons[i]);
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

            if (def == null)
            {
                Debug.Log("Definition at index " + i + " is null, skipping button setup.");
                towerBuildMenuWestButtons[i].gameObject.SetActive(false);
                continue;
            }

            towerBuildMenuWestButtons[i].Setup(def.DisplayName, def.Cost, GetTowerSprite(def.DisplayName.ToLowerInvariant()), def.Cooldown, (() => !PauseManager.IsPaused && GameManager.Instance.Economy.CanAfford(Team.South, def.Cost)));
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

            if (def == null)
            {
                Debug.Log("Definition at index " + i + " is null, skipping button setup.");
                towerBuildMenuEastButtons[i].gameObject.SetActive(false);
                continue;
            }

            towerBuildMenuEastButtons[i].Setup(def.DisplayName, def.Cost, GetTowerSprite(def.DisplayName.ToLowerInvariant()), def.Cooldown, (() => !PauseManager.IsPaused && GameManager.Instance.Economy.CanAfford(Team.South, def.Cost)));
            towerBuildMenuEastButtons[i].SetClickAction(() =>
            {
                SpawnSouthTowerClickAction(def.UnitPrefab, SpawnSide.East);
            });

            boundButtons.Add(towerBuildMenuEastButtons[i]);
        }
    }

    public void SetupTowerMenuButtons(BuildingPlot plot, SpawnSide spawnSide)
    {
        var buttonsToSetup = spawnSide == SpawnSide.West ? towerMenuWestButtons : towerMenuEastButtons;

        var sellButton = buttonsToSetup[0];
        var upgradeButton = buttonsToSetup[1];

        sellButton.Setup("Sell", plot.sellValue, sellSprite, 0,
            () => !PauseManager.IsPaused
        );

        sellButton.SetClickAction(() =>
        {
            plot.SellTower();
            boundButtons.Remove(upgradeButton);
        });

        upgradeButton.Setup("Upgrade", plot.upgradeCost, upgradeSprite, 0,
            () => !PauseManager.IsPaused
                && GameManager.Instance.Economy.CanAfford(Team.South, plot.upgradeCost)
                && plot.canUpgrade
        );

        upgradeButton.SetClickAction(() =>
        {
            plot.UpgradeTower(spawnSide);
            RefreshTowerMenu(sellButton, upgradeButton, plot);
        });

        if (!boundButtons.Contains(upgradeButton))
        {
            boundButtons.Add(upgradeButton);
        }
    }

    public void RefreshTowerMenu(ActionButton sellButton, ActionButton upgradeButton, BuildingPlot plot)
    {
        sellButton.UpdateText("Sell", plot.sellValue);

        if (!plot.canUpgrade)
        {
            upgradeButton.UpdateText("Upgrade", "Max");
            return;
        }

        upgradeButton.UpdateText("Upgrade", plot.upgradeCost);
    }

    public void RefreshAllButtons()
    {
        if (boundButtons == null)
            return;

        foreach (var button in boundButtons)
        {
            button.Refresh();
        }
        incomeButton.interactable = (!PauseManager.IsPaused && GameManager.Instance.Economy.CanAfford(Team.South, GameManager.Instance.Economy.IncomeUpgradeCost));
        orderButton.interactable = (!PauseManager.IsPaused);
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

    private void HandleCurrencyChanged(Team team, float amount)
    {
        // Only the player's currency is shown
        if (team != Team.South)
            return;

        currencyText.text = ((int)amount).ToString();
        RefreshAllButtons();
    }

    private void HandleIncomeChanged()
    {
        UpdateIncomeText(GameManager.Instance.Economy.IncomePerTick);
        incomeCostText.text = GameManager.Instance.Economy.IncomeUpgradeCost.ToString();
    }
    private void UpdateIncomeText(float incomePerTick)
    {
        incomeMultiplierText.text = incomePerTick.ToString();
    }

    private void UpdateWaveCountText(int waveNumber)
    {
        waveCountText.text = waveNumber.ToString();
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

    public void ToggleButtonPanel(GameObject panel)
    {
        // Clicking the active panel closes everything
        if (activeMenuPanel == panel)
        {
            panel.transform.localScale = Vector3.zero;
            activeMenuPanel = null;
            return;
        }

        // Hide current panel if one is open
        if (activeMenuPanel != null)
        {
            activeMenuPanel.transform.localScale = Vector3.zero;
        }

        // Show the requested panel
        panel.transform.localScale = Vector3.one;
        activeMenuPanel = panel;
    }

    private Sprite GetTowerSprite(string tower)
    {
        switch (tower)
        {
            case "ballistatower":
                return ballistaSprite;

            case "bombtower":
                return bombSprite;

            case "tower":
                return towerSprite;

            default:
                return towerSprite;
        }
    }

    private void EnsureCorrectInitialPanelState()
    {
        gameUI.gameObject.SetActive(false);
        pauseMenu.gameObject.SetActive(false);
    }
}
