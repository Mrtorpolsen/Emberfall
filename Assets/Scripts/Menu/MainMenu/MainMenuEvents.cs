using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using static UnityEditor.Recorder.OutputPath;

public class MainMenuEvents : IUIScreenEvents
{
    [SerializeField] private UIDocument uIDocument;
    private VisualElement root;

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

    private PopupButtonDefinition nightmarePlayBtn = new PopupButtonDefinition
    {
        BtnText = "Nightmare",
        OnClick = () =>
        {
            GameSettingsService.Instance.SetDifficulty(DifficultyLevel.Nightmare);
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
        this.root = root;
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

        PopupManager.Instance.OpenChoicePopup(easyPlayBtn, mediumPlayBtn, hardPlayBtn, nightmarePlayBtn);
    }

    private void Btn_Offer3Clicked()
    {
        Debug.Log("Btn_Offer3 clicked...");
        var element = UtilityUIBinding.QRequired<VisualElement>(root, "Btn_Play");
        var element1 = UtilityUIBinding.QRequired<VisualElement>(root, "Img_WaveAttackContainer");
        var element2 = UtilityUIBinding.QRequired<VisualElement>(root, "Btn_Offer5");
        var element3 = UtilityUIBinding.QRequired<VisualElement>(root, "EasyHighScoreContainer");

        var popupData3 = new TutorialPopupData { description = "this is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yo", heading = "Testus maximus", 
            onClick = () => { TutorialController.Instance.Hide(); } };
        var popupData2 = new TutorialPopupData { description = "this is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yo", heading = "Testus maximus", 
            onClick = () => { TutorialController.Instance.ExecuteTutorialStep(popupData3, element3); } };
        var popupData1 = new TutorialPopupData { description = "this is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yo", heading = "Testus maximus", 
            onClick = () => { TutorialController.Instance.ExecuteTutorialStep(popupData2, element2); } };
        var popupData0 = new TutorialPopupData { description = "this is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yo", heading = "Testus maximus", 
            onClick = () => { TutorialController.Instance.ExecuteTutorialStep(popupData1, element1); } };
        var popupData = new TutorialPopupData { description = "this is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yothis is a test yo", heading = "Testus maximus", 
            onClick = () => { TutorialController.Instance.ExecuteTutorialStep(popupData1, element); } };

        TutorialController.Instance.ExecuteTutorialStep(popupData, null, true);

    }
}
