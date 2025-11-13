using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager main;

    [Header("References")]
    [SerializeField] private TMP_Text survivalText;
    [SerializeField] private TMP_Text incomeCostText;
    [SerializeField] public Canvas gameUI;

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
        if (main != null && main != this)
        {
            Destroy(main);
        }
        else
        {
            main = this;
        }
    }

    public void Initialize()
    {
        gameUI.gameObject.SetActive(true);
        SetSurvivalMessage();
    }

    public void SetSurvivalMessage()
    {
        survivalText.SetText($"Congratulations! You survived for {TimerManager.main.GetFormattedTime()}");
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ToggleSpawnButtonsActive(float playerCurrency)
    {
        //Figure out how to add dynamic values
        if(incomeBtn.interactable)
        {
            incomeBtn.interactable = (playerCurrency >= GameManager.main.incomeUpgradeCost);
            cavalierBtn.interactable = (playerCurrency >= 100);
            rangerBtn.interactable = (playerCurrency >= 75);
            fighterBtn.interactable = (playerCurrency >= 50);
        }
        else
        {
            incomeBtnBottom.interactable = (playerCurrency >= GameManager.main.incomeUpgradeCost);
            cavalierBtnBottom.interactable = (playerCurrency >= 100);
            rangerBtnBottom.interactable = (playerCurrency >= 75);
            fighterBtnBottom.interactable = (playerCurrency >= 50);
        }
    }

    public void UpdateIncomeCostText()
    {
        incomeCostText.text = GameManager.main.incomeUpgradeCost.ToString();
    }
}
