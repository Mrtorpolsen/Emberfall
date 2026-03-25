using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    //Consider seperating this into smaller bits, and have this one start/stop them all
    public static GameManager Instance;

    public Transform south;

    private int nextRangedSpawn = 0;
    public Transform[] playerRangedRallies;

    public Transform playerUnitBoundary;

    public Dictionary<Team, float> currency;

    public event Action<int> OnCurrencyChanged;
    public event Action<float> OnIncomeMultiplierChanged;

    [Header("References")]
    [SerializeField] GameObject gameUICanvas;

    [Header("Attributes")]
    [SerializeField] float currencyTimer = 0f;
    [SerializeField] float currencyInterval = 1f;
    [SerializeField] float incomePerTick = 20;
    [SerializeField] float baseIncomePerTick = 20;
    [SerializeField] float incomeMultiplier = 1;
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

        OnCurrencyChanged?.Invoke((int)currency[Team.South]);
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
        OnCurrencyChanged?.Invoke((int)currency[team]);
        UIManager.Instance.RefreshAllButtons();
    }

    public void SubtractCurrency(Team team, float amount)
    {
        currency[team] -= amount;
        OnCurrencyChanged?.Invoke((int)currency[team]);
        UIManager.Instance.RefreshAllButtons();
    }

    public void UpgradeIncomeModifier()
    {
        incomeMultiplier += (float)0.2;
        incomePerTick = baseIncomePerTick * incomeMultiplier;
        OnIncomeMultiplierChanged?.Invoke(incomeMultiplier);
    }

    private void UpdateGameState(bool gameOver, Team losingTeam)
    {
        isGameOver = gameOver;
        winningTeam = GetWinningTeam(losingTeam);
    }

    private Team GetWinningTeam(Team losingTeam)
    {
        return losingTeam == Team.North ? Team.South : Team.North;
    }

    private void HandleUITransition()
    {
        gameUICanvas.SetActive(false);
        UIManager.Instance.Initialize();
    }

    private void StopGameplaySystems()
    {
        TimerManager.Instance.StopTimer();
        isGameRunning = false;
    }

    public void SetGameOver(bool gameOver, Team losingTeam)
    {
        StopGameplaySystems();
        UpdateGameState(gameOver, losingTeam);
        HandleUITransition();
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
        LeaderboardService.Instance.AddScore(TimerManager.Instance.GetElapsedTime());
        //add cinders
        CurrencyManager.Instance.Add(CurrencyTypes.Cinders,
            CinderRewardCalculator.GetCinders(TimerManager.Instance.GetElapsedTimeInMinutes()));
    }

    public Transform GetNextRangedRally()
    {
        Transform selected = playerRangedRallies[nextRangedSpawn];
        nextRangedSpawn = (nextRangedSpawn + 1) % playerRangedRallies.Length;
        return selected;
    }
}
