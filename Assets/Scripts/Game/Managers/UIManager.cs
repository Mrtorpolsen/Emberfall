using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager main;

    [Header("References")]
    [SerializeField] private TMP_Text survivalText;
    [SerializeField] public Canvas gameUI;
    [SerializeField] public Button incomeBtn;
    [SerializeField] public Button cavalierBtn;
    [SerializeField] public Button rangerBtn;
    [SerializeField] public Button fighterBtn;


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
        incomeBtn.interactable = (playerCurrency >= 200);
        cavalierBtn.interactable = (playerCurrency >= 100);
        rangerBtn.interactable = (playerCurrency >= 75);
        fighterBtn.interactable = (playerCurrency >= 50);
    }
}
