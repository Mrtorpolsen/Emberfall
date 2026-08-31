using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainMenuEvents : IUIScreenEvents
{
    [SerializeField] private UIDocument uIDocument;

    private PopupButtonDefinition easyPlayBtn = new PopupButtonDefinition
    {
        BtnText = "Easy",
        OnClick = () =>
        {
            GameSettingsService.Instance.SetDifficulty(DifficultyLevel.Easy);
            SceneManager.LoadScene("Game");
        }
    };
    private PopupButtonDefinition mediumPlayBtn = new PopupButtonDefinition
    {
        BtnText = "Medium",
        OnClick = () =>
        {
            GameSettingsService.Instance.SetDifficulty(DifficultyLevel.Medium);
            SceneManager.LoadScene("Game");
        }
    };
    private PopupButtonDefinition hardPlayBtn = new PopupButtonDefinition
    {
        BtnText = "Hard",
        OnClick = () =>
        {
            GameSettingsService.Instance.SetDifficulty(DifficultyLevel.Hard);
            SceneManager.LoadScene("Game");
        }
    };

    private readonly Dictionary<string, string> bindings = new()
    {
        { "Btn_Play", nameof(Btn_PlayClicked) },
        { "Btn_Offer3", nameof(Btn_Offer3Clicked) }
    };

    public void BindEvents(VisualElement root, IUIScreenController controller = null, IUIScreenView view = null)
    {
        UtilityUIBinding.BindEvents(root, this, bindings);
    }

    public void Cleanup()
    {
        UtilityUIBinding.CleanupEvents(this);
    }

    //USE NAMING CONVENTION OF BTN --- Btn_xxx so it can add Clicked behind
    private void Btn_PlayClicked()
    {
        Debug.Log("Play clicked loading Game...");
        UnitStatsManager.Instance.RecalculateAllFinalStats();
        
        PopupManager.Instance.OpenChoicePopup(easyPlayBtn, mediumPlayBtn, hardPlayBtn);
    }

    private void Btn_Offer3Clicked()
    {
        Debug.Log("Btn_Offer3 clicked...");
    }
}
