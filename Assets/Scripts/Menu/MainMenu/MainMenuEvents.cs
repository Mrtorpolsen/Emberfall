using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;
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

    //Maybe move to utility for easier use in every eventhandler
    public void BindEvents(VisualElement root)
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
