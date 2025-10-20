using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager main;

    public TMP_Text southCurrencyText;
    public TMP_Text southIncomeModifierText;

    public Transform north;
    public Transform south;

    public GameObject playerUnitBoundary;

    public Dictionary<Team, float> currency;

    [Header("References")]
    [SerializeField] public Canvas gameUI;
    [SerializeField] public SpawnMenuController spawnMenu;

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
        main = this;

        currency = new Dictionary<Team, float>()
        {
            { Team.North, 1000 },
            { Team.South, 300 },
        };

        UpdateCurrencyText();
        playerUnitBoundary = GameObject.FindGameObjectWithTag("PlayerUnitBarrier");
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
    }
    public void SubtractCurrency(Team team, float amount)
    {
        currency[team] -= amount;
        UpdateCurrencyText();
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
        southIncomeModifierText.text = "x" + incomeModifier.ToString();
    }
    public void SetGameOver(bool gameOver, Team team)
    {
        TimerManager.main.StopTimer();
        isGameOver = gameOver;
        winningTeam = team == Team.North ? Team.South : Team.North;
        gameUI.gameObject.SetActive(isGameOver);
        isGameRunning = false;
        spawnMenu.isOpen = false;
    }
}
