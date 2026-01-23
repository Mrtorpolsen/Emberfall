using System;
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

    [Header("References Spawn Buttons Mid")]
    [SerializeField] public Button incomeBtn;
    [SerializeField] public Button cavalierBtn;
    [SerializeField] public Button rangerBtn;
    [SerializeField] public Button fighterBtn;

    [Header("References Spawn Buttons Bottom")]
    [SerializeField] public Button fighterBtnBottom;
    [SerializeField] public Button rangerBtnBottom;
    [SerializeField] public Button cavalierBtnBottom;
    [SerializeField] public Button incomeBtnBottom;


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

    public void ToggleSpawnButtonsActive(float playerCurrency)
    {
        //Figure out how to add dynamic values
        //Check if its top or bottom buttons that are active

        //SetButtonInteractable(fighterBtn, playerCurrency, 50);
        //SetButtonInteractable(rangerBtn, playerCurrency, 75);
        //SetButtonInteractable(cavalierBtn, playerCurrency, 100);
        //SetButtonInteractable(incomeBtn, playerCurrency, GameManager.Instance.incomeUpgradeCost);

        SetButtonInteractable(fighterBtnBottom, playerCurrency, 50);
        SetButtonInteractable(rangerBtnBottom, playerCurrency, 75);
        SetButtonInteractable(cavalierBtnBottom, playerCurrency, 150);
        SetButtonInteractable(incomeBtnBottom, playerCurrency, GameManager.Instance.incomeUpgradeCost);

    }

    private void SetButtonInteractable(Button btn, float playerCurrency, float cost)
    {
        btn.interactable = (!PauseManager.IsPaused && (playerCurrency >= cost));
    }

    public void UpdateIncomeCostText()
    {
        incomeCostText.text = GameManager.Instance.incomeUpgradeCost.ToString();
    }
}
