using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashScreen : MonoBehaviour
{

    public static SplashScreen main;

    [Header("References")]
    [SerializeField] public GameObject splashPanel;

    private void Awake()
    {
        if (main == null)
        {
            main = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Show(string nextScene, float displayTime)
    {
        StartCoroutine(ShowRoutine(nextScene, displayTime));
    }

    private IEnumerator ShowRoutine(string nextScene, float displayTime)
    {
        splashPanel.SetActive(true);
        yield return new WaitForSeconds(displayTime);
        splashPanel.SetActive(false);
        SceneManager.LoadScene(nextScene);
    }
}
