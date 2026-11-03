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

    [Header("Rally Boundaries")]
    [Tooltip("Trigger outlining where the melee rally can be dragged")]
    [SerializeField] private Collider2D meleeRallyBoundary;
    [Tooltip("Trigger outlining where the ranged rallies can be dragged; can be the same as the melee one")]
    [SerializeField] private Collider2D rangedRallyBoundary;

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

    // The rally points RallyDragController can pick up
    public IEnumerable<Transform> DraggableRallies
    {
        get
        {
            yield return meleeRally;

            foreach (Transform rally in playerRangedRallies)
                yield return rally;
        }
    }

    // Moves a rally to target, kept inside its boundary: ClosestPoint returns target itself when it's inside,
    // otherwise the nearest point on the edge, so dragging past the edge slides along it
    public void MoveRally(Transform rally, Vector2 target)
    {
        Collider2D boundary = rally == meleeRally ? meleeRallyBoundary : rangedRallyBoundary;

        if (boundary != null)
        {
            target = boundary.ClosestPoint(target);
        }
        else
        {
            Debug.LogError($"No rally boundary assigned on GameManager for {rally.name}");
        }

        rally.position = new Vector3(target.x, target.y, rally.position.z);
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
