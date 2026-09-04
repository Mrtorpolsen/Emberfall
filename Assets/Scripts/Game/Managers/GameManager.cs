using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    //Consider seperating this into smaller bits, and have this one start/stop them all
    public static GameManager Instance { get; private set; }

    public Transform south;

    private int nextRangedSpawn = 0;
    public Transform[] playerRangedRallies;

    public Transform meleeRally;

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

    [Header("Rally Settings")]
    [SerializeField] private float maxYMeleeRally = 0.1f;
    [SerializeField] private float minYMeleeRally = -4.15f;
    [SerializeField] private float maxYRangedRally = -0.25f;
    [SerializeField] private float minYRangedRally = -4.5f;
    //[SerializeField] private float startYMeleeRally = 0.1f;
    //[SerializeField] private float startYRangedRally = -1.25f;

    private float rallyJump = 1f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

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
        //Needs to be before gameOver, otherwise it will lock the player out of getting rewards
        EndOfGame();
        UpdateGameState(gameOver, losingTeam);
        HandleUITransition();
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
        if(isGameOver)
            return;
        //save score, throws error if not logged in
        LeaderboardService.Instance.AddScore(TimerManager.Instance.GetElapsedTime(), GameSettingsService.Instance.Difficulty);
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

    public void AdvanceMeleeRally()
    {
        MoveRally(meleeRally, minYMeleeRally, maxYMeleeRally, rallyJump);
    }

    public void FallbackMeleeRally()
    {
        MoveRally(meleeRally, minYMeleeRally, maxYMeleeRally, -rallyJump);
    }

    public void AdvanceRangedRally()
    {
        foreach(Transform rally in playerRangedRallies)
        {
            MoveRally(rally, minYRangedRally, maxYRangedRally, rallyJump);
        }
    }

    public void FallbackRangedRally()
    {
        foreach(Transform rally in playerRangedRallies)
        {
            MoveRally(rally, minYRangedRally, maxYRangedRally, -rallyJump);
        }
    }

    public void MoveRally(Transform rallyPoint, float minY, float maxY, float amount)
    {
        Vector2 newPosition = rallyPoint.position;

        newPosition.y = Mathf.Clamp(rallyPoint.position.y + amount, minY, maxY);

        rallyPoint.position = newPosition;
    }
}
