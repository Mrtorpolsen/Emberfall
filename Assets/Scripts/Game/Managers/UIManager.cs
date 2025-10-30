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


}
