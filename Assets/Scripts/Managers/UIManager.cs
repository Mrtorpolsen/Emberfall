using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_Text text;
    [SerializeField] private TMP_Text buttonText;
    [SerializeField] private Button button;

    void Update()
    {
        if (GameManager.main == null)
        {
            Debug.LogError("GameManager.main is null!");
            return;
        }

        if (GameManager.main.isGameOver == true)
        {
            text.text = $"{GameManager.main.winningTeam.ToString()} won!";
            buttonText.text = "Play Again!";
        } else
        {
            text.text = "Geometry Wars";
            buttonText.text = "Play Game!";
        }
    }
    public void StartGame()
    {
        
        if(GameManager.main.isGameOver == false)
        {
            GameManager.main.gameUI.gameObject.SetActive(false);
            GameManager.main.isGameRunning = true;
            TimerManager.main.StartTimer();
            WaveManager.main.StartWaves();
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

}
