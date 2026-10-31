using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    //Consider separating this into smaller bits, and have this one start/stop them all
    public static GameManager Instance { get; private set; }

    [SerializeField] private Transform south;
    public Transform South => south;

    private int nextRangedSpawn = 0;
    [SerializeField] private Transform[] playerRangedRallies;

    [SerializeField] private Transform meleeRally;
    public Transform MeleeRally => meleeRally;

    public MatchEconomy Economy { get; private set; }

    [Header("References")]
    [SerializeField] GameObject gameUICanvas;

    [Header("Attributes")]
    [SerializeField] float currencyInterval = 1f;
    [SerializeField] float baseIncomePerTick = 20;
    [SerializeField] float incomeMultiplier = 1;
    [SerializeField] float incomeUpgradeCost = 200;
    [SerializeField] private bool isGameOver = false;
    [SerializeField] private bool isGameRunning = false;
    [SerializeField] private Team winningTeam;

    public bool IsGameOver => isGameOver;

#if UNITY_EDITOR
    [Header("Debug Edit (play mode)")]
    [Tooltip("Player (South) currency to set when Apply Currency is ticked")]
    [SerializeField] private int debugSetSouthCurrency;
    [Tooltip("Tick to apply Set South Currency; unticks itself")]
    [SerializeField] private bool debugApplyCurrency;
#endif

    [Header("Rally Settings")]
    [SerializeField] private float maxYMeleeRally = 0.1f;
    [SerializeField] private float minYMeleeRally = -4.15f;
    [SerializeField] private float maxYRangedRally = -0.25f;
    [SerializeField] private float minYRangedRally = -4.5f;
    //[SerializeField] private float startYMeleeRally = 0.1f;
    //[SerializeField] private float startYRangedRally = -1.25f;

    private float rallyJump = 1f;

    private bool hasReceivedReward = false;
    private bool isTutorial = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        Economy = new MatchEconomy(southCurrency: 300, northCurrency: 1000, baseIncomePerTick, currencyInterval,
            incomeMultiplier, incomeUpgradeCost);

        PauseManager.SetPaused(false);
    }

    public async void Start()
    {
        await StartGame();
    }

    void Update()
    {
#if UNITY_EDITOR
        if (debugApplyCurrency)
        {
            debugApplyCurrency = false;
            // Add the difference so CurrencyChanged fires and the HUD updates
            Economy.Add(Team.South, debugSetSouthCurrency - Economy.Get(Team.South));
        }
#endif

        if (isGameRunning)
        {
            Economy.Tick(Time.deltaTime);
        }
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
        UIManager.Instance.SetGameOver();
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
        UpdateGameState(gameOver, losingTeam);
        if (isTutorial)
        {
            UIManager.Instance.SetTutorialGameOver();
            return;
        }
        HandleUITransition();
        AwardReward();
    }

    public async Task StartGame()
    {
        Difficulty difficulty = Difficulties.Get(GameSettingsService.Instance.Difficulty);

        if (difficulty.Level == DifficultyLevel.Tutorial)
        {
            await StartTutorial();
        }

        WaveController.Instance.InitializeDifficulty(difficulty);
        UIManager.Instance.BindWaveChangeUpdateEvent();
        isGameRunning = true;
        TimerManager.Instance.StartTimer();

        StartCoroutine(CoroutineHelpers.DoAfterDelay(3f, () =>
        {
            WaveController.Instance.StartWaves();
        }));
    }

    public async Task StartTutorial()
    {
        isTutorial = true;
        UIManager.Instance.LockPauseMenuForTutorial();
        await GamePlayTutorialPresenter.Instance.RunTutorial();
    }

    public void SetTutorialOver()
    {
        gameUICanvas.SetActive(false);
        UIManager.Instance.SetTutorialOver();
        EndOfTutorial();
    }

    public void EndOfTutorial()
    {
        //move this to a reward manager or service later
        if (!SaveService.Instance.Current.Flags.HasReceivedLoginGift)
        {
            CurrencyManager.Instance.Add(CurrencyTypes.Cinders, 2000);
            SaveService.Instance.Current.Flags.HasReceivedLoginGift = true;
            SaveService.Instance.Save();
        }
        //Emit tutorial over
        TutorialFlow.Instance.SetGamePlayTutorialOver();
    }

    public void AwardReward()
    {
        if (hasReceivedReward)
            return;

        hasReceivedReward = true;
        //save score, throws error if not logged in
        LeaderboardService.Instance.AddScore(TimerManager.Instance.GetElapsedTimeInMiliseconds(), GameSettingsService.Instance.Difficulty);
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
        foreach (Transform rally in playerRangedRallies)
        {
            MoveRally(rally, minYRangedRally, maxYRangedRally, rallyJump);
        }
    }

    public void FallbackRangedRally()
    {
        foreach (Transform rally in playerRangedRallies)
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

#if UNITY_EDITOR
    // Ticking Apply outside play mode would save it into the scene and fire on the next play
    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            debugApplyCurrency = false;
        }
    }
#endif
}
