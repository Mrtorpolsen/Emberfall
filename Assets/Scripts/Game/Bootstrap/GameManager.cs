using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    //Consider seperating this into smaller bits, and have this one start/stop them all
    public static GameManager Instance;

    public TMP_Text southCurrencyText;
    public TMP_Text southIncomeModifierText;

    public Transform north;
    public Transform south;

    public GameObject playerUnitBoundary;

    public Dictionary<Team, float> currency;

    [Header("References")]
    [SerializeField] GameObject gameUICanvas;

    [Header("Attributes")]
    [SerializeField] float currencyTimer = 0f;
    [SerializeField] float currencyInterval = 1f;
    [SerializeField] float incomePerTick = 20;
    [SerializeField] float baseIncomePerTick = 20;
    [SerializeField] float incomeModifier = 1;
    [SerializeField] public float incomeUpgradeCost = 200;
    [SerializeField] public bool isGameOver = false;
    [SerializeField] public bool isGameRunning = false;
    [SerializeField] public Team winningTeam;

    private void Awake()
    {
        Instance = this;

        currency = new Dictionary<Team, float>()
        {
            { Team.North, 1000 },
            { Team.South, 300 },
        };

        PauseManager.SetPaused(false);

        UpdateCurrencyText();
        playerUnitBoundary = GameObject.FindGameObjectWithTag("PlayerUnitBarrier");
    }

    public void Start()
    {
        StartGame();
    }

    void Update()
    {
        if (isGameRunning)
        { 
            currencyTimer += Time.deltaTime;

            if(currencyTimer >= currencyInterval)
            {
                currencyTimer = 0f;

                AddCurrency(Team.South, incomePerTick);
            }
        }
    }

    public void AddCurrency(Team team, float amount)
    {
        currency[team] += amount;
        UpdateCurrencyText();
        UIManager.Instance.ToggleSpawnButtonsActive(currency[team]);
    }

    public void SubtractCurrency(Team team, float amount)
    {
        currency[team] -= amount;
        UpdateCurrencyText();
        UIManager.Instance.ToggleSpawnButtonsActive(currency[team]);
    }

    private void UpdateCurrencyText()
    {
        //northCurrencyText.text = currency[Team.North].ToString();
        southCurrencyText.text = ((int)currency[Team.South]).ToString();
    }
    public void UpgradeIncomeModifier()
    {
        incomeModifier = (float)(incomeModifier + 0.2);
        incomePerTick = baseIncomePerTick * incomeModifier;
        southIncomeModifierText.text = "x" + incomeModifier.ToString("F1");
    }
    public void SetGameOver(bool gameOver, Team team)
    {
        TimerManager.Instance.StopTimer();
        isGameOver = gameOver;
        winningTeam = team == Team.North ? Team.South : Team.North;
        UIManager.Instance.Initialize();
        isGameRunning = false;
        gameUICanvas.SetActive(false);
        EndOfGame();
    }

    public void StartGame()
    {
        Instance.isGameRunning = true;
        TimerManager.Instance.StartTimer();

        StartCoroutine(CoroutineHelpers.DoAfterDelay(3f, () =>
        {
            WaveController.Instance.StartWaves();
        }));
    }

    public void EndOfGame()
    {
        //save score, throws error if not logged in
        LeaderboardManager.Instance.AddScore(TimerManager.Instance.GetElapsedTime());
        //add cinders
        CurrencyManager.Instance.Add(CurrencyTypes.Cinders,
            CinderRewardCalculator.GetCinders(TimerManager.Instance.GetElapsedTimeInMinutes()));
    }
}
