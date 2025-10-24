using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_Text survivalText;

    private void Start()
    {
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
