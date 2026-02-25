using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainMenuEvents : IUIScreenEvents
{
    [SerializeField] private UIDocument uIDocument;


    private readonly Dictionary<string, string> bindings = new()
    {
        { "Btn_Play", nameof(Btn_PlayClicked) },
        { "Btn_Offer3", nameof(Btn_Offer3Clicked) }
    };

    public void BindEvents(VisualElement root, IUIScreenController manager = null, IUIScreenView view = null)
    {
        UtilityUIBinding.BindEvents(root, this, bindings);
    }

    public void Cleanup()
    {
        UtilityUIBinding.Cleanup(this);
    }

    //USE NAMING CONVENTION OF BTN --- Btn_xxx so it can add Clicked behind
    private void Btn_PlayClicked()
    {
        Debug.Log("Play clicked loading Game...");
        SceneManager.LoadScene("Game");
    }

    private void Btn_Offer3Clicked()
    {
        Debug.Log("Btn_Offer3 clicked...");
    }
}
